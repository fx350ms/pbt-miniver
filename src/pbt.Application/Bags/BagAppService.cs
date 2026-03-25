using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using MathNet.Numerics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.Formula.Functions;
using pbt.Application.Cache;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.Commons.Dto;
using pbt.Core;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.EntityAuditLogs;
using pbt.EntityAuditLogs.Dto;
using pbt.OrderNumbers;
using pbt.Packages.Dto;
using pbt.ShippingRates;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace pbt.Warehouses
{
    [Authorize]
    [UnitOfWork(false)]
    [Audited]
    public class BagAppService :
        AsyncCrudAppService<Bag, BagDto, int, PagedResultRequestDto, BagDto, BagDto>, IBagAppService
    {
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Order, long> _orderRepository;
        private readonly IRepository<ShippingPartner> _shippingPartner;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly IRepository<CustomerFake, long> _customerFakeRepository;
        private readonly IShippingCostAppService _shippingCostService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IRepository<DeliveryRequestOrder, int> _deliveryRequestOrderRepository;
        private IRepository<DeliveryRequest, int> _deliveryRequestRepository;
        private IRepository<Warehouse, int> _warehouseRepository;
        private IRepository<Customer, long> _customerRepository;
        private readonly string[] _roles;
        private pbtAppSession _pbtAppSession;
        private readonly ConfigAppCacheService _cacheService;

        private readonly IEntityAuditLogApiClient _entityAuditLogApiClient;


        public BagAppService(
            IRepository<Bag, int> repository,
            IRepository<Package> packageRepository,
            IIdentityCodeAppService identityCodeAppService,
            IRepository<Warehouse, int> warehouseRepository,
            IRepository<Customer, long> customerRepository,
            IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IRepository<CustomerFake, long> customerFakeRepository,
            IRepository<Order, long> orderRepository,
            IRepository<DeliveryRequestOrder, int> deliveryRequestOrderRepository,
            IRepository<DeliveryRequest, int> deliveryRequestRepository,
            IShippingCostAppService shippingCostService,
            pbtAppSession pbtAppSession,
            IHttpContextAccessor httpContextAccessor,
            ConfigAppCacheService cacheService,
            IEntityAuditLogApiClient entityAuditLogApiClient

        )
            : base(repository)
        {
            _httpContextAccessor = httpContextAccessor;
            _packageRepository = packageRepository;
            _identityCodeAppService = identityCodeAppService;
            _warehouseRepository = warehouseRepository;
            _customerRepository = customerRepository;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _customerFakeRepository = customerFakeRepository;
            _orderRepository = orderRepository;
            _deliveryRequestOrderRepository = deliveryRequestOrderRepository;
            _deliveryRequestRepository = deliveryRequestRepository;
            _shippingCostService = shippingCostService;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value.ToLower())
                .ToArray();
            _pbtAppSession = pbtAppSession;
            _cacheService = cacheService;
            _entityAuditLogApiClient = entityAuditLogApiClient;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        [Authorize]
        public async Task<BagDto> CreateNewAsync(CreateUpdateBagDto input)
        {
            if (input.BagType == 1 && input.CustomerId == null)
            {
                throw new UserFriendlyException("Vui lòng chọn khách hàng.");
            }
            try
            {

                var currentWarehouseId = _pbtAppSession.WarehouseId;
                // kiểm tra có thể gộp kiện vào bao hay không
                var packageCheckCombineResult = await ConnectDb.GetListAsync<PackageCheckCanCombineToBagDto>(
                    "SP_Packages_CheckCanCombineToBag",
                    System.Data.CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@PackageIds", SqlDbType.NVarChar) { Value = string.Join(',', input.selectedPackages)},
                        new SqlParameter("@CreateWarehouseId" ,input.WarehouseCreateId),
                        new SqlParameter("@DestinationWarehouseId", input.WarehouseDestinationId),
                        new SqlParameter("@ShippingLineId",  input.ShippingType),
                        new SqlParameter("@Status", (int) PackageDeliveryStatusEnum.Initiate ),
                    }
                );

                if (packageCheckCombineResult != null && packageCheckCombineResult.Count > 0)
                {
                    var packageNumbers = "";
                    foreach (var item in packageCheckCombineResult)
                    {
                        packageNumbers += item.PackageNumber + ",";
                    }
                    throw new UserFriendlyException("Các kiện sau không hợp lệ, vui lòng kiểm tra lại: " + packageNumbers.Trim(','));
                }

                var dateTimeNow = DateTime.Now;

                string prefix = PrefixConst.BagCode;

                if (input.BagType == (int)BagTypeEnum.SeparateBag)
                {
                    // get customer bagCode prefix
                   await _customerRepository.FirstOrDefaultAsync(x => x.Id == input.CustomerId.Value).ContinueWith(customerTask =>
                    {
                        var customer = customerTask.Result;
                        if (customer != null && !string.IsNullOrEmpty(customer.BagPrefix))
                        {
                            prefix = customer.BagPrefix;
                        }
                    });
                }
            
                var identity = await _identityCodeAppService.GenerateNewSequentialNumberAsync(prefix);

                var bag = ObjectMapper.Map<BagDto>(input);
                bag.WarehouseStatus = (int?)WarehouseStatus.InStock;
                bag.CurrentWarehouseId = currentWarehouseId;
                bag.BagCode = $"{PrefixConst.BagCode}{prefix}{dateTimeNow.ToString("ddMM")}{identity.SequentialNumber.ToString("D3")}";
                bag.ShippingStatus = (int)BagShippingStatus.Initiated;
                bag = await base.CreateAsync(bag);

                // add log bag
                _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                    null
                    , bag
                    , "created"
                    , $"Tạo bao hàng tại kho Bằng Tường"
                    , true
                );

                var bagAuditLog = new EntityAuditLogDto()
                {
                    EntityId = bag.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Create.ToString(),

                    Title = $"Tạo bao hàng #{bag.Id} - {bag.BagCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bag),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);


                foreach (var packageId in input.selectedPackages)
                {
                    RemovePackageFromCache(packageId);
                }

                if (input.selectedPackages != null && input.selectedPackages.Count > 0)
                {
                    await ConnectDb.ExecuteNonQueryAsync("SP_Package_AddListToBag", System.Data.CommandType.StoredProcedure, new[]{
                        new SqlParameter("@PackageIds", SqlDbType.NVarChar) { Value = string.Join(',', input.selectedPackages) },
                        new SqlParameter("@bagId", SqlDbType.Int) { Value = bag.Id },
                        new SqlParameter("@PackageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.WaitingForShipping },
                        new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock }
                    });

                    var packages = await ConnectDb.GetListAsync<PackageDto>(
                        "SP_Packages_GetByIds",
                        System.Data.CommandType.StoredProcedure,
                        new[]
                        {
                            new SqlParameter("@PackageIds", SqlDbType.NVarChar) { Value = string.Join(',', input.selectedPackages)}
                        }
                    );

                    foreach (var _package in packages)
                    {
                        _package.BagId = bag.Id;
                        _package.BagNumber = bag.BagCode;
                        _package.ShippingPartnerId = bag.ShippingPartnerId;
                        _package.BaggingDate = DateTime.Now;

                        _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                             null
                            , _package
                            , "updated"
                            , $"Kiện đã được cho vào bao: {bag.BagCode}"
                            , true
                        );

                        _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                          null
                         , bag
                         , "updated"
                         , $"Kiện {_package.PackageNumber} đã được cho vào bao: {bag.BagCode}"
                         , true
                     );

                        var packageAuditLog = new EntityAuditLogDto()
                        {

                            EntityId = _package.Id.ToString(),
                            EntityType = nameof(Package),
                            MethodName = EntityAuditLogMethodName.Update.ToString(),

                            Title = $"Kiện hàng #{_package.Id} - {_package.PackageNumber} vào bao #{bag.Id} - {bag.BagCode} (tạo bao nhanh)",
                            UserId = _pbtAppSession.UserId,
                            UserName = _pbtAppSession.UserName,
                            Data = JsonConvert.SerializeObject(_package),

                        };
                        await _entityAuditLogApiClient.SendAsync(packageAuditLog);

                    }
                }
                //await UpdateBagBaseCode(bag);
                //await _shippingCostService.CalcOriginBagShippingCost(bag.Id);
                return bag;
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi tạo bao mới: " + ex.Message, ex);
                throw ex;
            }
        }


        public async Task UpdateBagBaseCode(BagDto bag)
        {
            try
            {
                var packages = await ConnectDb.GetListAsync<PackageDto>(
                    "SP_Packages_GetByBagId",
                    System.Data.CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@BagId", bag.Id)
                    }
                );

                decimal totalOriginPackageShippingCost = 0;
                foreach (var package in packages)
                {
                    int volum = (package.Height ?? 0 * package.Width ?? 0 * package.Length ?? 0) / 1000000;
                    package.OriginShippingCost = await _shippingCostService.CalcOriginalCost(bag.ShippingPartnerId.Value, package.ShippingLineId ?? 0,
                        package.WarehouseCreateId ?? 0, package.WarehouseDestinationId ?? 0, (package.Weight ?? 0), volum, package.UnitType);

                    await ConnectDb.ExecuteQueryAsync("SP_Packages_UpdateOriginShippingCost",
                          System.Data.CommandType.StoredProcedure,
                          new[]
                          {
                            new SqlParameter("@Id", package.Id),
                            new SqlParameter("@OriginShippingCost", package.OriginShippingCost ?? 0)
                          }
                      );
                    totalOriginPackageShippingCost += package.OriginShippingCost ?? 0;
                }

                decimal coverCost = 0;
                if (bag.IsWeightCover)
                {
                    coverCost = await _shippingCostService.CalcOriginalCost(bag.ShippingPartnerId.Value, bag.ShippingType,
                        bag.WarehouseCreateId ?? 0, bag.WarehouseDestinationId ?? 0, (bag.WeightCover ?? 0), 0, 1);
                }
                var bagCost = await _shippingCostService.CalcOriginalCost(bag.ShippingPartnerId.Value, bag.ShippingType,
                    bag.WarehouseCreateId ?? 0, bag.WarehouseDestinationId ?? 0, (bag.Weight ?? 0), bag.Volume ?? 0, 1);

                bag.TotalOriginPackageShippingCost = totalOriginPackageShippingCost;
                bag.TotalOriginShippingCost = bagCost + coverCost;
                await ConnectDb.ExecuteQueryAsync("SP_Bag_UpdateOriginShippingCost",
                         System.Data.CommandType.StoredProcedure,
                         new[]
                         {
                            new SqlParameter("@BagId", bag.Id),
                            new SqlParameter("@CoverCost", coverCost)
                         }
                     );
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi cập nhật lại giá gốc bao: " + e.Message, e);
                throw; // TODO handle exception
            }
        }


        /// <summary>
        /// Tạo bao tương tự
        /// </summary>
        /// <param name="bagId"></param>
        /// <returns></returns>
        /// 

        [Authorize]
        public async Task<BagDto> CreateSimilarBagAsync(int bagId)
        {
            try
            {
                string bagPrefix = PrefixConst.BagCode;
                var bagDto = await GetByIdAsync(bagId);

                if (bagDto == null)
                {
                    throw new UserFriendlyException("Không tìm thấy bao hàng để tạo tương tự.");
                }

                if (bagDto.CustomerId.HasValue && bagDto.CustomerId > 0)
                {
                    var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == bagDto.CustomerId.Value);
                    if (customer != null && !string.IsNullOrEmpty(customer.BagPrefix))
                    {
                        bagPrefix = customer.BagPrefix;
                    }
                }

                var identity = await _identityCodeAppService.GenerateNewSequentialNumberAsync(PrefixConst.BagCode);
                var bag = ObjectMapper.Map<Bag>(bagDto);
                bag.WarehouseStatus = (int?)WarehouseStatus.InStock;
                bag.BagCode = identity.Code;
                bag.ShippingStatus = (int)BagShippingStatus.Initiated;

                bag.TotalPackages = 0;
                bag.TotalFee = 0;
                bag.TotalPackageFee = 0;
                bag.TotalWeightPackage = 0;
                bag.Id = 0;
                bag.IsClosed = false;
                bag.IsWeightCover = false;
                bag.Weight = 0;
                bag.WeightPackage = 0;
                bag.WeightCoverFee = 0;
                bag.WeightCover = 0;

                var id = await Repository.InsertAndGetIdAsync(bag);
                bag.Id = id;

                bagDto = ObjectMapper.Map<BagDto>(bag);
                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bag.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Create.ToString(),

                    Title = $"Tạo bao hàng #{bag.Id} - {bag.BagCode} - từ chức năng tạo bao tương tự",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bagDto),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);
                return bagDto;
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi tạo bao tương tự: " + ex.Message, ex);
                throw ex;
            }
        }

        /// <summary>
        /// update bag
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<JsonResult> UpdateBagAsync(UpdateBagDto input)
        {
            var result = new JsonResult(new { Status = 200 }) { };
            try
            {
                var bag = await Repository.GetAsync(input.Id);

                if (bag == null)
                {
                    result.StatusCode = 404;
                    return result;
                }

                ObjectMapper.Map(input, bag);
                bag.Volume = input.Length * input.Width * input.Height / 1000000;

                await Repository.UpdateAsync(bag);

                //add log bag
                _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                   null
                    , bag
                    , "updated"
                    , $"Cập nhật thông tin bao: "
                    , false
                );


                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bag.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Cập nhật bao hàng #{bag.Id} - {bag.BagCode} ",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bag),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);

                result.Value = bag;
                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = 500;
                return result;
            }
        }


        /// <summary>
        /// Chức năng thêm kiện bì
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<JsonResult> UpdateCoverWeightAsync(UpdateWeightDto input)
        {
            try
            {
                var statusCode = new SqlParameter("@StatusCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                await ConnectDb.ExecuteNonQueryAsync("SP_Bags_UpdateCoverWeight", CommandType.StoredProcedure,
                       new[]
                       {
                        new SqlParameter(){ ParameterName = "@bagId", Value = input.Id , SqlDbType = SqlDbType.Int },
                        new SqlParameter(){ ParameterName = "@IsWeightCover", Value = input.IsWeightCover ?? false , SqlDbType = SqlDbType.Bit },
                        new SqlParameter(){ ParameterName = "@WeightCover", Value = input.WeightCover ?? 0 , SqlDbType = SqlDbType.Decimal },
                        new SqlParameter(){ ParameterName = "@CreatorUserId", Value = AbpSession.UserId.Value , SqlDbType = SqlDbType.BigInt },
                        statusCode,
                        message
                       });
                if ((int)statusCode.Value < 0)
                {
                    Logger.Error("Lỗi khi cập nhật cân nặng bì cho bao: " + message.Value.ToString());
                    return new JsonResult(new { Status = -500, Message = message.Value.ToString() });
                }
                var bag = await GetByIdAsync(input.Id);
                // add lo bag
                _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                    null
                    , bag
                    , "updated"
                    , $"Cập nhật cân nặng bì cho bao bao: {bag.BagCode}"
                    , true
                );

                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bag.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = input.IsWeightCover == true ? $"Cập nhật bì cho bao #{bag.Id} - {bag.BagCode} " : $"Bỏ bì cho bao #{bag.Id} - {bag.BagCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bag),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);
                return new JsonResult(new { Status = 200, Message = "Cập nhật cân nặng bì thành công." });
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi cập nhật cân nặng bì cho bao: " + e.Message, e);
                throw e;
            }
        }


        public async Task<CreateUpdateBagDto> GetAsync(int id)
        {
            var bag = await Repository.GetAsync(id);
            if (bag == null)
            {
                throw new UserFriendlyException($"Warehouse with Id {id} not found");
            }

            // Ánh xạ thực thể sang DTO
            var dto = ObjectMapper.Map<CreateUpdateBagDto>(bag);
            return dto;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<BagStampDto> GetForStampAsync(int id, bool printStamp = false)
        {
            // var bag =  await Repository.FirstOrDefaultAsync(b => b.Id == id);
            //.Include(b => b.WarehouseCreate)
            //.Include(b => b.WarehouseDestination)
            //.Include(x => x.Packages)

            var bag = await GetByIdAsync(id);

            if (bag == null)
            {
                throw new UserFriendlyException($"Warehouse with Id {id} not found");
            }

            // Ánh xạ thực thể sang DTO
            var dto = ObjectMapper.Map<BagStampDto>(bag);
            dto.WarehouseCreate = await GetWarehouseByIdAsync(bag.WarehouseCreateId.Value);
            dto.WarehouseDestination = await GetWarehouseByIdAsync(bag.WarehouseDestinationId.Value);
            var packageDto = await GetFirstPackageByBagIdAsync(bag.Id);
            dto.PackagesDtos = new() { packageDto };

            // add log bag
            _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                null
                , bag
                , "updated"
                , $"In tem"
                , true
            );

            return dto;
        }


        /// <summary>
        /// lấy danh customer
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<CustomerDto>> getCustomerFilter(PagedBagResultRequestDto input)
        {
            var data = await _customerRepository.GetAllAsync();

            var query = (await Repository.GetAllAsync())
                .Include(x => x.Customer)
                .Include(x => x.WarehouseCreate)
                .Include(x => x.WarehouseDestination)
                .Include(x => x.Packages)
                .WhereIf(input.pendingBag, u => !u.IsClosed);

            if ((_roles.Contains(RoleConstants.sale) || _roles.Contains(RoleConstants.saleadmin)) && !_roles.Contains(RoleConstants.admin))
            {
                var userId = AbpSession.UserId;
                List<long> saleIds = new List<long>();
                if (_roles.Contains(RoleConstants.saleadmin))
                {
                    saleIds = (await _customerRepository.GetAllAsync()).Where(x => x.SaleId == userId).Select(y => y.Id).ToList();
                }
                query = query.Where(x => x.Customer.SaleId == userId || saleIds.Contains(x.Customer.SaleId));
            }

            // get current user warehouse id
            var warehouseId = _pbtAppSession.WarehouseId;

            if (_roles.Contains(RoleConstants.warehouseVN) && !_roles.Contains(RoleConstants.admin))
            {
                query = query.Where(x => x.Customer != null && x.Customer.WarehouseId == warehouseId);
            }

            if (input.FilterType == "today")
            {
                query = query.Where(x => x.CreationTime.Date == DateTime.Now.Date);
            }
            if (input.Status > 0)
            {
                query = query.Where(x => x.ShippingStatus == input.Status);
            }

            if (input.StartCreateDate != null)
            {
                query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
            }
            if (input.EndCreateDate != null)
            {
                query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
            }

            if (input.StartImportDate != null)
            {
                query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value >= input.StartImportDate.Value);
            }
            if (input.EndImportDate != null)
            {
                query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value <= input.EndImportDate.Value);
            }

            if (input.StartExportDate != null)
            {
                query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value >= input.StartExportDate.Value);
            }
            if (input.EndExportDate != null)
            {
                query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value <= input.EndExportDate.Value);
            }

            if (input.WarehouseCreate > 0)
            {
                query = query.Where(x => x.WarehouseCreateId == input.WarehouseCreate);
            }
            if (input.WarehouseDestination > 0)
            {
                query = query.Where(x => x.WarehouseDestinationId == input.WarehouseDestination);
            }

            if (input.BagType > 0)
            {
                query = query.Where(x => x.BagType == input.BagType);
            }

            if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
            {
                query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
            }

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x => x.BagCode.ToUpper() == input.Keyword.ToUpper());
            }
            if (input.CustomerId != -1 && input.CustomerId != null)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId);
            }
            var bags = await query.ToListAsync();
            var customersIds = bags.Select(x => x.CustomerId ?? 0).ToList();
            return ObjectMapper.Map<List<CustomerDto>>(data.Where(x => customersIds.Contains(x.Id)));
        }
        /// <summary>
        /// lấy danh sách bao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<BagDto>> getBagsFilter(PagedBagResultRequestDto input)
        {
            try
            {
                var currentUserId = AbpSession.UserId;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var currentCustomerId = _pbtAppSession.CustomerId;
                var bagIdsDto = new List<BagIdDto>();
                var customerDtoIds = new List<CustomerIdDto>();
                var bagIds = new List<int>();
                // admin thì nhìn thấy tất cả kiện hàng

                var permissionCaseCheckResult = GetPermissionCheckerWithCustomerIds();

                var permissionCase = permissionCaseCheckResult.PermissionCase;
                var customerIds = permissionCaseCheckResult.CustomerIds;

                var query = base.CreateFilteredQuery(input);
                // Tạo query để lấy danh sách kiện hàng
                query = query
                     .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4 || permissionCase == 7) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value)) ||
                       (permissionCase == 5 && x.WarehouseCreateId == currentUserWarehouseId) ||
                       (permissionCase == 6 && x.WarehouseDestinationId == currentUserWarehouseId) ||
                        x.CustomerId == currentCustomerId
                    // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );

                if (input.FilterType == "today")
                {
                    query = query.Where(x => x.CreationTime.Date == DateTime.Now.Date);
                }
                if (input.Status > 0)
                {
                    query = query.Where(x => x.ShippingStatus == input.Status);
                }

                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value >= input.StartImportDate.Value);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value <= input.EndImportDate.Value);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value >= input.StartExportDate.Value);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value <= input.EndExportDate.Value);
                }

                if (input.WarehouseCreate > 0)
                {
                    query = query.Where(x => x.WarehouseCreateId == input.WarehouseCreate);
                }
                if (input.WarehouseDestination > 0)
                {
                    query = query.Where(x => x.WarehouseDestinationId == input.WarehouseDestination);
                }

                if (input.BagType > 0)
                {
                    query = query.Where(x => x.BagType == input.BagType);
                }

                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x => x.BagCode.ToUpper().Contains(input.Keyword.ToUpper()));
                }
                if (input.CustomerId != -1 && input.CustomerId != null)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }

                query = query.OrderByDescending(x => x.CreationTime);

                int total = query.Count();
                var totalBagWeight = query.Sum(x => (x.Weight ?? 0));
                var totalPackage = query.Sum(x => (x.TotalPackages ?? 0));
                query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
                query = query
                  .Include(x => x.Customer)
                  .Include(x => x.WarehouseCreate)
                  .Include(x => x.WarehouseDestination);

                var bags = await query.ToListAsync();

                var bagDtos = ObjectMapper.Map<List<BagDto>>(bags);

                return new PageResultBagDto()
                {
                    Items = bagDtos,
                    TotalCount = total,
                    TotalWeight = totalBagWeight,
                    TotalPackage = totalPackage
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách bao: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// lấy danh sách bao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<BagDto>> GetPagedList(PagedBagResultRequestDto input)
        {
            try
            {
                var currentUserId = AbpSession.UserId;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var currentCustomerId = _pbtAppSession.CustomerId;
                var bagIdsDto = new List<BagIdDto>();
                var customerDtoIds = new List<CustomerIdDto>();
                var bagIds = new List<int>();
                // admin thì nhìn thấy tất cả kiện hàng

                var permissionCaseCheckResult = GetPermissionCheckerWithCustomerIds();

                var permissionCase = permissionCaseCheckResult.PermissionCase;
                var customerIds = permissionCaseCheckResult.CustomerIds;

                var totalCountOutputParam = new SqlParameter()
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                var totalWeightOutputParam = new SqlParameter()
                {
                    ParameterName = "@TotalWeight",
                    SqlDbType = SqlDbType.Decimal,
                    Precision = 18,
                    Scale = 2,
                    Direction = ParameterDirection.Output
                };
                var totalPackageOutputParam = new SqlParameter()
                {
                    ParameterName = "@TotalPackage",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                var bagDtos = await ConnectDb.GetListAsync<BagDto>(
                    "SP_Bags_GetPagedList",
                    System.Data.CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@PermissionCase", permissionCase),
                        new SqlParameter("@CustomerIds", string.Join(',', customerIds)),
                        new SqlParameter("@CurrentWarehouseId", currentUserWarehouseId),
                        new SqlParameter("@CurrentCustomerId", currentCustomerId),
                        new SqlParameter("@FilterType", input.FilterType ?? (object)DBNull.Value),
                        new SqlParameter("@Status", input.Status),
                        new SqlParameter("@StartCreateDate", input.StartCreateDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndCreateDate", input.EndCreateDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartImportDate", input.StartImportDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndImportDate", input.EndImportDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartExportDate", input.StartExportDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndExportDate", input.EndExportDate ?? (object)DBNull.Value),
                        new SqlParameter("@WarehouseCreate", input.WarehouseCreate),
                        new SqlParameter("@WarehouseDestination", input.WarehouseDestination),
                        new SqlParameter("@BagType", input.BagType),
                        new SqlParameter("@ShippingPartnerId", input.ShippingPartnerId ?? -1),
                        new SqlParameter("@Keyword", input.Keyword ?? ""),
                        new SqlParameter("@CustomerId", input.CustomerId ?? -1),
                        new SqlParameter("@SkipCount", input.SkipCount),
                        new SqlParameter("@MaxResultCount", input.MaxResultCount),
                        new SqlParameter("@BagFilterPackageType", input.BagFilterPackageType),

                        totalCountOutputParam,
                        totalWeightOutputParam,
                        totalPackageOutputParam
                    }
                );

                return new PageResultBagDto()
                {
                    Items = bagDtos,
                    TotalCount = (int)totalCountOutputParam.Value,
                    TotalWeight = (decimal)totalWeightOutputParam.Value,
                    TotalPackage = (int)totalPackageOutputParam.Value
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách bao: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// lấy danh sách bao yêu cầu giao
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<BagDeliveryRequestDto>> GetBagByCustomer(long id)
        {
            try
            {

                var bags = await Repository.GetAllListAsync(x => x.CustomerId == id
                && x.BagType == (int)BagTypeEnum.SeparateBag
                && (!x.DeliveryNoteId.HasValue || x.DeliveryNoteId == 0)
                && x.ShippingStatus == (int)BagShippingStatus.GoToWarehouse
                );

                var bagDtos = ObjectMapper.Map<List<BagDeliveryRequestDto>>(bags);


                return bagDtos;
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi lấy danh sách bao yêu cầu giao: " + e.Message, e);
                throw;
            }
        }

        public async Task<List<BagReadyForCreateDeliveryNoteDto>> GetBagForCreateDeliveryNoteByCustomerId(long id)
        {
            try
            {
                var bagDtos = await ConnectDb.GetListAsync<BagReadyForCreateDeliveryNoteDto>(
                    "SP_Bags_GetForCreateDeliveryNoteByCustomerId",
                    System.Data.CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@CustomerId", id),
                        new SqlParameter("@WarehouseStatus", (int) WarehouseStatus.InStock),
                        new SqlParameter("@ShippingStatus", (int) BagShippingStatus.GoToWarehouse),
                        new SqlParameter("@CurrentWarehouseId", _pbtAppSession.WarehouseId)
                    }
                );

                return bagDtos;
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi lấy danh sách bao yêu cầu giao: " + e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<BagDto> GetBagDeliveryRequestById(int id)
        {
            try
            {
                var query = (await Repository.GetAllAsync())
                    .Include(x => x.Packages)
                    .FirstOrDefault(x => x.Id == id);
                return ObjectMapper.Map<BagDto>(query);
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi lấy bao yêu cầu giao theo Id: " + e.Message, e);
                throw new UserFriendlyException("Lỗi hệ thống");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bagCode"></param>
        /// <returns></returns>
        public async Task<BagDto> GetBagDeliveryRequestByCode(string bagCode)
        {
            try
            {
                var query = (await Repository.GetAllAsync())
                    .Include(x => x.Packages)
                    .FirstOrDefault(x => x.BagCode == bagCode);
                if (query == null)
                {
                    return null;
                }
                var bagDtos = ObjectMapper.Map<BagDto>(query);
                bagDtos.PackagesDtos = ObjectMapper.Map<List<PackageDto>>(query.Packages);
                return bagDtos;
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi lấy bao yêu cầu giao theo mã: " + e.Message, e);
                throw new UserFriendlyException("Lỗi hệ thống");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<PackageDto>> GetPackagesByBag(PagedPackageResultRequestDto input)
        {
            var query = (await _packageRepository.GetAllAsync())
                .Include(x => x.Bag)
                .Include(x => x.Order)
                .WhereIf(input.BagId.HasValue, u => u.BagId == input.BagId);
            var PackageDto = ObjectMapper.Map<List<PackageDto>>(query);
            foreach (var package in PackageDto)
            {
                if (package.Order != null)
                {
                    package.Order.CNWarehouseName =
                        (await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == package.Order.CNWarehouseId))
                        ?.Name;
                    package.Order.VNWarehouseName =
                        (await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == package.Order.VNWarehouseId))
                        ?.Name;
                }
            }
            var result = new PagedResultDto<PackageDto>()
            {
                Items = PackageDto,
                TotalCount = query.Count(),
            };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// 
        [Authorize]
        public async Task<JsonResult> AddPackageToBagAsync(AddPackageToBagRequestDto input)
        {
            try
            {
                var package = await GetByPackageCode(input.PackageCode);
                if (package == null)
                {
                    return new JsonResult(new { Status = -401, Message = "PackgeNotFound" });
                }

                var bag = await GetByIdAsync(input.BagId);
                if (package.WarehouseDestinationId != bag.WarehouseDestinationId)
                {
                    return new JsonResult(new { Status = -402, Message = "PackageWarehouseNotMatch" });
                }

                if (package.BagId != null)
                {
                    return new JsonResult(new { Status = -403, Message = "PackageWasInBag" });
                }

                // Nếu là bao riêng  thì kiểm tra khách hàng
                if (bag.BagType == (int)BagTypeEnum.SeparateBag && package.CustomerId != bag.CustomerId)
                {
                    return new JsonResult(new { Status = -404, Message = "PackageCustomerIsNotMatch" });
                }

                // kiểm tra kho đích
                if (package.WarehouseDestinationId != bag.WarehouseDestinationId)
                {
                    return new JsonResult(new { Status = -405, Message = "PackageWarehouseDestinationIsNotMatch" });
                }
                package.BagId = input.BagId;
                package.LastBagId = input.BagId;
                package.BagNumber = bag.BagCode;
                package.ShippingPartnerId = bag.ShippingPartnerId;
                package.BaggingDate = DateTime.Now;

                // Tính lại tiền của kiện khi quét vào bao vì lúc này là có dữ liệu chính xác từ kho TQ sang kho VN

                await ConnectDb.ExecuteNonQueryAsync("SP_Package_AddToBag", System.Data.CommandType.StoredProcedure, new[]{
                    new SqlParameter("@Id", SqlDbType.Int) { Value = package.Id },
                    new SqlParameter("@BagId", SqlDbType.Int) { Value = bag.Id },
                    new SqlParameter("@PackageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.WaitingForShipping },
                    new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock }
                    }
                );

                var packageAuditLog = new EntityAuditLogDto()
                {

                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Kiện hàng #{package.Id} - {package.PackageNumber} thêm vào bao #{bag.Id} - {bag.BagCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),
                };

                await _entityAuditLogApiClient.SendAsync(packageAuditLog);

                // Xóa khỏi danh sách tạo bao nhanh
                RemovePackageFromCache(package.Id, package.CreatorUserId.Value);


                //add log bag
                _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                  null
                    , bag
                    , "updated"
                    , $"Thêm kện vào bao: {package.PackageNumber}"
                    , true
                );

                // add log package
                _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                    null
                    , package
                    , "updated"
                    , $"Kiện được đóng vào bao: {bag.BagCode}"
                    , true
                );

                bag = await GetByIdAsync(input.BagId);
                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bag.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Bao #{bag.Id} - {bag.BagCode} được thêm kiện #{package.Id} - {package.PackageNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bag),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);


                return new JsonResult(new { Status = 200, data = bag.Weight, Message = "AddPackageSuccess" });
                //return new JsonResult(bag.Weight);
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi thêm kiện hàng vào bao: " + e.Message, e);
                throw new UserFriendlyException(e.Message);
            }
        }


        [HttpPost]
        // update package and bag
        public async Task<JsonResult> RemovePackageFromBagAsync(AddPackageToBagRequestDto input)
        {
            try
            {
                var package = await GetPackageByIdAsync(input.PackageId);
                var bag = await GetByIdAsync(input.BagId);
                if (package == null)
                {
                    return new JsonResult(new { Status = -401, Message = "PackgeNotFound" });
                }

                if (package.BagId == null)
                {
                    return new JsonResult(new { Status = -402, Message = "PackageNotInBag" });
                }
                if (package.BagId != input.BagId)
                {
                    return new JsonResult(new { Status = -403, Message = "PackageInAnotherBag" });
                }

                package.BagId = null;
                package.BagNumber = string.Empty;
                package.ShippingStatus = (int)PackageDeliveryStatusEnum.Initiate;


                var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Package_UnBag", CommandType.StoredProcedure,
                    new[]
                    {
                    new SqlParameter("@id", input.PackageId)
                    }
                );

                //  await Repository.UpdateAsync(bag);
                var bagDto = ObjectMapper.Map<BagDto>(bag);


                var packageAuditLog = new EntityAuditLogDto()
                {
                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Kiện #{package.Id} - {package.PackageNumber} được bỏ khỏi bao #{bag.Id} - {bag.BagCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),
                };

                await _entityAuditLogApiClient.SendAsync(packageAuditLog);

                bag = await GetByIdAsync(input.BagId);

                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bag.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Bao #{bag.Id} - {bag.BagCode} gỡ kiện #{package.PackageNumber} - {package.TrackingNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bag),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);

                // add log package
                _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                    null
                    , package
                    , "updated"
                    , $"Kiện được xóa khỏi bao: {bag.BagCode}"
                    , true
                );

                // add log bag
                _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                    null
                    , bag
                    , "updated"
                    , $"Xóa kiện khỏi bao: {package.PackageNumber}"
                    , true
                );

                return new JsonResult(new { Status = 200, data = bagDto, Message = "RemovePackageSuccess" });
            }
            catch (Exception e)
            {
                Logger.Error("Lỗi khi xóa kiện hàng khỏi bao: " + e.Message, e);
                throw new UserFriendlyException("Có lỗi xảy ra khi xóa kiện hàng khỏi bao: " + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<JsonResult> BaggingAsync(BaggingDto input)
        {
            var result = new JsonResult(new { Status = 200 }) { };
            try
            {
                var bag = await Repository.GetAsync(input.BagId);
                bag.IsClosed = true;

                await ConnectDb.ExecuteNonQueryAsync("SP_Bag_Close", CommandType.StoredProcedure,
                    new[]
                    {
                            new SqlParameter("@bagId", input.BagId),
                            new SqlParameter("@bagStatus", (int)BagShippingStatus.WaitingForShipping),
                            new SqlParameter("@packageStatus", (int)PackageDeliveryStatusEnum.WaitingForShipping)
                    });
                // add log bag

                var bagDto = ObjectMapper.Map<BagDto>(bag);

                _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                  null
                  , bagDto
                  , "updated"
                  , $"Kết thúc đóng bao"
                  , true
              );

                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bagDto.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Bao #{bagDto.Id} - {bagDto.BagCode} đã đóng",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bagDto),

                };

                result.Value = ObjectMapper.Map<BagDto>(bag);
                await _entityAuditLogApiClient.SendAsync(bagAuditLog);

                return result;
            }
            catch (Exception e)
            {
                result.StatusCode = 500;
                return result;
            }
        }

        public async Task<List<BagDto>> GetFull()
        {
            var data = await Repository.GetAllAsync();
            if (data == null)
            {
                throw new UserFriendlyException($"Bag is empty");
            }

            return ObjectMapper.Map<List<BagDto>>(data);
        }

        public async Task<List<BagDto>> GetBagsTodayAsync()
        {
            var data = await ConnectDb.GetListAsync<BagDto>(
                "SP_Bags_GetAllToday",
                CommandType.StoredProcedure
            );

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<List<BagClosedDto>> GetBagsClosedAsync()
        {
            var data = (await Repository.GetAllAsync())
                .Include(x => x.Packages)
                .Where(x => x.IsClosed).ToList()
                .Select(x => new BagClosedDto
                {
                    Id = x.Id,
                    BagCode = x.BagCode,
                    Weight = x.Weight,
                    TotalPackage = x.Packages.Count
                })
                .ToList();
            if (data == null)
            {
                throw new UserFriendlyException($"Bag is empty");
            }

            return data;
        }


        public async Task<List<PackageDetailDto>> GetListDetailAsync(string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    throw new UserFriendlyException($"Package with Id {ids} not found");
                }

                var packagesIds = ids.Split(",").Select(int.Parse).ToList();

                var packages = await _packageRepository.GetAllListAsync(x => packagesIds.Contains(x.Id) && x.BagId == null);

                var packagesDto = ObjectMapper.Map<List<PackageDetailDto>>(packages);

                return packagesDto;
            }
            catch (Exception ex)
            {

                Logger.Error("Lỗi khi lấy danh sách chi tiết kiện hàng: " + ex.Message, ex);
                throw;
            }
        }



        /// <summary>
        /// lấy danh sách bao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<BagDto>> GetDataForDownload(BagDownloadFileRequestDto input)
        {
            try
            {
                // convert input.CreateDate to from,to date
                //DateTime fromDate = Convert.ToDateTime(input.CreateDate.Split(" - ")[0]);
                //DateTime toDate = Convert.ToDateTime(input.CreateDate.Split(" - ")[1]);

                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var permissionCaseCheckResult = GetPermissionCheckerWithCustomerIds();

                var permissionCase = permissionCaseCheckResult.PermissionCase;
                var customerIds = permissionCaseCheckResult.CustomerIds;

                var query = base.CreateFilteredQuery(input);
                // Tạo query để lấy danh sách kiện hàng
                query = query
                     .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4 || permissionCase == 7) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value)) ||
                       (permissionCase == 5 && x.WarehouseCreateId == currentUserWarehouseId) ||
                       (permissionCase == 6 && x.WarehouseDestinationId == currentUserWarehouseId)
                    // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );


                //var query = (await Repository.GetAllAsync())

                //    .WhereIf(input.pendingBag, u => !u.IsClosed);

                if (input.FilterType == "today")
                {
                    query = query.Where(x => x.CreationTime.Date == DateTime.Now.Date);
                }


                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
                }

                if (input.Status > 0)
                {
                    query = query.Where(x => x.ShippingStatus == input.Status);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value >= input.StartImportDate.Value);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value.Date <= input.EndImportDate.Value.Date);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value >= input.StartExportDate.Value);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value <= input.EndExportDate.Value);
                }

                if (input.WarehouseCreate > 0)
                {
                    query = query.Where(x => x.WarehouseCreateId == input.WarehouseCreate);
                }
                if (input.WarehouseDestination > 0)
                {
                    query = query.Where(x => x.WarehouseDestinationId == input.WarehouseDestination);
                }

                if (input.BagType > 0)
                {
                    query = query.Where(x => x.BagType == input.BagType);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x => x.BagCode.ToUpper().Contains(input.Keyword.ToUpper()));
                }
                if (input.CustomerId != -1 && input.CustomerId != null)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }
                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                query = query.OrderByDescending(x => x.CreationTime);

                int total = query.Count();

                query = query.Include(x => x.Customer)
                    .Include(x => x.WarehouseCreate)
                    .Include(x => x.WarehouseDestination)
                    .Include(x => x.ShippingPartner)
                    .Include(x => x.Packages);
                var bags = await query.ToListAsync();

                var bagDtos = ObjectMapper.Map<List<BagDto>>(bags);

                foreach (var bagDto in bagDtos)
                {
                    var packages = bags.FirstOrDefault(b => b.Id == bagDto.Id)?.Packages ?? new List<Package>();
                    var totalWeight = packages.Sum(p => p.Weight) ?? 0;
                    var totalPackages = packages.Count;
                    bagDto.TotalPackages = totalPackages;
                    bagDto.WeightPackages = totalWeight;
                    bagDto.ShippingPartnerName = bagDto.ShippingPartner?.Name ?? "";
                }

                return new PagedResultDto<BagDto>()
                {
                    Items = bagDtos,
                    TotalCount = total
                };
            }
            catch (Exception e)
            {
                throw;
            }
        }


        /// <summary>
        /// lấy danh sách bao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<PackageDto>> GetPackageDataForDownload(BagDownloadFileRequestDto input)
        {
            try
            {

                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var permissionCaseCheckResult = GetPermissionCheckerWithCustomerIds();

                var permissionCase = permissionCaseCheckResult.PermissionCase;
                var customerIds = permissionCaseCheckResult.CustomerIds;

                var query = base.CreateFilteredQuery(input);
                // Tạo query để lấy danh sách kiện hàng

                if (input.FilterType == "today")
                {
                    query = query.Where(x => x.CreationTime.Date == DateTime.Now.Date);
                }

                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
                }

                if (input.Status > 0)
                {
                    query = query.Where(x => x.ShippingStatus == input.Status);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value >= input.StartImportDate.Value);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value.Date <= input.EndImportDate.Value.Date);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value >= input.StartExportDate.Value);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value <= input.EndExportDate.Value);
                }

                if (input.WarehouseCreate > 0)
                {
                    query = query.Where(x => x.WarehouseCreateId == input.WarehouseCreate);
                }
                if (input.WarehouseDestination > 0)
                {
                    query = query.Where(x => x.WarehouseDestinationId == input.WarehouseDestination);
                }

                if (input.BagType > 0)
                {
                    query = query.Where(x => x.BagType == input.BagType);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x => x.BagCode.ToUpper().Contains(input.Keyword.ToUpper()));
                }

                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                if (input.CustomerId.HasValue && input.CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }
                else
                {
                }

                var bagIds = query.Select(x => x.Id).ToList();
                var packages = await (await _packageRepository.GetAllAsync())
                     .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value)) ||
                       (permissionCase == 5 && x.WarehouseCreateId == currentUserWarehouseId) ||
                       (permissionCase == 6 && x.WarehouseDestinationId == currentUserWarehouseId)
                    // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    )
                    .Include(x => x.Customer)
                    .Where(x => x.BagId.HasValue && bagIds.Contains(x.BagId ?? 0))
                    .ToListAsync();

                return ObjectMapper.Map<List<PackageDto>>(packages);
            }
            catch (Exception ex)
            {
                Logger.Error("GetPackageDataForDownload", ex);
                throw ex;
            }
        }


        /// <summary>
        /// lấy danh sách bao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ListResultDto<PackageDto>> GetDataManifestForDownload(BagDownloadFileRequestDto input)
        {
            try
            {
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var permissionCaseCheckResult = GetPermissionCheckerWithCustomerIds();

                var permissionCase = permissionCaseCheckResult.PermissionCase;
                var customerIds = permissionCaseCheckResult.CustomerIds;

                var query = base.CreateFilteredQuery(input);
                // Tạo query để lấy danh sách kiện hàng
                query = query
                     .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4 || permissionCase == 7) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value)) ||
                       (permissionCase == 5 && x.WarehouseCreateId == currentUserWarehouseId) ||
                       (permissionCase == 6 && x.WarehouseDestinationId == currentUserWarehouseId)
                    // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );

                query = query
                   .Include(x => x.Customer)
                   .Include(x => x.WarehouseCreate)
                   .Include(x => x.WarehouseDestination)
                   .Include(x => x.Packages)
                   .WhereIf(input.pendingBag, u => !u.IsClosed);

                if (input.FilterType == "today")
                {
                    query = query.Where(x => x.CreationTime.Date == DateTime.Now.Date);
                }

                if (input.Status > 0)
                {
                    query = query.Where(x => x.ShippingStatus == input.Status);
                }

                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value >= input.StartImportDate.Value);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value <= input.EndImportDate.Value);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value >= input.StartExportDate.Value);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value <= input.EndExportDate.Value);
                }

                if (input.WarehouseCreate > 0)
                {
                    query = query.Where(x => x.WarehouseCreateId == input.WarehouseCreate);
                }
                if (input.WarehouseDestination > 0)
                {
                    query = query.Where(x => x.WarehouseDestinationId == input.WarehouseDestination);
                }

                if (input.BagType > 0)
                {
                    query = query.Where(x => x.BagType == input.BagType);
                }

                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x => x.BagCode.ToUpper().Contains(input.Keyword.ToUpper()));
                }

                query = query.OrderByDescending(x => x.CreationTime);

                int total = query.Count();

                var bags = await query.ToListAsync();

                var bagIds = bags.Select(x => x.Id).ToList();
                var packageQuery = _packageRepository.GetAllIncluding(x => x.Bag)
                    .Include(x => x.Customer)
                    .Include(x => x.Order)
                    .Where(x => bagIds.Contains(x.BagId ?? 0));
                if (input.IsExcel)
                {
                    packageQuery = packageQuery.Where(x => !x.IsRepresentForWeightCover);
                }

                var packages = await packageQuery.ToListAsync();
                // Lấy danh sách customerFakeId
                var customerFakeIds = packages.Select(x => x.CustomerFakeId).Distinct().ToList();
                var customerFakeList = await _customerFakeRepository.GetAllListAsync(x => customerFakeIds.Contains(x.Id));

                var packageDtos = ObjectMapper.Map<List<PackageDto>>(packages);

                foreach (var packageDto in packageDtos)
                {
                    var customerFake = customerFakeList.FirstOrDefault(x => x.Id == packageDto.CustomerFakeId);
                    if (customerFake != null)
                    {
                        packageDto.CustomerFakeName = customerFake.FullName;
                        packageDto.CustomerFakePhone = customerFake.PhoneNumber;
                        packageDto.CustomerFakeAddress = customerFake.Address;
                    }
                }
                return new PagedResultDto<PackageDto>()
                {
                    Items = packageDtos,
                    TotalCount = total
                };

            }
            catch (Exception ex)
            {
                Logger.Error("GetDataManifestForDownload", ex);
                throw ex;
            }
        }

        public async Task<PagedResultDto<BagViewForPartnerDto>> GetBagsForPartnerAsync(PagedBagResultRequestDto input)
        {
            try
            {
                var currentUserId = AbpSession.UserId;

                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var checkPermissionResult = GetPermissionCheckerWithCustomerIds();

                if (checkPermissionResult.PermissionCase <= 0)
                {
                    return new PagedResultDto<BagViewForPartnerDto>()
                    {
                        Items = new List<BagViewForPartnerDto>(),
                        TotalCount = 0
                    };
                }

                var permissionCase = checkPermissionResult.PermissionCase;
                var customerIds = checkPermissionResult.CustomerIds;

                var query = (await Repository.GetAllAsync()).Where(x =>
                   permissionCase == 1 ||
                  ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4) && x.CustomerId.HasValue && x.CustomerId > 0 && customerIds.Contains(x.CustomerId.Value))
                   );

                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    var keyword = input.Keyword.Trim().ToUpper();
                    query = query.Where(x => x.BagCode.ToUpper().Contains(keyword) || x.Receiver.Contains(keyword));
                }
                if (input.StartExportDate.HasValue)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value.Date >= input.StartExportDate.Value.Date);
                }
                if (input.EndExportDate.HasValue)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value.Date <= input.EndExportDate.Value.Date);
                }

                if (input.StartExportDateVN.HasValue)
                {
                    query = query.Where(x => x.ExportDateVn.HasValue && x.ExportDateVn.Value.Date >= input.StartExportDateVN.Value.Date);
                }
                if (input.EndExportDateVN.HasValue)
                {
                    query = query.Where(x => x.ExportDateVn.HasValue && x.ExportDateVn.Value.Date <= input.EndExportDateVN.Value.Date);
                }

                if (input.StartImportDate.HasValue)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value.Date >= input.StartImportDate.Value.Date);
                }

                if (input.EndImportDate.HasValue)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value.Date <= input.EndImportDate.Value.Date);
                }

                if (input.StartCreateDate.HasValue)
                {
                    query = query.Where(x => x.CreationTime.Date >= input.StartCreateDate.Value.Date);
                }
                if (input.EndCreateDate.HasValue)
                {
                    query = query.Where(x => x.CreationTime.Date <= input.EndCreateDate.Value.Date);
                }

                if (input.ShippingLine.HasValue && input.ShippingLine > 0)
                {
                    query = query.Where(u => u.ShippingType == input.ShippingLine);
                }

                if (input.FromWeight.HasValue && input.FromWeight > 0)
                {
                    query = query.Where(u => u.Weight >= input.FromWeight);
                }
                if (input.ToWeight.HasValue && input.ToWeight > 0)
                {
                    query = query.Where(u => u.Weight <= input.ToWeight);
                }

                if (input.Status > 0)
                {
                    query = query.Where(u => u.ShippingStatus == input.Status);
                }

                if (input.WarehouseStatus.HasValue && input.WarehouseStatus > 0)
                {
                    query = query.Where(u => u.WarehouseStatus == input.WarehouseStatus);
                }

                if (input.CustomerId.HasValue && input.CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }

                if (input.BagType > 0)
                {
                    query = query.Where(x => x.BagType == input.BagType);
                }

                query = query.Include(x => x.ShippingPartner);
                var totalCount = query.Count();

                var bags = input.IsExcel
                    ? await query.OrderByDescending(x => x.Id).ToListAsync()
                    : await query.OrderByDescending(x => x.Id)
                        .Skip(input.SkipCount)
                        .Take(input.MaxResultCount)
                        .ToListAsync();

                var bagDtos = new List<BagViewForPartnerDto>();
                for (int i = 0; i < bags.Count; i++)
                {
                    var bag = bags[i];
                    bagDtos.Add(new BagViewForPartnerDto
                    {
                        ExportDateCN = bag.ExportDate,
                        ExportDateVN = bag.ExportDateVn,
                        Receiver = bag.Receiver,
                        BagCode = bag.BagCode,
                        Weight = bag.Weight,
                        Volume = bag.Volume,
                        Length = bag.Length,
                        Width = bag.Width,
                        Height = bag.Height,
                        ShippingPartnerName = bag.ShippingPartner.Name,
                        TotalPackages = bag.TotalPackages,

                        Characteristic = bag.IsSolution ? "Dung dịch" :
                                        bag.IsWoodSealing ? "Đóng gỗ" :
                                        bag.IsFakeGoods ? "Hàng giả" :
                                        bag.IsOtherFeature ? bag.otherReason : "",
                        ImportDateHN = bag.ImportDate,
                        Note = bag.Note
                    });
                }

                return new PagedResultDto<BagViewForPartnerDto>
                {
                    Items = bagDtos,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Có lỗi khi lấy danh sách bao - đối tác", ex);

                return new PagedResultDto<BagViewForPartnerDto>
                {
                    Items = new List<BagViewForPartnerDto>(),
                    TotalCount = 0
                };
            }

        }

        // Add this method to BagAppService
        public async Task<int> CountPackagesByBagIdAsync(int bagId)
        {
            var count = await _packageRepository.GetAll().CountAsync(x => x.BagId == bagId);
            return count;
        }

        public async Task<int> UpdateImportStatus(long bagId, int bagStatus, int warehouseStatus, int orderStatus, int packageStatus)
        {
            try
            {
                var result = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                  "EXEC SP_Bag_UpdateImportStatus  @bagId, @bagStatus, @warehouseStatus, @orderStatus, @packageStatus",
                    new SqlParameter("@bagId", bagId),
                    new SqlParameter("@bagStatus", bagStatus),
                    new SqlParameter("@warehouseStatus", warehouseStatus),
                    new SqlParameter("@orderStatus", orderStatus),
                    new SqlParameter("@packageStatus", packageStatus)
               );
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateImportStatus", ex);
                throw new UserFriendlyException($"Error updating import status: {ex.Message}");
            }
        }

        [Authorize]
        public async Task<object> DeleteBagAsync(int bagId)
        {

            var bagInfo = Repository.Get(bagId);
            if (bagInfo == null)
            {
                return new
                {
                    Success = false,
                    Message = "Bag not found"
                };
            }

            if (bagInfo.ShippingStatus == (int)BagShippingStatus.WaitingForShipping
                || bagInfo.ShippingStatus == (int)BagShippingStatus.InTransit
                || bagInfo.ShippingStatus == (int)BagShippingStatus.Initiated)
            {
                var countPackage = await _packageRepository.GetAll().CountAsync(x => x.BagId == bagId);
                if (countPackage > 0)
                {
                    return new
                    {
                        Success = false,
                        Message = "Không thể xóa bao khi có kiện hàng trong bao"
                    };
                }

                await DeleteAsync(new EntityDto<int>(bagId));

                // Bao bị xóa, ghi log
                var bagDto = ObjectMapper.Map<BagDto>(bagInfo);
                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bagDto.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Xóa bao #{bagDto.Id} - {bagDto.BagCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bagDto),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);

                return new
                {
                    Success = true,
                    Message = "Xóa bao thành công"
                };
            }
            else
            {
                return new
                {
                    Success = false,
                    Message = "Chỉ xóa được bao khi đang ở kho TQ"
                };
            }

        }


        public async Task<List<BagDto>> GetByDeliveryNoteIdAsync(int deliveryNoteId)
        {
            var bags = await (await Repository.GetAllAsync())
                .Where(x => x.DeliveryNoteId == deliveryNoteId)
                .Include(x => x.DeliveryNote)
                .Select(x => new BagDto()
                {
                    Id = x.Id,
                    BagCode = x.BagCode,
                    Weight = x.Weight,
                    Volume = x.Volume,
                    Length = x.Length,
                    Width = x.Width,
                    BagSize = x.BagSize,
                    Height = x.Height,
                    DeliveryNoteCode = x.DeliveryNote.DeliveryNoteCode,
                    Note = x.Note,
                    CustomerId = x.CustomerId
                })
                .ToListAsync();

            return bags;
            ///return ObjectMapper.Map<List<BagDto>>(bags);

        }

        /// <summary>
        /// Lấy danh sách bao theo danh sách ID
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<BagDto>> GetByIdsAsync(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<BagDto>();
            }
            var bags = await (await Repository.GetAllAsync())
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
            return ObjectMapper.Map<List<BagDto>>(bags);
        }

        public async Task<List<BagDto>> GetBagsInVietnamWarehouseAsync(long customerId, string excludedBagIds)
        {
            var query = Repository.GetAll()
                .Where(b => b.CustomerId == customerId
                && b.ShippingStatus == (int)BagShippingStatus.GoToWarehouse
                && !b.DeliveryNoteId.HasValue
                );

            if (!string.IsNullOrEmpty(excludedBagIds))
            {
                var ids = excludedBagIds.Split(',')
                    .Select(id => int.TryParse(id, out var parsedId) ? parsedId : 0)
                    .Where(id => id > 0)
                    .ToList();
                query = query.Where(b => !ids.Contains(b.Id));
            }

            var bags = await query.ToListAsync();
            return ObjectMapper.Map<List<BagDto>>(bags);
        }


        [HttpGet]
        public async Task<List<OptionItemDto>> GetSelectableBagsForPackageAsync(string q, int shippingLine, int customerId)
        {

            //            CREATE OR ALTER PROCEDURE dbo.SP_Bags_GetForAddPackage
            //(
            //    @ShippingType INT,
            //    @CustomerId BIGINT,
            //    @Keyword NVARCHAR(200) = NULL
            //)

            var data = await ConnectDb.GetListAsync<BagItemForSelectDto>("SP_Bags_GetForAddPackage",
                CommandType.StoredProcedure,
                new[] {
                new SqlParameter("@ShippingType", shippingLine),
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@Keyword", string.IsNullOrEmpty(q) ? (object)DBNull.Value : q)
                });
            return data.Select(b => new OptionItemDto
            {
                id = b.Id.ToString(),
                text = $"{b.BagCode} - {((BagShippingStatus)b.ShippingStatus).GetDescription()}"
            }).ToList();

            //var query = Repository.GetAll()
            //    .Where(b => !b.IsClosed)
            //    .Where(b => b.ShippingType == shippingLine)
            //    .Where(b => (b.CustomerId == customerId && b.BagType == (int)BagTypeEnum.SeparateBag) // bao riêng
            //     || (b.BagType == (int)BagTypeEnum.InclusiveBag) // bao ghép
            //    );

            //// Keyword search
            //if (!string.IsNullOrEmpty(q))
            //{
            //    var keyword = q.Trim().ToUpper();
            //    query = query.Where(b => b.BagCode.ToUpper().Contains(keyword) || b.Receiver.Contains(keyword));
            //}

            //var bags = await query.OrderByDescending(u => u.Id).ToListAsync();
            //return bags.Select(b => new OptionItemDto
            //{
            //    id = b.Id.ToString(),
            //    text = $"{b.BagCode} - {((BagShippingStatus)b.ShippingStatus).GetDescription()}"
            //}).ToList();
        }


        /// <summary>
        /// lấy danh sách bao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<OptionItemDto>> GetCustomersByDateSelect(PagedBagResultRequestDto input)
        {
            try
            {
                var query = (await Repository.GetAllAsync())
                    .Include(x => x.Packages)
                    .WhereIf(input.pendingBag, u => !u.IsClosed);


                if (input.Status > 0)
                {
                    query = query.Where(x => x.ShippingStatus == input.Status);
                }

                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value >= input.StartImportDate.Value);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.ImportDate.HasValue && x.ImportDate.Value <= input.EndImportDate.Value);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value >= input.StartExportDate.Value);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.ExportDate.HasValue && x.ExportDate.Value <= input.EndExportDate.Value);
                }

                if (input.WarehouseCreate > 0)
                {
                    query = query.Where(x => x.WarehouseCreateId == input.WarehouseCreate);
                }
                if (input.WarehouseDestination > 0)
                {
                    query = query.Where(x => x.WarehouseDestinationId == input.WarehouseDestination);
                }

                if (input.BagType > 0)
                {
                    query = query.Where(x => x.BagType == input.BagType);
                }

                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                var customerByPackageIds = query.SelectMany(x => x.Packages)
                    .Where(p => p.CustomerId.HasValue)
                    .Select(p => p.CustomerId.Value)
                    .Distinct()
                    .ToList();

                if (customerByPackageIds != null && customerByPackageIds.Count > 0)
                {
                    var customers = (await _customerRepository.GetAllAsync())
                        .Where(x => customerByPackageIds.Contains(x.Id))
                        .Where(x => string.IsNullOrEmpty(input.Keyword) || x.Username.ToUpper().Contains(input.Keyword.ToUpper()))
                        .Select(x => new OptionItemDto
                        {
                            id = x.Id.ToString(),
                            text = x.FullName,

                        })
                        .ToList();
                    return customers;
                }
            }
            catch (Exception ex)
            {
            }

            return new List<OptionItemDto>();
        }


        /// <summary>
        /// Lấy danh sách bao và kiện đã về đến kho theo khách hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<BagPackageDto>> GetListByCustomerAndWarehouseAsync(BagPackageTransferRequestDto input)

        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@CustomerId", input.CustomerId),
                    new SqlParameter("@BagStatus", (int) BagShippingStatus.GoToWarehouse),
                    new SqlParameter("@PackageStatus",  (int) PackageDeliveryStatusEnum.InWarehouseVN),
                    new SqlParameter("@WarehouseDestination", input.WarehouseId )
                };

                var result = await ConnectDb.GetListAsync<BagPackageDto>(
                    "SP_BagsPackages_GetListByCustomerAndStatus",
                    CommandType.StoredProcedure,
                    parameters
                );
                return new PagedResultDto<BagPackageDto>()
                {
                    Items = result,
                    TotalCount = result.Count
                };
            }
            catch (Exception ex)
            {
                Logger.Error("GetListByCustomerAndWarehouseAsync", ex);
                throw ex;
            }

        }

        private async Task<PackageDto> GetByPackageCode(string packageCode)
        {
            var data = await ConnectDb.GetItemAsync<PackageDto>(
               "SP_Packages_GetByPackageNumber",
               CommandType.StoredProcedure,
               new[]
               {
                    new SqlParameter("@PackageNumber", packageCode)
               });

            return data;
        }

        [Authorize]
        // Method to remove a PackageNewByCreatorDto from cache by package id
        public void RemovePackageFromCache(int packageId)
        {
            var userId = AbpSession.UserId;
            RemovePackageFromCache(packageId, userId.Value);
        }

        [Authorize]
        // Method to remove a PackageNewByCreatorDto from cache by package id
        public void RemovePackageFromCache(int packageId, long userId)
        {
            string cacheKey = $"NewPackages-{userId}-{DateTime.Now:yyMMdd}";
            // Lấy danh sách hiện tại từ cache
            var cachedPackages = _cacheService.GetCacheValue<List<PackageNewByCreatorDto>>(cacheKey);

            if (cachedPackages != null)
            {
                // Xóa package theo id
                cachedPackages.RemoveAll(p => p.Id == packageId);
                // Cập nhật lại cache
                _cacheService.SetCacheValue(cacheKey, cachedPackages);
            }
        }

        /// <summary>
        /// 1. Admin
        /// 2. Sale Admin
        /// 3. Sale
        /// 4. Customer
        /// 5. WarehouseCN
        /// 6. WarehouseVN
        /// </summary>
        /// <returns></returns>
        private (int PermissionCase, List<long> CustomerIds) GetPermissionCheckerWithCustomerIds()
        {
            try
            {
                var currentUserId = AbpSession.UserId;

                var permissionCase = -1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;

                var customerDtoIds = new List<CustomerIdDto>();

                // admin thì nhìn thấy tất cả kiện hàng
                if (PermissionChecker.IsGranted(PermissionNames.Role_Admin)
                    || PermissionChecker.IsGranted(PermissionNames.Function_PackageViewAll)
                    )
                {
                    // query = query.Where(x => x.CustomerId == _pbtAppSession.CustomerId);
                    permissionCase = 1;
                    Logger.Info("Admin hoặc Sale Admin truy cập vào danh sách kiện hàng.");
                }

                // Sale admin nhìn thấy tất cả các kiện hàng của mình, của sale dưới quyền và của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                    Logger.Info($"Sale Admin truy cập vào danh sách kiện hàng. SaleAdminId: {_pbtAppSession.CustomerId}");

                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleAdminUserId", CommandType.StoredProcedure,
                    new[]
                    {
                    new SqlParameter("@SaleAdminUserId", currentUserId)
                    });
                }

                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 3;
                    Logger.Info($"Sale truy cập vào danh sách kiện hàng. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleId", CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }
                // customer chỉ nhìn thấy kiện hàng của mình
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 4;
                    Logger.Info($"Customer truy cập vào danh sách kiện hàng. CustomerId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>(
                        "SP_Customers_GetIdsByParentId",
                        CommandType.StoredProcedure,
                        new[]
                        {
                        new SqlParameter("@CustomerId", _pbtAppSession.CustomerId)
                        }
                    );
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseCN))
                {
                    Logger.Info($"Warehouse CN truy cập vào danh sách kiện hàng. WarehouseId: {currentUserWarehouseId}");
                    permissionCase = 5;
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseVN))
                {
                    Logger.Info($"Warehouse VN truy cập vào danh sách kiện hàng. WarehouseId: {currentUserWarehouseId}");
                    permissionCase = 6;
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleCustom))
                {
                    permissionCase = 7;
                    Logger.Info($"Sale Custom truy cập vào danh sách kiện hàng. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleId", CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }

                else
                {
                    permissionCase = -1;
                    Logger.Warn($"Người dùng không có quyền truy cập danh sách kiện hàng. UserId: {currentUserId}");
                }

                if (_pbtAppSession.CustomerId.HasValue && _pbtAppSession.CustomerId > 0)
                {
                    customerDtoIds.Add(new CustomerIdDto { CustomerId = _pbtAppSession.CustomerId.Value });
                }
                return (permissionCase, customerDtoIds.Select(u => u.CustomerId).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Logger.Error("Gặp lỗi khi kiểm tra quyền lấy thông tin khách hàng theo người dùng", ex);
            }
            return (-1, new List<long>());
        }

        private async Task<PackageDto> GetPackageByIdAsync(int packageId)
        {
            return await ConnectDb.GetItemAsync<PackageDto>(
               "SP_Packages_GetById",
               CommandType.StoredProcedure,
               new[]
               {
                    new SqlParameter("@id", packageId)
               });
        }

        public async Task<BagDto> GetByIdAsync(int id)
        {
            return await ConnectDb.GetItemAsync<BagDto>(
               "SP_Bags_GetById",
               CommandType.StoredProcedure,
               new[]
               {
                    new SqlParameter("@id", id)
               });
        }

        private async Task<WarehouseDto> GetWarehouseByIdAsync(int warehouseId)
        {
            return await ConnectDb.GetItemAsync<WarehouseDto>(
               "SP_Warehouses_GetById",
               CommandType.StoredProcedure,
               new[]
               {
                    new SqlParameter("@id", warehouseId)
               });
        }

        private async Task<PackageDto> GetFirstPackageByBagIdAsync(int bagId)
        {
            return await ConnectDb.GetItemAsync<PackageDto>(
               "SP_Packages_GetFirstByBagId",
               CommandType.StoredProcedure,
               new[]
               {
                    new SqlParameter("@BagId", bagId)
               });
        }
    }
}