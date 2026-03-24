using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using MathNet.Numerics;
using MathNet.Numerics.Financial;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NPOI.HPSF;
using NPOI.HSSF.Record.Chart;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Crypto;
using pbt.Application.Cache;
using pbt.Application.Common;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Users;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.Commons.Dto;
using pbt.ConfigurationSettings;
using pbt.Core;
using pbt.CustomerFakes.Dto;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.EntityAuditLogs;
using pbt.EntityAuditLogs.Dto;
using pbt.FundAccounts;
using pbt.OrderNumbers;
using pbt.Orders.Dto;
using pbt.Packages.Dto;
using pbt.ShippingRates;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;
using pbt.WoodenPackings;
using pbt.WoodenPackings.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace pbt.Packages
{
    /// <summary>
    /// 
    /// </summary>
    [Audited]
    [UnitOfWork(false)]
    public class PackageAppService :
        AsyncCrudAppService<Package, PackageDto, int, PagedResultRequestDto, CreateUpdatePackageDto, PackageDto>,
        IPackageAppService
    {
        private pbtAppSession _pbtAppSession;
        private IRepository<Order, long> _orderRepository;
        private IRepository<Customer, long> _customerRepository;
        private IRepository<DeliveryRequestOrder, int> _deliveryRequestOrderRepository;
        private IRepository<DeliveryRequest, int> _deliveryRequestRepository;
        private IIdentityCodeAppService _identityCodeAppService;
        private IRepository<CustomerFake, long> _customerFakeRepository;
        private IRepository<Warehouse, int> _warehouseRepository;
        private IRepository<CustomerAddress, long> _customerAddressRepository;
        private IRepository<ProductGroupType> _productGroupTypeRepository;
        private static Random random = new Random();
        private readonly IConfigurationSettingAppService _configurationSettingAppService;
        private readonly IShippingCostAppService _shippingCostService;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly IRepository<ShippingPartner> _shippingPartnerRepository;
        private readonly IRepository<User, long> _userRepository;

        private readonly IRepository<Transaction, long> _transactionRepository;
        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly IRepository<Bag> _bagRepository;
        private readonly IRepository<DeliveryNote> _deliveryNoteRepository;
        private readonly string[] _roles;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWoodenPackingService _woodenPackingService;
        private readonly ConfigAppCacheService _cacheService;
        private IEntityAuditLogApiClient _entityAuditLogApiClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="orderRepository"></param>
        /// <param name="deliveryRequestOrderRepository"></param>
        /// <param name="deliveryRequestRepository"></param>
        /// <param name="customerRepository"></param>
        /// <param name="identityCodeAppService"></param>
        /// <param name="waybillRepository"></param>
        public PackageAppService(
            IRepository<Package, int> repository,
            IRepository<Order, long> orderRepository,
            IRepository<DeliveryRequestOrder, int> deliveryRequestOrderRepository,
            IRepository<DeliveryRequest, int> deliveryRequestRepository,
            IRepository<Customer, long> customerRepository,
            IIdentityCodeAppService identityCodeAppService,
            //  IRepository<Waybill, long> waybillRepository,
            IRepository<CustomerFake, long> customerFakeRepository,
            IRepository<Warehouse, int> warehouseRepository,
            IRepository<CustomerAddress, long> customerAddressRepository,
            IConfigurationSettingAppService configurationSettingAppService,
            IShippingCostAppService shippingCostService,
            pbtAppSession pbtAppSession,
            IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IRepository<ShippingPartner> shippingPartnerRepository,
            IRepository<User, long> userRepository,
            IRepository<Transaction, long> transactionRepository,
            IFundAccountAppService fundAccountAppService,
            IRepository<Bag> bagRepository,
            IRepository<DeliveryNote> deliveryNoteRepository,
            IHttpContextAccessor httpContextAccessor,
            IWoodenPackingService woodenPackingService,
            IRepository<ProductGroupType> productGroupTypeRepository,
            ConfigAppCacheService cacheService,
            IEntityAuditLogApiClient entityAuditLogApiClient

        ) : base(repository)
        {
            _httpContextAccessor = httpContextAccessor;
            _orderRepository = orderRepository;
            _deliveryRequestOrderRepository = deliveryRequestOrderRepository;
            _deliveryRequestRepository = deliveryRequestRepository;
            _customerRepository = customerRepository;
            _identityCodeAppService = identityCodeAppService;
            //  _waybillRepository = waybillRepository;
            _customerFakeRepository = customerFakeRepository;
            _warehouseRepository = warehouseRepository;
            _configurationSettingAppService = configurationSettingAppService;
            _shippingCostService = shippingCostService;
            _customerAddressRepository = customerAddressRepository;
            _pbtAppSession = pbtAppSession;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _shippingPartnerRepository = shippingPartnerRepository;
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
            _fundAccountAppService = fundAccountAppService;
            _bagRepository = bagRepository;
            _deliveryNoteRepository = deliveryNoteRepository;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
            _woodenPackingService = woodenPackingService;
            _productGroupTypeRepository = productGroupTypeRepository;
            _cacheService = cacheService;
            _entityAuditLogApiClient = entityAuditLogApiClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeepCopy<T>(T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreatePackagesWithSingleOrderAsync(ListCreateUpdatePackageInput input)
        {
            try
            {
                if (input == null || input.Packages == null || !input.Packages.Any())
                {
                    return new BadRequestObjectResult(new { message = "Dữ liệu đầu vào không hợp lệ" });
                }

                if (!input.CustomerId.HasValue)
                {
                    Logger.Error("[CreatePackagesAsync] CustomerId is null or invalid in CreatePackagesAsync");
                    return new BadRequestObjectResult(new { message = "Khách hàng không hợp lệ" });
                }

                var currentCustomer = await GetCustomerWithAddressById(input.CustomerId.Value);
                if (currentCustomer == null)
                {
                    Logger.Error($"[CreatePackagesAsync] Không tìm thấy thông tin khách hàng với ID {input.CustomerId.Value}");
                    return new BadRequestObjectResult(new { message = "Không tìm thấy thông tin khách hàng" });
                }

                if (currentCustomer.WarehouseId == null)
                {
                    Logger.Error($"[CreatePackagesAsync] Khách hàng với ID {input.CustomerId.Value} chưa có kho đích.");
                    return new BadRequestObjectResult(new { message = "Khách hàng chưa có kho đích." });
                }

                var packagePrefixList = await _configurationSettingAppService.GetValueAsync("PackagePrefixCode");
                if (string.IsNullOrEmpty(packagePrefixList))
                {
                    Logger.Error("Configuration 'PackageCode' is not set. Using default prefixes.");
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Mã kiện chưa được khai báo trong thiết lập."
                    });
                }
                Logger.Info($"[CreatePackages] Input: {JsonConvert.SerializeObject(input)}");
                string[] packagePrefix = packagePrefixList.Split(",");
                var date = DateTime.Now;
                var currentUser = _pbtAppSession.UserId;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var waybillNumber = input.Packages[0].TrackingNumber;

                // Tìm kiện gốc
                var rootPackage = await GetPackageByTrackingNumber(waybillNumber);

                // Nếu như có kiện gốc thì thực hiện thêm các kiện con ( có orderId = OrderId của kiện gốc)
                // Nếu như không có kiện gốc thì tìm đơn hàng theo trackingNumber
                // Nếu như có vận đơn, thì thêm kiện 
                // Cập nhật tổng cân nặng, tiền vào đơn hàng

                // Tìm vận đơn gốc
                var rootOrder = await GetOrderByTrackingNumber(waybillNumber);

                Logger.Info($"[CreatePackages] {waybillNumber}| currentCustomer: {currentCustomer.Id} - {currentCustomer.Username} rootPackage: {JsonConvert.SerializeObject(rootPackage)} | rootOrder: {JsonConvert.SerializeObject(rootOrder)}");

                if (rootOrder != null && rootOrder.CustomerId != null && rootOrder.CustomerId != input.CustomerId)
                {
                    Logger.Error($"[CreatePackagesAsync] Đơn hàng với mã vận đơn {waybillNumber} đã thuộc về khách hàng khác.");
                    return new BadRequestObjectResult(new { message = "Đơn hàng đã thuộc về khách hàng khác." });
                }

                long rootOrderId = 0;

                if (rootOrder == null || rootOrder.OrderStatus > (int)OrderStatus.InChinaWarehouse)
                {
                    var rootOrderIdSqlParam = new SqlParameter
                    {
                        ParameterName = "@NewOrderId",
                        SqlDbType = SqlDbType.BigInt,
                        Direction = ParameterDirection.Output
                    };
                    await ConnectDb.ExecuteNonQueryAsync(
                         "SP_Orders_QuickCreateOnPackage",
                         CommandType.StoredProcedure,
                         new[] {
                        new SqlParameter("@WaybillNumber", waybillNumber),
                        new SqlParameter("@CustomerId", input.CustomerId.Value),
                        new SqlParameter("@UserSaleId", currentCustomer.SaleId ?? 0),
                        new SqlParameter("@CustomerName", currentCustomer.Username),
                        new SqlParameter("@CNWarehouseId", currentUserWarehouseId ?? 0),
                        new SqlParameter("@VNWarehouseId", (int)currentCustomer.WarehouseId),
                        new SqlParameter("@ShippingLine", (int)input.ShippingLineId),
                        new SqlParameter("@AddressId",  currentCustomer.AddressId ??0),
                        new SqlParameter("@CreatorUserId", currentUser.Value),
                        rootOrderIdSqlParam
                        }
                    );
                    rootOrderId = (long)rootOrderIdSqlParam.Value;

                    var orderCreated = new OrderDto()
                    {
                        Id = rootOrderId,
                        WaybillNumber = waybillNumber,
                        CustomerId = input.CustomerId.Value,
                        CustomerName = currentCustomer.Username,
                        CNWarehouseId = currentUserWarehouseId ?? 0,
                        VNWarehouseId = (int)currentCustomer.WarehouseId,
                        ShippingLine = input.ShippingLineId,
                        AddressId = currentCustomer.AddressId ?? 0,
                        CreationTime = DateTime.Now,
                        CreatorUserId = currentUser.Value
                    };
                    _entityChangeLoggerAppService.LogChangeAsync<OrderDto>(
                      null
                      , orderCreated
                      , "created"
                      , $"Tạo vận đơn #{orderCreated.Id} - {waybillNumber} thông qua tạo kiện"
                      , true
                    );


                }

                else
                {
                    rootOrderId = rootOrder.Id;
                    // kiểm tra xem order có thông tin khách hàng, kho không để thực hiện update
                    if (rootOrder.CustomerId == null || rootOrder.CustomerId != input.CustomerId || rootOrder.CNWarehouseId == 0 || rootOrder.VNWarehouseId == 0)
                    {
                        rootOrder.ShippingLine = input.ShippingLineId;
                        rootOrder.CustomerId = input.CustomerId.Value;
                        rootOrder.CustomerName = currentCustomer.Username;
                        rootOrder.CNWarehouseId = currentUserWarehouseId ?? 0;
                        rootOrder.VNWarehouseId = (int)currentCustomer.WarehouseId;


                        // Nếu như trạng thái đơn là mới, kí gửi thì cập nhật trạng thái của đơn thành đã về kho TQ
                        if (rootOrder.OrderStatus < (int)OrderStatus.InChinaWarehouse)
                        {
                            rootOrder.OrderStatus = (int)OrderStatus.InChinaWarehouse;
                        }
                        var rootOrderEntity = ObjectMapper.Map<Order>(rootOrder);

                        await _orderRepository.UpdateAsync(rootOrderEntity);

                    }

                }
                var newPackageLst = new List<int>();
                var numberOfPackage = input.NumberPackage != null ? (input.NumberPackage + 1) : 1;
                var warehouse = await GetWarehouseByCustomerId(input.CustomerId.Value);
                var rmbRateStr = await _configurationSettingAppService.GetValueAsync("ExchangeRateRMB");
                decimal.TryParse(rmbRateStr, out var rmbRate);
                for (int i = 1; i <= numberOfPackage; i++)
                {
                    var package = input.Packages[0];
                    
                    var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(packagePrefix[0], date);
                    var fakeCustomer = await GetRandomCustomerFake();
                    var domesticShippingFeeVND = package.DomesticShippingFee * rmbRate;
                    var newPackage = new Package
                    {
                        WaybillNumber = waybillNumber,
                        TrackingNumber = (rootPackage == null && i == 1) ? waybillNumber : ShortProductCode.GenerateSafeAoId(),
                        Description = package.Description,
                        Height = package.Height,
                        IsDomesticShipping = package.IsDomesticShipping,
                        IsInsured = package.IsInsured,
                        IsShockproof = package.IsShockproof,
                        IsWoodenCrate = package.IsWoodenCrate,
                        Length = package.Length,
                        PackageNumber = $"{identityCode.Prefix}{DateTime.Now.ToString("ddMMyy")}{identityCode.SequentialNumber.ToString("D5")}",
                        PriceCN = package.Price,
                        Price = (package.Price  ?? 0)* rmbRate,
                        ProductLink = package.ProductLink,
                        ProductNameCn = package.ProductNameCn,
                        ProductNameVi = package.ProductNameVi,
                        Quantity = package.Quantity,
                        ShippingLineId = package.ShippingLineId,
                        ProductGroupTypeId = package.ProductGroupTypeId,

                        Weight = package.Weight,
                        Width = package.Width,
                        WarehouseId = currentUserWarehouseId,
                        OrderId = rootOrderId,
                        WarehouseStatus = (int)WarehouseStatus.InStock,
                        ShippingStatus = (int)PackageDeliveryStatusEnum.Initiate, // Khởi tạo
                        DomesticShippingFeeRMB = package.DomesticShippingFee, // phí vận chuyển nội địa  RMB
                        DomesticShippingFee = domesticShippingFeeVND, // phí vận chuyển nội địa VND
                        CustomerId = package.CustomerId,
                        CustomerFakeId = fakeCustomer?.Id,
                        MatchTime = DateTime.Now,
                        IsQuickBagging = true,
                        WarehouseCreateId = currentUserWarehouseId,
                        WarehouseDestinationId = currentCustomer.WarehouseId

                    };
                    if (rootPackage != null && rootPackage.ShippingStatus < (int)PackageDeliveryStatusEnum.Delivered)
                        newPackage.ParentId = rootPackage.Id;


                    // var newPackageId = Repository.InsertAndGetId(newPackage);

                    var newPackageDto = ObjectMapper.Map<PackageDto>(newPackage);

                    newPackageDto.Id = await InsertPreparedPackageViaSpAsync(newPackageDto);

                    var newPackageByCreator = new PackageNewByCreatorDto
                    {
                        Id = newPackageDto.Id,
                        PackageNumber = newPackage.PackageNumber,
                        TrackingNumber = newPackage.TrackingNumber,
                        Weight = newPackage.Weight,
                        TotalPrice = newPackage.TotalPrice,
                        CreationTime = newPackage.CreationTime,
                        CustomerId = newPackage.CustomerId.Value,
                        CustomerName = currentCustomer.Username,
                        WarehouseId = newPackage.WarehouseDestinationId,
                        WarehouseName = warehouse.Name,
                        WarehouseCode = warehouse.Code,
                        ShippingLineId = newPackage.ShippingLineId
                    };

                    AddPackageToCache(newPackageByCreator);
                    // Thêm vào cache
                    //
                    

                    // add log
                    _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                        null
                        , newPackageDto
                        , "created"
                        , $"Tạo kiện hàng #{newPackageDto.Id} - {waybillNumber}"
                        , true
                    );
                    newPackageLst.Add(newPackageDto.Id);
                }

                // cập nhật lại đơn hàng
                UpdateOrderSummaryFromPackages(rootOrderId);

                return new JsonResult(new
                {
                    success = true,
                    message = "Tạo kiện hàng thành công",
                    data = new
                    {
                        id = newPackageLst,
                        //  childs = waybillChildCount,
                        //parentTrackingNumber = parentTrackingNumber,
                        //newTrackingNumber = newTrackingNumber
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"[CreatePackagesWithSingleOrderAsync] Exception: {ex}");
                return new JsonResult(new
                {
                    success = false,
                    message = "Tạo kiện hàng thất bại: " + ex.Message
                });
            }
        }

        private async Task CreateTransactionDomesticShippingFee(long fundAccountId, string orderId, decimal amount, int transDirection, int transType, int execSrc, int transStatus, long creatorUserId, long? customerId)
        {

            var newTransactionIdParam = new SqlParameter
            {
                ParameterName = "@TransactionId",
                SqlDbType = SqlDbType.NVarChar,
                Size = 50,
                Direction = ParameterDirection.Output
            };
            var newIdParam = new SqlParameter
            {
                ParameterName = "@Id",
                SqlDbType = SqlDbType.BigInt,
                Direction = ParameterDirection.Output
            };
            var statusParam = new SqlParameter
            {
                ParameterName = "@Status",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var msgParam = new SqlParameter
            {
                ParameterName = "@Msg",
                SqlDbType = SqlDbType.NVarChar,
                Size = 500,
                Direction = ParameterDirection.Output
            };
            await ConnectDb.ExecuteNonQueryAsync(
                "SP_Transactions_InsertDomesticShippingExpense",
                CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@FundAccountId", fundAccountId),
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@Amount", amount),

                    new SqlParameter("@TransactionDirection", transDirection),
                    new SqlParameter("@TransactionType", transType),
                    new SqlParameter("@ExecutionSource", execSrc),
                    new SqlParameter("@TransactionStatus", transStatus),
                    new SqlParameter("@CreatorUserId", creatorUserId),
                      new SqlParameter("@RecipientPayer", customerId),
                    newIdParam,
                    newTransactionIdParam,
                    statusParam,
                    msgParam
                });

            var newTransactionId = (string)newTransactionIdParam.Value;
            var newId = (long)newIdParam.Value;
            var status = (int)statusParam.Value;
            var msg = (string)msgParam.Value;
            if (status <= 0)
            {
                Logger.Error($"[CreateTransactionDomesticShippingFee] Failed to create transaction for domestic shipping fee. Status: {status}, Message: {msg}");
                throw new UserFriendlyException("Không thể tạo giao dịch cho phí vận chuyển nội địa: " + msg);
            }
            else
            {
                Logger.Info($"[CreateTransactionDomesticShippingFee] Successfully created transaction for domestic shipping fee. TransactionId: {newTransactionId}, Id: {newId}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditPackagesAsync(EditPackageDto input)
        {
            try
            {
                if (input == null || input.Id <= 0)
                {
                    return new BadRequestObjectResult(new { message = "Invalid package data" });
                }

                if (!input.CustomerId.HasValue || input.CustomerId <= 0)
                {
                    return new BadRequestObjectResult(new { message = "Invalid customer data" });
                }
                var package = await Repository.GetAsync(input.Id);

                if (package == null)
                {
                    return new NotFoundObjectResult(new { message = "Package not found" });
                }

                Customer customer = null;
                // Nếu thay đổi khách hàng thì kiểm tra khách hàng có tồn tại không
                if (package.CustomerId != input.CustomerId.Value)
                {
                    customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == input.CustomerId.Value);
                    if (customer == null)
                    {
                        throw new UserFriendlyException($"Không tìm thấy thông tin khách hàng với ID {package.CustomerId}");
                    }
                }

                var oldBagId = package.BagId;

                var oldPackage = ObjectMapper.Map<Package>(package);

                package.PackageNumber = input.PackageNumber;
                package.CustomerId = input.CustomerId.Value;
                // Tính lại phí vận chuyển
                package.Weight = input.Weight;
                package.Length = input.Length;
                package.Width = input.Width;
                package.Height = input.Height;
                package.IsShockproof = input.IsShockproof;
                package.IsWoodenCrate = input.IsWoodenCrate;
                package.PriceCN = input.PriceCN;
                package.ShippingLineId = input.ShippingLineId;
                package.ProductNameCn = input.ProductNameCn;
                package.ProductNameVi = input.ProductNameVi;
                package.Quantity = input.Quantity;
                package.PriceCN = input.PriceCN;
                package.Description = input.Description;
                package.CategoryId = input.CategoryId;
                package.ProductGroupTypeId = input.ProductGroupTypeId;
                package.IsInsured = input.IsInsured;
                package.InsuranceValue = input.InsuranceValue;
                package.IsWoodenCrate = input.IsWoodenCrate;
                package.IsShockproof = input.IsShockproof;
                package.IsDomesticShipping = input.IsDomesticShipping;

                // Cập nhật trạng thái kho và vận chuyển
                package.WarehouseStatus = input.WarehouseStatus;
                package.ShippingStatus = input.ShippingStatus;


                package.BagId = input.BagId;

                // Tính phí đóng gỗ
                //package.WoodenPackingId = input.SharedCrateSelectId;
                var _customer = _customerRepository.FirstOrDefault(x => x.Id == input.CustomerId.Value);
                if (package.IsWoodenCrate)
                {
                    package = await _woodenPackingService.UpdateWoodenPacking(package, input.SharedCrateSelectId, _customer.Username, package.TrackingNumber);
                }
                else
                {
                    package.WoodenPackagingFee = 0;
                    package.WoodenPackingId = null;
                    package = await _woodenPackingService.UpdateWoodenPacking(package, null, _customer.Username, package.TrackingNumber);
                }

                if (package.CustomerId != oldPackage.CustomerId && package.OrderId.HasValue && package.OrderId > 0)
                {
                    var order = await _orderRepository.GetAsync(package.OrderId.Value);
                    if (order != null)
                    {
                        order.CustomerId = package.CustomerId;
                        order.CustomerName = customer?.Username;
                        var orderEntity = ObjectMapper.Map<Order>(order);
                        await _orderRepository.UpdateAsync(orderEntity);
                    }
                }
                await Repository.UpdateAsync(package);
                // Update the associated order if it exists
                if (package.OrderId.HasValue)
                {
                    await UpdateOrderSummaryFromPackages(package.OrderId.Value);
                }

                await Repository.GetDbContext().SaveChangesAsync();

                // add log
                var packageDto = ObjectMapper.Map<PackageDto>(package);
                _ = _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                 null
                 , packageDto
                 , "updated"
                 , $"Sửa kiện hàng"
             );
                var entityAuditLog = new EntityAuditLogDto()
                {

                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Cập nhật kiện hàng #{packageDto.Id} - {packageDto.TrackingNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(packageDto),

                };


                return new JsonResult(new
                {
                    success = true,
                    message = "Package updated successfully"
                });
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(500);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> EditPackageByAdmin(PackageEditByAdminDto input)
        {
            try
            {
                if (input == null || input.Id <= 0)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        statusCode = -400,
                        message = "Invalid package data"
                    });
                }

                if (!input.CustomerId.HasValue || input.CustomerId <= 0)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        statusCode = -401,
                        message = "Invalid customer data"
                    });
                }

                var package = await base.GetAsync(new EntityDto<int>(input.Id));
                //  var packageDto = ObjectMapper.Map<PackageDto>(package);
                if (package == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        statusCode = -404,
                        message = "Package not found"
                    });
                }

                // Copy dữ liệu cũ để log diff
                //   var oldPackage = ObjectMapper.Map<Package>(package);

                // Validate thay đổi CustomerId

                string note = "";

                if (package.CustomerId != input.CustomerId.Value)
                {
                    var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == input.CustomerId.Value);
                    if (customer == null)
                    {
                        return new JsonResult(new
                        {
                            success = false,
                            statusCode = -405,
                            message = $"Không tìm thấy khách hàng với ID {input.CustomerId}"
                        });
                    }

                    note += $" Thay đổi khách hàng từ {package.CustomerId} sang {input.CustomerId.Value} - {customer.Username}.";
                }

                // ================================
                // GÁN GIÁ TRỊ UPDATE
                // ================================
                package.PackageNumber = input.PackageNumber;
                package.CustomerId = input.CustomerId.Value;

                package.ProductNameCn = input.ProductNameCn;
                package.ProductNameVi = input.ProductNameVi;

                package.Quantity = input.Quantity;
                package.PriceCN = input.PriceCN;
                package.ProductGroupTypeId = input.ProductGroupTypeId;

                package.Weight = input.Weight;
                package.Length = input.Length;
                package.Width = input.Width;
                package.Height = input.Height;

                package.IsShockproof = input.IsShockproof;
                package.IsWoodenCrate = input.IsWoodenCrate;
                package.IsDomesticShipping = input.IsDomesticShipping;
                package.DomesticShippingFee = input.DomesticShippingFeeRMB ?? 0;

                package.IsInsured = input.IsInsured;
                package.InsuranceValue = input.InsuranceValue ?? 0;

                package.ShippingLineId = input.ShippingLineId;

                //// timeline
                package.ExportDate = input.ExportDate ?? null;
                package.ImportDate = input.ImportDate ?? null;
                package.DeliveryTime = input.DeliveryTime ?? null;

                package.WarehouseStatus = input.WarehouseStatus;
                package.ShippingStatus = input.ShippingStatus;

                await base.UpdateAsync(package);

                // ================================
                // LOG THAY ĐỔI
                // ================================
                _ = _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                    null,
                    package,
                    "updated",
                    "Cập nhật kiện hàng bởi Admin"
                );

                var auditLog = new EntityAuditLogDto()
                {
                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Cập nhật kiện hàng #{package.Id} - {package.TrackingNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),
                    CreatationTime = DateTime.Now,
                    ServiceName = "pbt",
                    TenantId = _pbtAppSession.TenantId
                };

                _entityAuditLogApiClient.SendAsync(auditLog);

                return new JsonResult(new
                {
                    success = true,
                    statusCode = 200,
                    message = "Cập nhật kiện hàng thành công",
                    data = package
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new JsonResult(new
                {
                    success = false,
                    statusCode = 500,
                    message = "Có lỗi xảy ra trong quá trình xử lý."
                });
            }
        }

        public async Task<List<PackageDto>> GetAllByDeliveryRequestAsync(int deliveryId)
        {
            var deliveryOrderIds = _deliveryRequestOrderRepository.GetAll()
                .Where(x => x.DeliveryRequestId == deliveryId).Select(u => u.OrderId).ToList();
            var query = (await Repository.GetAllAsync())
                .Where(u => u.OrderId.HasValue && deliveryOrderIds.Contains(u.OrderId.Value));
            var packages = await query.ToListAsync();
            return ObjectMapper.Map<List<PackageDto>>(packages);
        }

        public async Task<PagedResultDto<PackageByBagDetailDto>> GetAllPackagesByBagIdForBagDetail(PagedPackageByBagIdRequestDto input)
        {
            try
            {
                var currentUserId = AbpSession.UserId;

                var permissionCase = -1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var customerDtoIds = new List<CustomerIdDto>();

                var permissionCheck = GetPermissionCheckerWithCustomerIds();
                permissionCase = permissionCheck.permissionCase;
                var customerIds = permissionCheck.CustomerIds;
                var customerIdsString = string.Join(",", customerIds);

                var data = await ConnectDb.GetListAsync<PackageByBagDetailDto>(
                 "SP_Packages_GetByBagId_ForBagDetailWithPermission",
                 CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@BagId", SqlDbType.Int) { Value = input.BagId },
                        new SqlParameter("@PermissionCase", permissionCase),
                        new SqlParameter("@CustomerIds", customerIdsString),
                 }
                );

                return new PagedResultDto<PackageByBagDetailDto>()
                {
                    Items = data,
                    TotalCount = data.Count,
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"[GetAllPackagesByBagId] Exception: {ex}");
                return new PagedResultDto<PackageByBagDetailDto>()
                {
                    Items = new List<PackageByBagDetailDto>(),
                    TotalCount = 0,
                };
            }
        }


        public async Task<List<PackageByBagDetailDto>> GetAllPackagesListByBagIdAsync(int bagId)
        {
            try
            {
                var data = await ConnectDb.GetListAsync<PackageByBagDetailDto>(
                 "SP_Packages_GetByBagId_ForBagDetail",
                 CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@BagId", SqlDbType.Int) { Value = bagId }
                 }
             );

                return data;
            }
            catch (Exception ex)
            {
                Logger.Error($"[GetAllPackagesListByBagId] Exception: {ex}");

                return new List<PackageByBagDetailDto>();
            }
        }



        public async Task<PagedResultDto<PackageByBagDetailDto>> GetAllPackagesByBagId(PagedPackageByBagIdRequestDto input)
        {
            try
            {
                var data = await ConnectDb.GetListAsync<PackageByBagDetailDto>(
                 "SP_Packages_GetByBagId_ForBagDetail",
                 CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@BagId", SqlDbType.Int) { Value = input.BagId }
                 }
             );

                return new PagedResultDto<PackageByBagDetailDto>()
                {
                    Items = data,
                    TotalCount = data.Count,
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"[GetAllPackagesByBagId] Exception: {ex}");
                return new PagedResultDto<PackageByBagDetailDto>()
                {
                    Items = new List<PackageByBagDetailDto>(),
                    TotalCount = 0,
                };
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<PackageDetailDto> GetDetailAsync(int id, bool printStamp = false)
        {

            var packageDto = await GetByIdAsync(id);
            var package = ObjectMapper.Map<PackageDetailDto>(packageDto);

            if (package.WarehouseId != null)
            {
                package.Warehouse = await GetWarehouseByIdAsync(package.WarehouseId.Value);

                if (printStamp)
                {
                    // add log package
                    _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                        null
                        , packageDto
                        , "updated"
                        , $"In tem"
                        , true
                    );
                }
            }

            package.Order = await GetOrderById(package.OrderId);

            var value = ObjectMapper.Map<PackageDetailDto>(package);

            if (package.ProductGroupTypeId != null && package.ProductGroupTypeId > 0)
            {
                var productGroupType = await _productGroupTypeRepository.FirstOrDefaultAsync(x => x.Id == package.ProductGroupTypeId);
                if (productGroupType != null)
                {
                    value.ProductGroupType = ObjectMapper.Map<ProductGroupTypeDto>(productGroupType);
                }
            }

            if (package.Order != null)
            {
                var customerFake = package.CustomerFakeId != null ? await _customerFakeRepository.FirstOrDefaultAsync(x => x.Id == package.CustomerFakeId) : null;
                value.CustomerFake = ObjectMapper.Map<CustomerFakeDto>(customerFake);
                Customer customer = null;
                if (package.CustomerId.HasValue)
                    customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == package.CustomerId);
                else if (package.Order.CustomerId.HasValue)
                    customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == package.Order.CustomerId);

                if (customer != null)
                {
                    value.Customer = ObjectMapper.Map<CustomerDto>(customer);
                }
                value.FakePackages = await Repository.CountAsync(x => x.ParentId == id);
                var cnWarehouse = await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == package.Order.CNWarehouseId);
                var vnWarehouse = await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == package.Order.VNWarehouseId);
                var currentwarehouse = await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == package.WarehouseId);

                value.CnWarehouse = ObjectMapper.Map<WarehouseDto>(cnWarehouse);
                value.VnWarehouse = ObjectMapper.Map<WarehouseDto>(vnWarehouse);
                value.CurrentWarehouse = ObjectMapper.Map<WarehouseDto>(currentwarehouse);
                string key = "fake_company";
                var rsString = await _configurationSettingAppService.GetValueAsync(key);
                value.FakeCompany = rsString;
            }

            if (package == null)
            {
                throw new UserFriendlyException($"Package with Id {id} not found");
            }

            if (package.DeliveryNoteId.HasValue && package.DeliveryNoteId > 0)
            {
                var deliveryNote = await _deliveryNoteRepository.FirstOrDefaultAsync(x => x.Id == package.DeliveryNoteId);
                if (deliveryNote != null)
                {
                    value.DeliveryNoteCode = deliveryNote.DeliveryNoteCode;
                }
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="printStamp"></param>
        /// <returns></returns>
        public async Task<List<PackageDetailDto>> GetListDetailAsync(string ids, bool printStamp = false)
        {
            try
            {
                if (ids == null)
                {
                    throw new UserFriendlyException($"Package with Id {ids} not found");
                }
                //var packagesIds = ids.Split(",").Select(int.Parse).ToList();

                var packages = await GetByIdsAsync(ids);

                var packagesDto = ObjectMapper.Map<List<PackageDetailDto>>(packages);

                // Use a foreach loop to ensure each operation completes before moving to the next
                foreach (var package in packagesDto)
                {
                    if (package.WarehouseId != null)
                    {
                        package.Warehouse = await GetWarehouseByIdAsync((package.WarehouseId ?? 0)); // await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == (package.WarehouseId ??0));
                    }
                    var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == package.CustomerId);
                    package.Customer = ObjectMapper.Map<CustomerDto>(customer);
                    var customerFake = package.CustomerFakeId != null ? await _customerFakeRepository.FirstOrDefaultAsync(x => x.Id == package.CustomerFakeId) : null;
                    package.CustomerFake = ObjectMapper.Map<CustomerFakeDto>(customerFake);
                    package.VnWarehouse = await GetWarehouseByIdAsync((package.WarehouseDestinationId ?? 0));
                    package.CnWarehouse = await GetWarehouseByIdAsync((package.WarehouseCreateId ?? 0));

                    string fakeCompanyKey = "fake_company";
                    string fakeCompanyAddressKey = "fake_address";
                    package.FakeCompany = await _configurationSettingAppService.GetValueAsync(fakeCompanyKey);
                    package.FakeCompanyAddress = await _configurationSettingAppService.GetValueAsync(fakeCompanyAddressKey);
                    package.FakePackages = 1;

                    if (printStamp)
                    {
                        // add log package
                        _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                            null
                            , package
                            , "updated"
                            , $"In tem"
                            , true
                        );
                    }
                }


                return packagesDto;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public async Task<PagedPackagesFilterResultDto> GetAllPackagesFilterAsync(PagedPackageResultRequestDto input)
        {
            try
            {
                #region Permission Check
                var currentUserId = AbpSession.UserId;

                var permissionCase = -1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var customerDtoIds = new List<CustomerIdDto>();

                var permissionCheck = GetPermissionCheckerWithCustomerIds();
                permissionCase = permissionCheck.permissionCase;
                var customerIds = permissionCheck.CustomerIds;

                #endregion
                var customerIdsString = string.Join(",", customerIds);
                var totalCountParam = new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                var totalWeightParam = new SqlParameter
                {
                    ParameterName = "@TotalWeight",
                    SqlDbType = SqlDbType.Decimal,
                    Precision = 18,
                    Scale = 2,
                    Direction = ParameterDirection.Output
                };
                var data = await ConnectDb.GetListAsync<PackageDto>("SP_Packages_GetListPaging", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@PermissionCase", permissionCase),
                        new SqlParameter("@CurrentUserWarehouseId", currentUserWarehouseId ?? -1),
                        new SqlParameter("@CustomerIds", customerIdsString),
                        new SqlParameter("@ExcludeCoverWeightType", input.ExcludeCoverWeightType),
                        new SqlParameter("@Keyword", input.Keyword ?? string.Empty),
                        new SqlParameter("@BagType", input.BagType ?? -1),
                        new SqlParameter("@UnBagType", input.UnBagType ?? -1),
                        new SqlParameter("@CustomerId", input.CustomerId ?? -1),
                        new SqlParameter("@FeatureId", input.FeatureId ?? -1),
                        new SqlParameter("@ServiceId", input.ServiceId ?? -1),
                        new SqlParameter("@WarehouseId", input.WarehouseId ?? -1),
                        new SqlParameter("@WarehouseDestinationId", input.WarehouseDestinationId ?? -1),
                        new SqlParameter("@WarehouseStatus", input.WarehouseStatus ?? -1),
                        new SqlParameter("@ShippingStatus", input.ShippingStatus ?? -1),
                        new SqlParameter("@ShippingLine", input.ShippingLine ?? -1),
                        new SqlParameter("@Status", input.Status ?? -1),
                        new SqlParameter("@FilterType", -1),
                        new SqlParameter("@StartCreateDate", input.StartCreateDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndCreateDate", input.EndCreateDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartExportDate", input.StartExportDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndExportDate", input.EndExportDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartDeliveryDate", input.StartDeliveryDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndDeliveryDate", input.EndDeliveryDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartImportDate", input.StartImportDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndImportDate", input.EndImportDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartMatchDate", input.StartMatchDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndMatchDate", input.EndMatchDate ?? (object)DBNull.Value),
                        new SqlParameter("@SkipCount", input.SkipCount),
                        new SqlParameter("@MaxResultCount", input.MaxResultCount),
                        new SqlParameter("@IsExportExcel", 0),


                        totalCountParam,
                        totalWeightParam
                    });
                return new PagedPackagesFilterResultDto()
                {
                    Items = data,
                    TotalCount = (int)(totalCountParam.Value ?? 0),
                    TotalWeight = (decimal)(totalWeightParam.Value ?? 0)
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"[GetAllPackagesFilterAsync] Exception: {ex}");
                return new PagedPackagesFilterResultDto()
                {
                    Items = new List<PackageDto>(),
                    TotalCount = 0,
                    TotalWeight = 0
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bagId"></param>
        /// <returns></returns>
        public async Task<List<PackageDto>> GetPackageByBag(int bagId)
        {

            var packages = await ConnectDb.GetListAsync<PackageDto>(
                   "SP_Packages_GetByBagId",
                   System.Data.CommandType.StoredProcedure,
                   new[]
                   {
                        new SqlParameter("@BagId", bagId)
                   }
               );

            return packages;
        }

        public async Task<PackageDto> GetPackageByCodeAsync(string packageCode)
        {
            return await ConnectDb.GetItemAsync<PackageDto>("SP_Packages_GetByPackageNumber",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@PackageNumber",
                    Value = packageCode
                } });
        }

        public async Task<List<PackageDto>> GetByOrder(long orderId)
        {
            var query = (await Repository.GetAllAsync())
                .Where(x => x.ParentId == null)
                .WhereIf(true, u => u.OrderId == orderId);
            var result = ObjectMapper.Map<List<PackageDto>>(query);
            return result;
        }

        public async Task<List<PackageDto>> GetListWithBagInfoByOrder(long orderId)
        {
            var query = (await Repository.GetAllAsync())
                .Include(x => x.Bag)
                .Where(x => x.ParentId == null)
                .WhereIf(true, u => u.OrderId == orderId);

            var data = ObjectMapper.Map<List<PackageDto>>(query);

            return data;
        }

        /// <summary>
        /// lấy danh sách kiện yêu cầu giao
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<PackageReadyForCreateDeliveryNoteDto>> GetPackageByCustomer(int customerId)
        {
            try
            {
                var currentWarehouseId = _pbtAppSession.WarehouseId.Value;
                var data = await ConnectDb.GetListAsync<PackageReadyForCreateDeliveryNoteDto>("SP_Packages_GetForCreateDeliveryNoteByCustomerId", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@CustomerId", SqlDbType.BigInt) { Value = customerId },
                        new SqlParameter("@WarehouseStatus", SqlDbType.Int) { Value = (int) WarehouseStatus.InStock },
                        new SqlParameter("@ShippingStatus", SqlDbType.Int) { Value = (int) PackageDeliveryStatusEnum.InWarehouseVN },
                        new SqlParameter("@currentWarehouseId", SqlDbType.Int) { Value = currentWarehouseId  },
                    });
                return data;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                throw;
            }
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// 
        [HttpPost]
        public async Task UpdatePackageAsync(UpdatePackageDto input)
        {
            var package = await Repository.GetAsync(input.Id);
            var oldPackage = ObjectMapper.Map<Package>(package);
            if (package.BagId.HasValue)
            {
                package.BagId = input.BagId;
            }
            if (input.IsQuickBagging == false)
            {
                package.IsQuickBagging = false;
            }
            // add log
            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                oldPackage
                , package
                , "updated"
                , $"Sửa kiện hàng"
                , true
            );



            await Repository.UpdateAsync(package);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        [HttpPost]
        public async Task RemoveQuickBaggingAsync(PackageRemoveQuickBaggingDto input)
        {

            var package = await Repository.GetAsync(input.Id);
            if (package != null)
            {
                var oldPackage = ObjectMapper.Map<Package>(package);
                if (package.BagId.HasValue)
                {
                    package.BagId = input.BagId;
                }
                if (input.IsQuickBagging == false)
                {
                    package.IsQuickBagging = false;
                }
                // add log
                _entityChangeLoggerAppService.LogChangeAsync<Package>(
                    oldPackage
                    , package
                    , "updated"
                    , $"Sửa kiện hàng"
                    , true
                );
                await Repository.UpdateAsync(package);
            }

            RemovePackageFromCache(input.Id);
        }

        public async Task<List<PackageDto>> GetPackageFakes(long packageId)
        {
            var data = (await Repository.GetAllAsync()).Include(x => x.Order)
                .Where(x => x.ParentId == packageId);
            var result = ObjectMapper.Map<List<PackageDto>>(data);
            foreach (var packageDto in result)
            {
                packageDto.CustomerFake = packageDto.CustomerFakeId != null ? await _customerFakeRepository.FirstOrDefaultAsync(x => x.Id == packageDto.CustomerFakeId) : null;
                packageDto.WaybillCodeFake = RandomString(10);
            }
            return result;
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        /// <summary>
        /// lấy danh sách kiện mới tạo theo currentUser.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// 
        public async Task<List<PackageNewByCreatorDto>> GetNewPackageByCurrentUser()
        {
            // lấy user id đang đăng nhập
            if (AbpSession.UserId.HasValue && AbpSession.UserId > 0)
            {
                var userId = AbpSession.UserId;
                try
                {
                    // Tạo 1 key: {userid}-{yyMMdd}-NewPackages
                    // Kiểm tra cacheKey có không?  nếu có thì lấy ra
                    // Nếu không có thì lấy phía dưới và gán vào

                    // Định nghĩa key cho cache
                    string cacheKey = $"NewPackages-{userId}-{DateTime.Now:yyMMdd}";

                    // Kiểm tra xem cache đã có dữ liệu chưa
                    var cachedData = _cacheService.GetCacheValue<List<PackageNewByCreatorDto>>(cacheKey);
                    if (cachedData != null)
                    {
                        // Nếu cache đã có dữ liệu, trả về từ cache
                        return cachedData.OrderByDescending(x => x.Id).ToList();
                    }
                    var data = await ConnectDb.GetListAsync<PackageNewByCreatorDto>("SP_Packages_GetNewByCreator", CommandType.StoredProcedure,
                          new[]
                      {
                          new SqlParameter("@CreatorUserId", SqlDbType.BigInt) { Value = userId },
                      });
                    _cacheService.SetCacheValue(cacheKey, data, TimeSpan.FromHours(2)); // Lưu cache trong 2 giờ
                    // Trả về dữ liệu
                    return data.OrderByDescending(x => x.Id).ToList();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi lấy danh sách kiện mới tạo theo user hiện tại. {AbpSession.UserId}", ex);
                }
            }
            return new List<PackageNewByCreatorDto>();
        }


        [Authorize]
        // Method to add a PackageNewByCreatorDto to cache
        public void AddPackageToCache(PackageNewByCreatorDto package)
        {

            var userId = AbpSession.UserId;
            string cacheKey = $"NewPackages-{userId}-{DateTime.Now:yyMMdd}";

            // 1. Lấy collection từ cache, nếu chưa có thì tạo mới.
            // Dùng ConcurrentQueue để đảm bảo thứ tự vào trước ra trước (FIFO) và an toàn đa luồng.
            var cachedPackages = _cacheService.GetOrCreate(cacheKey, entry =>
            {
                // Cấu hình cache entry chỉ 1 lần khi tạo mới
                entry.SlidingExpiration = TimeSpan.FromHours(2); // Tự động gia hạn nếu có truy cập
                return new List<PackageNewByCreatorDto>();
            });

            // 2. Thêm trực tiếp vào collection (Cache tự động thấy sự thay đổi vì nó giữ tham chiếu)
            cachedPackages.Add(package);
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

        [Authorize]
        public RememberCustomerDto GetRememberCustomerId()
        {
            var userId = AbpSession.UserId;
            if (userId == null)
            {
                return null;
            }
            string cacheKey = $"RememberCustomer-{userId}";
            if (_cacheService.TryGetCacheValue<RememberCustomerDto>(cacheKey, out var data))
            {
                return data;
            }
            return null;
        }

        [Authorize]
        public void SetOrRemoveRememberCustomerId(RememberCustomerDto data)
        {
            var userId = AbpSession.UserId;
            if (userId == null)
            {
                return;
            }
            string cacheKey = $"RememberCustomer-{userId}";
            if (data != null && data.CustomerId > 0)
            {
                _cacheService.SetCacheValue(cacheKey, data, TimeSpan.FromDays(1));
            }
            else
            {
                _cacheService.RemoveCacheValue(cacheKey);
            }
        }


        /// <summary>
        /// Lấy thông tin đơn hàng theo mã vận đơn
        /// </summary>
        /// <param name="waybillNumber"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        /// 
        [Authorize]
        public async Task<OrderDto> GetByWaybillNumber(string waybillNumber)
        {
            if (string.IsNullOrEmpty(waybillNumber) || string.IsNullOrEmpty(waybillNumber.Trim()))
                return null;
            var data = await ConnectDb.GetItemAsync<OrderDto>("SP_Orders_GetByWaybillNumber", CommandType.StoredProcedure,
                      new[]
                  {
                new SqlParameter("@waybillNumber", SqlDbType.NVarChar) { Value = waybillNumber.Trim() },
                  });
            return data;

        }


        public async Task<PagedSaleViewResultDto<PackageViewForSaleDto>> GetListForSaleViewAsync(SaleViewRequestDto input)
        {
            try
            {
                var currentUserId = AbpSession.UserId;
                var permissionCase = 1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;
                var customerDtoIds = new List<CustomerIdDto>();
                var result = GetPermissionCheckerWithCustomerIds();
                permissionCase = result.permissionCase;
                var customerIds = result.CustomerIds;
                var query = base.CreateFilteredQuery(input);
                // Tạo query để lấy danh sách kiện hàng
                query = query
                     .Where(x =>

                       (permissionCase == 1) || // admin  nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value)) ||
                       (permissionCase == 5 && x.WarehouseCreateId == currentUserWarehouseId) ||
                       (permissionCase == 6 && x.WarehouseDestinationId == currentUserWarehouseId)
                    // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );

                if (input.CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }
                if (input.StartExportDate.HasValue)
                {
                    query = query.Where(x => x.ExportDate >= input.StartExportDate);
                }
                if (input.EndImportDate.HasValue)
                {
                    query = query.Where(x => x.ImportDate <= input.EndImportDate);
                }
                if (input.StartImportDate.HasValue)
                {
                    query = query.Where(x => x.ImportDate >= input.StartImportDate);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    var keyword = input.Keyword.ToUpper();
                    query = query.Where(x =>
                        x.TrackingNumber.ToUpper().Contains(keyword) ||
                        x.PackageNumber.ToUpper().Contains(keyword) ||
                        x.BagNumber.ToUpper().Contains(keyword)
                        );
                }
                if (input.ShippingLine > 0)
                {
                    query = query.Where(x => x.ShippingLineId == input.ShippingLine);
                }


                // Tổng số lượng bản ghi
                var totalCount = query.Count();

                // Lấy dữ liệu phân trang
                var data = ObjectMapper.Map<List<PackageViewForSaleDto>>(
                    query
                    .OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToList());

                customerIds = data.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId.Value).Distinct().ToList();

                var customerDict = (await _customerRepository.GetAllAsync()).Where(x => customerIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x.Username);
                var shippingPartnerIds = data.Select(u => u.ShippingPartnerId).Distinct().ToList();
                var shippingPartners = (await _shippingPartnerRepository.GetAllAsync()).Where(x => shippingPartnerIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x.Name);

                foreach (var item in data)
                {
                    item.CustomerName = item.CustomerId.HasValue && customerDict.ContainsKey(item.CustomerId.Value) ? customerDict[item.CustomerId.Value] : "";
                    item.ShippingPartner = item.ShippingPartnerId.HasValue && shippingPartners.ContainsKey(item.ShippingPartnerId.Value) ? shippingPartners[item.ShippingPartnerId.Value] : "";
                }

                // Tính tổng trọng lượng
                var totalWeight = query.Sum(x => (x.Weight ?? 0)); /*data.Sum(x => x.Weight ?? 0);*/
                return new PagedSaleViewResultDto<PackageViewForSaleDto>
                {
                    Items = data,
                    TotalCount = totalCount,
                    TotalWeight = totalWeight
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách kiện theo SALE", ex);
                throw ex;
            }

        }

        public async Task<PagedSaleViewResultDto<PackageImportExportWithBagDto>> GetPackageImportExportWithBagAsync(ImportExportWithBagRequestDto input)
        {
            try
            {
                var permissionCheckResult = GetPermissionCheckerWithCustomerIds();

                if (permissionCheckResult.permissionCase == -1)
                {
                    return new PagedSaleViewResultDto<PackageImportExportWithBagDto>
                    {
                        Items = new List<PackageImportExportWithBagDto>(),
                        TotalCount = 0,
                        TotalWeight = 0
                    };
                }
                var permissionCase = permissionCheckResult.permissionCase;
                var customerIds = permissionCheckResult.CustomerIds;
                var bagQuery = (await _bagRepository.GetAllAsync())
                    .Where(x => x.ShippingStatus > 1
                     && x.ExportDate.HasValue // Chỉ lấy bao đã xuất kho TQ
                    )
                    .Include(x => x.Packages)
                    .Include(x => x.DeliveryNote)
                    .ThenInclude(p => p.Customer)
                    .AsQueryable();

                var isSale = (_roles.Contains(RoleConstants.sale) || _roles.Contains(RoleConstants.saleadmin)) && !_roles.Contains(RoleConstants.admin);
                var parentId = _pbtAppSession.UserId;
                if (isSale)
                {
                    List<long> saleIds = new List<long>();
                    if (_roles.Contains(RoleConstants.saleadmin))
                    {
                        saleIds = (await _customerRepository.GetAllAsync()).Where(x => x.SaleId == parentId).Select(y => y.Id).ToList();
                    }
                    bagQuery = bagQuery.Where(x => x.Customer.SaleId == parentId || saleIds.Contains(x.Customer.SaleId));
                }


                // Ngày xuất kho TQ
                if (input.StartExportDate.HasValue)
                {
                    bagQuery = bagQuery.Where(x => x.ExportDate.Value.Date >= input.StartExportDate.Value.Date);
                }
                if (input.EndImportDate.HasValue)
                {
                    bagQuery = bagQuery.Where(x => x.ImportDate.Value.Date <= input.EndImportDate.Value.Date);
                }

                // Ngày nhập kho VN
                if (input.StartImportDate.HasValue)
                {
                    bagQuery = bagQuery.Where(x =>
                    x.ImportDate.HasValue
                    && x.ImportDate.Value.Date >= input.StartImportDate.Value.Date
                    );
                }

                if (input.EndImportDate.HasValue)
                {
                    bagQuery = bagQuery.Where(x =>
                    x.ImportDate.HasValue
                    && x.ImportDate.Value.Date <= input.EndImportDate.Value.Date);
                }

                // Ngày xuất kho VN
                if (input.StartExportVNDate.HasValue)
                {
                    // chỉ lấy bao riêng có ngày xuất kho tại VN vì bao riêng mới có ngày xuất kho. Còn bao ghép thì lấy ngày xuất kho theo kiện
                    bagQuery = bagQuery.Where(x => x.DeliveryDate.HasValue
                    && x.DeliveryDate.Value.Date >= input.StartExportVNDate.Value.Date
                    && x.BagType == (int)BagTypeEnum.SeparateBag);
                }

                if (input.EndExportVNDate.HasValue)
                {
                    bagQuery = bagQuery.Where(x => x.DeliveryDate.HasValue
                   && x.DeliveryDate.Value.Date <= input.EndExportVNDate.Value.Date
                   && x.BagType == (int)BagTypeEnum.SeparateBag);
                }

                // Lọc theo CustomerId trong kiện
                if (input.CustomerId > 0)
                {
                    bagQuery = bagQuery.Where(b =>
                    (b.BagType == (int)BagTypeEnum.InclusiveBag && b.Packages.Any(p => p.CustomerId == input.CustomerId))
                    || (b.BagType == (int)BagTypeEnum.SeparateBag && b.CustomerId == input.CustomerId)
                    );
                }

                // Đối tác vận chuyển  quốc tế
                if (input.ShippingPartnerIntern > 0)
                {
                    bagQuery = bagQuery.Where(b => b.DeliveryNote != null && b.DeliveryNote.ShippingPartnerId == input.ShippingPartnerDomestic);
                }

                var totalCount = bagQuery.Count();

                if ((input.StartExportDate.HasValue && input.EndExportDate.HasValue && input.StartExportDate.Value.Date == input.EndExportDate.Value.Date)
                    || (input.StartExportVNDate.HasValue && input.EndExportVNDate.HasValue && input.StartExportVNDate.Value.Date == input.EndExportVNDate.Value.Date)
                    || (input.StartImportDate.HasValue && input.EndImportDate.HasValue && input.StartImportDate.Value.Date == input.EndImportDate.Value.Date))
                {
                    //bagQuery = bagQuery.Where(x => x.ExportDate >= input.StartExportDate && x.ExportDate <= input.EndExportDate);
                    bagQuery = bagQuery.OrderBy(x => x.CustomerId);
                }
                else
                {
                    bagQuery = bagQuery.OrderByDescending(x => x.Id);
                }

                var bags = bagQuery
                    //   .OrderBy(x => x.CustomerId)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToList();

                var dataPackage = new List<PackageImportExportWithBagDto>();

                foreach (var bag in bags)
                {
                    // Bao ghép: lấy tất cả các kiện trong bao, lọc theo CustomerId, ExportDate, ImportDate
                    if (bag.BagType == (int)BagTypeEnum.InclusiveBag)
                    {
                        var packageIds = bag.Packages.Select(u => u.Id);

                        var packagesQuery = (await Repository.GetAllAsync()).Where(p => packageIds.Contains(p.Id));
                        //  .Where(p => p.ParentId == null)
                        // Lọc theo khách hàng
                        if (input.CustomerId > 0)
                        {
                            packagesQuery = packagesQuery.Where(p => p.CustomerId == input.CustomerId);
                        }

                        // Ngày xuất kho TQ
                        if (input.StartExportDate.HasValue)
                        {
                            packagesQuery = packagesQuery.Where(p => p.ExportDate.HasValue && p.ExportDate.Value.Date >= input.StartExportDate.Value.Date);
                        }
                        if (input.EndExportDate.HasValue)
                        {
                            packagesQuery = packagesQuery.Where(p => p.ExportDate.HasValue && p.ExportDate.Value.Date <= input.EndExportDate.Value.Date);
                        }


                        // Ngày xuất kho VN
                        if (input.StartExportVNDate.HasValue)
                        {
                            packagesQuery = packagesQuery.Where(p => p.DeliveryTime.HasValue && p.DeliveryTime.Value.Date >= input.StartExportVNDate.Value.Date);
                        }
                        if (input.EndExportVNDate.HasValue)
                        {
                            packagesQuery = packagesQuery.Where(p => p.DeliveryTime.HasValue && p.DeliveryTime.Value.Date <= input.EndExportVNDate.Value.Date);
                        }


                        // Ngày nhập kho VN
                        if (input.StartImportDate.HasValue)
                        {
                            packagesQuery = packagesQuery.Where(p => p.ImportDate.HasValue && p.ImportDate.Value.Date >= input.StartImportDate.Value.Date);
                        }
                        if (input.EndImportDate.HasValue)
                        {
                            packagesQuery = packagesQuery.Where(p => p.ImportDate.HasValue && p.ImportDate.Value.Date <= input.EndImportDate.Value.Date);
                        }

                        if (input.ShippingPartnerIntern > 0)
                        {
                            packagesQuery = packagesQuery.Where(p => p.ShippingPartnerId == input.ShippingPartnerIntern);
                        }

                        var packages = await packagesQuery
                            .OrderBy(x => x.CustomerId)
                            .Include(u => u.ShippingPartner)
                            .Include(u => u.DeliveryNote).ThenInclude(sp => sp.ShippingPartner)
                            .Include(u => u.Customer)
                            .ToListAsync();

                        foreach (var package in packages)
                        {

                            dataPackage.Add(new PackageImportExportWithBagDto
                            {
                                PackageId = package.Id,
                                BagId = (int)package.BagId,
                                ExportDate = package.ExportDate ?? bag.ExportDate,
                                CustomerName = package.Customer?.FullName ?? "",
                                WaybillCode = package.TrackingNumber,
                                PackageCode = package.PackageNumber ?? "",
                                BagNumber = bag.BagCode,

                                Weight = package.Weight,
                                Dimension = (decimal)((package.Length ?? 0) * (package.Width ?? 0) * (package.Height ?? 0)) / 1000000,
                                PackageCount = package.Quantity ?? 0,
                                ShippingPartner = package.ShippingPartner?.Name,
                                Characteristic = bag.IsSolution ? "Dung dịch" :
                                    bag.IsWoodSealing ? "Đóng gỗ" :
                                    bag.IsFakeGoods ? "Hàng giả" :
                                    bag.IsOtherFeature ? bag.otherReason : "",

                                ImportDate = package.ImportDate ?? bag.ImportDate,
                                // VCQT

                                ShippingFeeIntern = package.TotalFee ?? 0,

                                // Phí vận chuyển nội địa TQ
                                ShippingFeeCN = package.DomesticShippingFee,

                                SecuringCost = (package.InsuranceFee ?? 0) + (package.ShockproofFee ?? 0),
                                ShippingFeeVN = 0,
                                // Tổng phí = phí Vận chuyển nội địa + phí bảo hiểm + phí chống sốc + phí vận chuyển quốc tế (TQ -> VN)  + phí vận chuyển VN
                                TotalFee = (package.TotalFee ?? 0) + package.DomesticShippingFee + (package.InsuranceFee ?? 0) + (package.ShockproofFee ?? 0) + 0,
                                ShippingFeeAbsorbedByWarehouse = (package.IsRepresentForDeliveryNote
                                                                && package.DeliveryNote != null
                                                                && package.DeliveryNote.DeliveryFeeReason == (int)DeliveryFeeType.WithFee
                                                                && package.DeliveryNote.ShippingFee.HasValue
                                                                ? package.DeliveryNote.ShippingFee.Value : 0),

                                ShippingPartnerVN = (package.DeliveryNote != null
                                                                && package.DeliveryNote.ShippingPartner != null
                                                                ? package.DeliveryNote.ShippingPartner.Name : ""),

                                ExportDateVN = package.DeliveryTime,
                                WoodenPackagingFee = package.WoodenPackagingFee ?? 0,
                                ShippingLineId = package.ShippingLineId,
                                Note = package.Note ?? "",
                                IsRepresentForDeliveryNote = package.IsRepresentForDeliveryNote,
                                BagType = bag.BagType,
                                DeliveryNoteId = bag.DeliveryNoteId,
                                UnitPrice = package?.UnitPrice,
                                OriginShippingCost = package.OriginShippingCost
                            });
                        }
                    }
                    else // Bao riêng: chỉ lấy thông tin của bao, nếu thiếu thì lấy từ package đầu tiên
                    {
                        //var package = bag.Packages.FirstOrDefault(p => p.ParentId == null);
                        bag.ShippingPartner = (await _shippingPartnerRepository.GetAllAsync()).FirstOrDefault(p => p.Id == bag.ShippingPartnerId);
                        bag.Customer = (await _customerRepository.GetAllAsync()).FirstOrDefault(p => p.Id == bag.CustomerId);
                        var packages = (await Repository.GetAllAsync())
                            .Where(p => p.BagId == bag.Id && p.ParentId == null
                                && p.ExportDate.HasValue // Chỉ lấy kiện đã xuất kho TQ
                            ).AsNoTracking()

                            .Include(p => p.Customer)
                            .Include(p => p.ShippingPartner);

                        var package = packages.FirstOrDefault();

                        // Tối ưu: dùng vòng lặp for để tổng hợp số liệu
                        decimal totalPackageDomesticShippingFee = 0;
                        decimal totalPackageSecuringCost = 0;
                        decimal totalPackageShipChina = 0;
                        decimal totalPackageShippingIntern = 0;
                        decimal totalPackageFee = 0;
                        int packagePackageCount = 0;
                        decimal totalPackageWeight = 0;

                        decimal totalPackageWoodenPackagingFee = 0;

                        foreach (var x in packages)
                        {
                            packagePackageCount++;

                            // Tính tổng cân nặng
                            totalPackageWeight += x.Weight ?? 0;


                            // Tính tổng phí ship nội địa
                            totalPackageDomesticShippingFee += x.DomesticShippingFee;

                            // Tính tổng phí gia cố (quấn bọt khí + đóng gỗ)
                            totalPackageSecuringCost += (x.ShockproofFee ?? 0) + (x.WoodenPackagingFee ?? 0);

                            // Tính tổng phí vận chuyển nội địa Trung Quốc
                            totalPackageShipChina += x.DomesticShippingFeeRMB;

                            // Tính tổng phí vận chuyển quốc tế
                            totalPackageShippingIntern += x.TotalFee ?? 0;

                            // Tính tổng phí đóng gỗ
                            totalPackageWoodenPackagingFee += x.WoodenPackagingFee ?? 0;

                            // Tính tổng tiền của kiện (bao gồm tất cả các phí)
                            totalPackageFee += (x.TotalPrice ?? 0);

                        }

                        var dataPackageItem = new PackageImportExportWithBagDto
                        {
                            PackageId = package?.Id ?? 0,
                            BagId = bag.Id,
                            ExportDate = bag.ExportDate ?? package?.ExportDate,
                            CustomerName = bag?.Customer?.Username ?? "",
                            WaybillCode = package?.TrackingNumber ?? "",
                            PackageCode = package?.PackageNumber ?? "",
                            BagNumber = bag.BagCode,
                            Weight = totalPackageWeight,
                            TotalFee = totalPackageFee,

                            SecuringCost = totalPackageSecuringCost,
                            ShippingFeeIntern = totalPackageShippingIntern,
                            ShippingFeeCN = totalPackageDomesticShippingFee,
                            WoodenPackagingFee = totalPackageWoodenPackagingFee,
                            ShippingLineId = package?.ShippingLineId,
                            Note = package?.Note ?? "",
                            BagType = bag.BagType,


                            Dimension = (decimal)((bag.Length ?? 0) * (bag.Width ?? 0) * (bag.Height ?? 0)) / 1000000,
                            PackageCount = packagePackageCount,
                            ShippingPartner = bag.ShippingPartner?.Name,
                            Characteristic = bag.IsSolution ? "Dung dịch" :
                                    bag.IsWoodSealing ? "Đóng gỗ" :
                                    bag.IsFakeGoods ? "Hàng giả" :
                                    bag.IsOtherFeature ? bag.otherReason : "",

                            ImportDate = bag.ImportDate ?? package?.ImportDate,
                            IsRepresentForDeliveryNote = bag.IsRepresentForDeliveryNote,

                            ShippingFeeAbsorbedByWarehouse = (bag.IsRepresentForDeliveryNote
                                                                && bag.DeliveryNote != null
                                                                && bag.DeliveryNote.DeliveryFeeReason == (int)DeliveryFeeType.WithFee
                                                                && bag.DeliveryNote.ShippingFee.HasValue
                                                                ? bag.DeliveryNote.ShippingFee.Value : 0),

                            ShippingPartnerVN = (bag.DeliveryNote != null
                                                                && bag.DeliveryNote.ShippingPartner != null
                                                                ? bag.DeliveryNote.ShippingPartner.Name : ""),
                            ExportDateVN = bag.DeliveryDate,
                            UnitPrice = package?.UnitPrice,
                            OriginShippingCost = bag.TotalOriginShippingCost
                        };

                        if (bag.DeliveryNoteId.HasValue)
                        {
                            var deliveryNote = (await _deliveryNoteRepository.GetAllAsync())
                                .Include(x => x.ShippingPartner)
                                .FirstOrDefault(x => x.Id == bag.DeliveryNoteId.Value);
                            if (deliveryNote != null)
                            {
                                dataPackageItem.DeliveryNoteId = deliveryNote.Id;
                                dataPackageItem.DeliveryNote = deliveryNote;
                                dataPackageItem.ShippingPartnerVN = deliveryNote.ShippingPartner?.Name;
                            }
                        }
                        dataPackage.Add(dataPackageItem);

                    }
                }

                var totalWeight = dataPackage.Sum(x => x.Weight ?? 0);

                return new PagedSaleViewResultDto<PackageImportExportWithBagDto>
                {
                    Items = dataPackage,
                    TotalCount = totalCount,
                    TotalWeight = totalWeight
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<PagedResultDto<PackageDto>> GetAllPackagesByCurrentUserOrderViewAsync(PagedPackageResultRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            try
            {
                var permissionCase = 1;
                var customerDtoIds = new List<CustomerIdDto>();
                // admin và sale admin sẽ nhìn thấy tất cả kiện hàng
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin) || await PermissionChecker.IsGrantedAsync(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 1;
                    Logger.Info("Admin hoặc Sale Admin truy cập vào danh sách kiện hàng.");
                }
                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 2;
                    Logger.Info($"Sale truy cập vào danh sách kiện hàng. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = await ConnectDb.GetListAsync<CustomerIdDto>(
                   "SP_Customers_GetIdsBySaleId",
                   CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 3;
                    Logger.Info($"Customer truy cập vào danh sách kiện hàng. CustomerId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = await ConnectDb.GetListAsync<CustomerIdDto>(
                        "SP_Customers_GetIdsByParentId",
                        CommandType.StoredProcedure,
                        new[]
                        {
                        new SqlParameter("@CustomerId", _pbtAppSession.CustomerId)
                        }
                    );
                }
                else
                {
                    permissionCase = 4;

                    return new PagedResultDto<PackageDto>
                    {
                        Items = new List<PackageDto>(),
                        TotalCount = 0
                    };
                }

                var customerIds = new List<long>();
                if (customerDtoIds != null)
                {
                    customerIds = customerDtoIds.Select(u => u.CustomerId).ToList();
                }

                // Tạo query để lấy danh sách kiện hàng
                var query = (await Repository.GetAllAsync())
                    .Include(x => x.Customer) // Bao gồm thông tin khách hàng

                    .Where(x => x.ParentId == null)
                    .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3) && customerIds.Contains(x.CustomerId ?? 0)) // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );

                // Lọc theo từ khóa
                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x =>
                        x.TrackingNumber.ToUpper().Contains(input.Keyword.ToUpper()) ||
                        x.BagNumber.ToUpper().Contains(input.Keyword.ToUpper()));
                }

                // Lọc theo trạng thái
                if (input.Status.HasValue && input.Status > 0)
                {
                    query = query.Where(x => x.ShippingStatus == input.Status);
                }

                // Lọc theo ngày tạo
                if (input.StartCreateDate.HasValue)
                {
                    query = query.Where(x => x.CreationTime.Date >= input.StartCreateDate.Value.Date);
                }
                if (input.EndCreateDate.HasValue)
                {
                    query = query.Where(x => x.CreationTime.Date <= input.EndCreateDate.Value.Date);
                }

                // Lấy tổng số lượng
                var totalCount = query.Count();

                // Áp dụng sắp xếp và phân trang
                query = ApplySorting(query, input);
                query = ApplyPaging(query, input);

                // Lấy dữ liệu
                var packages = await query.ToListAsync();

                // Trả về kết quả
                return new PagedResultDto<PackageDto>
                {
                    Items = ObjectMapper.Map<List<PackageDto>>(packages),
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        public async Task<List<PackageDto>> GetPackagesByOrderIds(List<long> orderIds)
        {
            var query = (await Repository.GetAllAsync())
                .Where(x => orderIds.Contains(x.OrderId ?? 0));
            var data = ObjectMapper.Map<List<PackageDto>>(query.ToList());
            return data;
        }

        public async Task<List<PackageDownloadByBagDto>> GetPackageDownloadByBag(BagDownloadFileRequestDto input)
        {
            try
            {
                var permissionCheckResult = GetPermissionCheckerWithCustomerIds();
                if (permissionCheckResult.permissionCase < 0)
                {
                    return new List<PackageDownloadByBagDto>();
                }

                var currentUserwarehouseId = _pbtAppSession.WarehouseId ?? 0;

                var customerIdsParam = string.Join(",", permissionCheckResult.CustomerIds ?? new List<long>());
                var data = await ConnectDb.GetListAsync<PackageDownloadByBagDto>(
                    "SP_Package_GetPackageDataForDownloadByBag",
                    CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@FilterType", ""),
                        new SqlParameter("@StartCreateDate", input.StartCreateDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndCreateDate", input.EndCreateDate ?? (object)DBNull.Value),
                        new SqlParameter("@Status", input.Status ),
                        new SqlParameter("@StartImportDate", input.StartImportDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndImportDate", input.EndImportDate ?? (object)DBNull.Value),
                        new SqlParameter("@StartExportDate", input.StartExportDate ?? (object)DBNull.Value),
                        new SqlParameter("@EndExportDate", input.EndExportDate ?? (object)DBNull.Value),
                        new SqlParameter("@WarehouseCreate", input.WarehouseCreate ?? (object)DBNull.Value),
                        new SqlParameter("@WarehouseDestination", input.WarehouseDestination ?? (object)DBNull.Value),
                        new SqlParameter("@BagType", input.BagType ),
                        new SqlParameter("@Keyword", input.Keyword ?? ""),
                        new SqlParameter("@ShippingPartnerId", input.ShippingPartnerId ?? (object)DBNull.Value),
                        new SqlParameter("@CustomerId", input.CustomerId ?? -1),
                        new SqlParameter("@PermissionCase", permissionCheckResult.permissionCase),
                        new SqlParameter("@CurrentUserWarehouseId", currentUserwarehouseId),
                        new SqlParameter("@CustomerIds", customerIdsParam ),
                    });
                return data;
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách kiện để tải file theo bao", ex);
                return new List<PackageDownloadByBagDto>();
            }
        }


        public async Task<List<PackageDto>> GetPackagesByOrderId(long orderId)
        {
            var query = (await Repository.GetAllListAsync(x => x.OrderId == orderId));
            var data = ObjectMapper.Map<List<PackageDto>>(query.ToList());
            return data;
        }


        public async Task<List<PackageDto>> GetByDeliveryNoteIdAsync(int deliveryNoteId, bool inclusiveBagOnly = true)
        {

            if (inclusiveBagOnly)
            {
                var query = (await Repository.GetAllAsync())
                   .Where(x => x.DeliveryNoteId == deliveryNoteId)
                   .Include(x => x.DeliveryNote)
                   .Include(x => x.Bag)
                    .Where(x => x.Bag == null || x.Bag.BagType == (int)BagTypeEnum.InclusiveBag)
                     .Select(x => new PackageDto()
                     {
                         Id = x.Id,
                         PackageNumber = x.PackageNumber,
                         TrackingNumber = x.TrackingNumber,
                         Quantity = x.Quantity,
                         Weight = x.Weight,
                         Length = x.Length,
                         Width = x.Width,
                         Height = x.Height,
                         Note = x.Note,
                         DeliveryNoteCode = x.DeliveryNote.DeliveryNoteCode,
                         CustomerId = x.CustomerId,
                     });

                return query.ToList();
            }

            else
            {
                var query = (await Repository.GetAllAsync())
                 .Where(x => x.DeliveryNoteId == deliveryNoteId)
                 .Include(x => x.DeliveryNote)

                   .Select(x => new PackageDto()
                   {
                       Id = x.Id,
                       PackageNumber = x.PackageNumber,
                       TrackingNumber = x.TrackingNumber,
                       Quantity = x.Quantity,
                       Weight = x.Weight,
                       Length = x.Length,
                       Width = x.Width,
                       Height = x.Height,
                       Note = x.Note,
                       DeliveryNoteCode = x.DeliveryNote.DeliveryNoteCode,
                       CustomerId = x.CustomerId,
                   });

                return query.ToList();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        /// 
        [HttpPut]
        public async Task MarkAsDeliveredAsync(long packageId)
        {
            // Lấy thông tin kiện hàng
            var package = await Repository.GetAll()
                .FirstOrDefaultAsync(p => p.Id == packageId);

            if (package == null)
            {
                throw new UserFriendlyException("Không tìm thấy kiện hàng.");
            }

            // Kiểm tra trạng thái kiện hàng
            if (package.ShippingStatus != (int)PackageDeliveryStatusEnum.DeliveryInProgress)
            {
                throw new UserFriendlyException("Chỉ có thể đánh dấu kiện hàng đang giao là đã giao.");
            }

            // Cập nhật trạng thái kiện hàng
            package.ShippingStatus = (int)PackageDeliveryStatusEnum.Delivered;
            package.ReceivedTime = DateTime.Now;

            // Tìm đơn hàng theo kiện
            var order = await _orderRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == package.OrderId);
            if (order != null)
            {
                // Cập nhật trạng thái đơn hàng
                order.OrderStatus = (int)OrderStatus.Delivered;
                order.DeliveredTime = DateTime.Now;
            }
            // Lưu thay đổi
            await CurrentUnitOfWork.SaveChangesAsync();
        }


        [HttpPut]
        public async Task MarkAsCompletedAsync(long packageId)
        {
            // Lấy thông tin kiện hàng
            var package = await Repository.GetAll()
                .FirstOrDefaultAsync(p => p.Id == packageId);

            if (package == null)
            {
                throw new UserFriendlyException("Không tìm thấy kiện hàng.");
            }

            // Kiểm tra trạng thái kiện hàng
            if (package.ShippingStatus != (int)PackageDeliveryStatusEnum.Delivered)
            {
                throw new UserFriendlyException("Chỉ có thể đánh dấu kiện hàng đang giao là đã giao.");
            }

            // Cập nhật trạng thái kiện hàng
            package.ShippingStatus = (int)PackageDeliveryStatusEnum.Completed;
            package.CompletedTime = DateTime.Now;

            // Tìm đơn hàng theo kiện
            var order = await _orderRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == package.OrderId);
            if (order != null)
            {
                // Cập nhật trạng thái đơn hàng
                order.OrderStatus = (int)OrderStatus.OrderCompleted;
                order.OrderCompletedTime = DateTime.Now;
            }
            // Lưu thay đổi
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        [Authorize]
        [HttpDelete]
        public override async Task DeleteAsync(EntityDto<int> input)
        {
            try
            {

                Logger.Info($"Bắt đầu xóa kiện hàng với ID: {input.Id}");
                // Kiểm tra xem kiện hàng có tồn tại không
                var package = await GetByIdAsync(input.Id);
                if (package == null)
                {
                    throw new UserFriendlyException("Không tìm thấy kiện hàng với ID: " + input.Id);
                }

                if (package.BagId.HasValue && package.BagId > 0)
                {
                    throw new UserFriendlyException("Không thể xóa kiện hàng đã được đóng vào bao. Vui lòng tháo kiện khỏi bao trước khi xóa.");
                }

                if (package.DomesticShippingFeeRMB > 0 && package.ShippingStatus <= (int)PackageDeliveryStatusEnum.InWarehouseVN)
                {
                    // Tạo phiếu thu hoàn tiền phí ship nội địa
                    // Lấy ra tài khoản fundAccount
                    var currentFund = (await _fundAccountAppService.GetFundAccountsByCurrentUserAsync()).FirstOrDefault();
                    if (currentFund == null)
                    {
                        Logger.Error("[DeleteAsync] Không tìm thấy tài khoản quỹ để ghi nhận phí vận chuyển nội địa.");
                        throw new UserFriendlyException("Không tìm thấy tài khoản quỹ để ghi nhận phí vận chuyển nội địa.");
                    }
                    var gd = await _identityCodeAppService.GenerateNewSequentialNumberAsync("GD");

                    var transaction = new Transaction
                    {
                        TransactionId = gd.Code,
                        OrderId = package.TrackingNumber,
                        Amount = package.DomesticShippingFeeRMB ?? 0,
                        TransactionContent = $"Hoàn tiền Phí vận chuyển nội địa khi xóa kiện hàng {package.PackageNumber} - ({package.TrackingNumber})",
                        TransactionDirection = (int)TransactionDirectionEnum.Receipt,
                        TransactionType = (int)TransactionTypeEnum.Deposit,
                        FundAccountId = currentFund.Id,
                        TotalAmount = currentFund.TotalAmount + (package.DomesticShippingFeeRMB ?? 0),
                        Currency = currentFund.Currency,
                        ExecutionSource = (int)TransactionSourceEnum.Manual,
                        Status = (int)TransactionStatusEnum.PendingApprove,
                        ExpensePurpose = "Hoàn tiền ship nội địa xóa kiện",
                        RecipientPayer = package.CustomerId
                    };
                    await _transactionRepository.InsertAsync(transaction);
                    currentFund.TotalAmount = currentFund.TotalAmount + (package.DomesticShippingFeeRMB ?? 0);

                    await _fundAccountAppService.UpdateAsync(currentFund);
                }

                Logger.Info($"Xóa kiện hàng với ID: {input.Id}, PackageNumber: {package.PackageNumber}");

                await base.DeleteAsync(input);

                // Cập nhật lại đơn hàng
                //SP_Orders_UpdateSummaryFromPackages @OrderId
                await ConnectDb.ExecuteNonQueryAsync("SP_Orders_UpdateSummaryFromPackages", CommandType.StoredProcedure,
                    new[]
                    {
                    new SqlParameter("@OrderId", package.OrderId )
                    });

                if (package.CreatorUserId != AbpSession.UserId.Value)
                {
                    RemovePackageFromCache(input.Id, AbpSession.UserId.Value);
                }
                else
                {
                    RemovePackageFromCache(input.Id);
                }

                // =========================
                // 7. PACKAGE AUDIT LOG
                // =========================
                var packageAuditLog = new EntityAuditLogDto()
                {

                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Delete.ToString(),

                    Title = $"Xóa kiện #{package.Id} - {package.PackageNumber} - {package.TrackingNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),

                };


            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw ex;
            }

        }


        public async Task<List<PackageDto>> GetPackagesInVietnamWarehouseAsync(long customerId, string excludedPackageIds)
        {
            var query = (await Repository.GetAllAsync()).Include(x => x.Bag)
                   .Where(p => p.CustomerId == customerId
                            && p.ShippingStatus == (int)PackageDeliveryStatusEnum.InWarehouseVN
                            && !p.DeliveryNoteId.HasValue
                            )
                      .Where(x => x.Bag == null || x.Bag.BagType == (int)BagTypeEnum.InclusiveBag)
                   ;


            if (!string.IsNullOrEmpty(excludedPackageIds))
            {
                var ids = excludedPackageIds.Split(',')
                 .Select(id => int.TryParse(id, out var parsedId) ? parsedId : 0)
                 .Where(id => id > 0)
                 .ToList();
                query = query.Where(p => !ids.Contains(p.Id));
            }

            var packages = await query.ToListAsync();
            return ObjectMapper.Map<List<PackageDto>>(packages);
        }

        public async Task<List<PackageDto>> GetByIdsAsync(List<int> ids)
        {
            var idsParam = string.Join(",", ids);
            return await GetByIdsAsync(idsParam);
        }

        private async Task<List<PackageDto>> GetByIdsAsync(string ids)
        {
            return await ConnectDb.GetListAsync<PackageDto>("SP_Packages_GetByIds",
                CommandType.StoredProcedure, new[] { new SqlParameter() {
                ParameterName = "@PackageIds", Value = ids
            } });
        }

        public async Task<PackageDto> GetByIdAsync(int id)
        {
            var package = await ConnectDb.GetItemAsync<PackageDto>(
                "SP_Packages_GetById",
                CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@Id", id)
                });
            return package;

        }

        public async Task<PackageEditByAdminDto> GetForAdminEditByIdAsync(int id)
        {
            var package = await ConnectDb.GetItemAsync<PackageEditByAdminDto>(
                "SP_Packages_GetById",
                CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@Id", id)
                });
            return package;

        }

        /// <summary>
        /// Lấy danh sách các kiện theo DeliveryNoteId
        /// </summary>
        /// <param name="deliveryNoteId">ID của phiếu xuất kho</param>
        /// <returns>Danh sách các kiện</returns>
        public async Task<List<PackageDto>> GetPackagesByDeliveryNoteIdAsync(long deliveryNoteId)
        {
            // Lấy danh sách kiện hàng theo DeliveryNoteId
            var packages = await Repository.GetAllListAsync(u => u.DeliveryNoteId == deliveryNoteId);

            if (!packages.Any())
            {
                throw new UserFriendlyException($"Không tìm thấy kiện hàng nào thuộc phiếu xuất kho ID: {deliveryNoteId}");
            }

            // Ánh xạ dữ liệu sang DTO
            return ObjectMapper.Map<List<PackageDto>>(packages);
        }

        [Authorize]
        public async Task<JsonResult> UnBag(int packageId)
        {
            // Lấy thông tin kiện hàng trước khi unbag
            var package = await GetByIdAsync(packageId);
            if (package == null)
            {
                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = -404,
                    Message = "Không tìm thấy kiện hàng.",

                });
            }

            if (!package.BagId.HasValue || package.BagId <= 0)
            {
                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = -400,
                    Message = "Kiện hàng không thuộc bao nào.",
                    Data = (object)null
                });
            }


            var oldBagId = package.BagId.Value;

            // Lấy bag để audit
            var bag = await _bagRepository.FirstOrDefaultAsync(b => b.Id == oldBagId);

            var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Package_UnBag", CommandType.StoredProcedure,
                   new[]
                   {
                       new SqlParameter("@id", packageId)
                   }
                );

            _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
               package,
               null,
               "unbagged",
               $"Bỏ kiện hàng khỏi bao #{package.BagId}",
               true
           );

            // =========================
            // 4. PACKAGE AUDIT LOG
            // =========================

            package = await GetByIdAsync(packageId);

            var packageAuditLog = new EntityAuditLogDto()
            {
                EntityId = package.Id.ToString(),
                EntityType = nameof(Package),
                MethodName = EntityAuditLogMethodName.Update.ToString(),
                Title = $"Kiện #{package.Id} - {package.PackageNumber} được bỏ khỏi bao #{oldBagId} - {bag.BagCode}",
                UserId = _pbtAppSession.UserId,
                UserName = _pbtAppSession.UserName,
                Data = JsonConvert.SerializeObject(package),

            };

            await _entityAuditLogApiClient.SendAsync(packageAuditLog);

            bag = await _bagRepository.FirstOrDefaultAsync(b => b.Id == oldBagId);
            var bagDto = ObjectMapper.Map<BagDto>(bag);
            var bagAuditLog = new EntityAuditLogDto()
            {

                EntityId = bagDto.Id.ToString(),
                EntityType = nameof(Bag),
                MethodName = EntityAuditLogMethodName.Update.ToString(),

                Title = $"Bao #{bagDto.Id} - {bagDto.BagCode} gỡ kiện #{package.PackageNumber} - {package.TrackingNumber}",
                UserId = _pbtAppSession.UserId,
                UserName = _pbtAppSession.UserName,
                Data = JsonConvert.SerializeObject(bagDto),

            };

            await _entityAuditLogApiClient.SendAsync(bagAuditLog);

            return new JsonResult(new
            {
                Success = true,
                StatusCode = 200,
                Message = "Bỏ kiện hàng khỏi bao thành công.",
                Data = excuteResult
            });
        }


        public async Task<JsonResult> AddToBagAsync(int packageId, int bagId)
        {
            try
            {
                // Lấy thông tin kiện hàng
                var package = await GetByIdAsync(packageId);

                if (package == null)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        StatusCode = -404,
                        Message = "Không tìm thấy kiện hàng."
                    });
                }

                // Kiểm tra xem kiện hàng đã thuộc bao nào chưa
                if (package.BagId.HasValue)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        StatusCode = -405,
                        Message = "Kiện hàng đã thuộc bao khác."
                    });
                }


                var bag = await _bagRepository.FirstOrDefaultAsync(b => b.Id == bagId);
                if (bag == null)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        StatusCode = -406,
                        Message = "Không tìm thấy bao với ID: " + bagId
                    });
                }

                await ConnectDb.ExecuteNonQueryAsync("SP_Package_AddToBag", System.Data.CommandType.StoredProcedure, new[]{
                    new SqlParameter("@Id", SqlDbType.Int) { Value = package.Id },
                    new SqlParameter("@BagId", SqlDbType.Int) { Value = bag.Id },
                    new SqlParameter("@PackageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.WaitingForShipping },
                    new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock }
                    }
               );

                if (package.CreationTime.Date == DateTime.Now.Date)
                {
                    // xóa khỏi tạo bao nhanh
                    RemovePackageFromCache(package.Id);
                }

                // Ghi log cho kiện hàng đã được thêm vào bao
                _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                    package,
                    null,
                    "added_to_bag",
                    $"Thêm kiện hàng vào bao #{bagId}",
                    true
                );

                var packageAuditLog = new EntityAuditLogDto()
                {
                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Kiện hàng #{package.Id} - {package.PackageNumber} thêm vào bao #{bagId} - {bag.BagCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),
                };

                await _entityAuditLogApiClient.SendAsync(packageAuditLog);

                /// =======================
                /// ✅ BỔ SUNG BAG AUDIT LOG TẠI ĐÂY
                /// =======================

                bag = await _bagRepository.FirstOrDefaultAsync(b => b.Id == bagId);
                var bagDto = ObjectMapper.Map<BagDto>(bag);
                var bagAuditLog = new EntityAuditLogDto()
                {

                    EntityId = bagDto.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Bao #{bagId} - {bagDto.BagCode} được thêm kiện #{package.Id} - {package.PackageNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bagDto),

                };

                await _entityAuditLogApiClient.SendAsync(bagAuditLog);

                // Trả về kết quả
                return new JsonResult(new
                {
                    Success = true,
                    StatusCode = -200,
                    Message = "Đã thêm kiện hàng vào bao thành công.",

                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw ex;
            }

        }


        public async Task<List<OptionItemDto>> GetCustomersByDateSelect(PagedPackageResultRequestDto input)
        {
            var permissionCheckResult = GetPermissionCheckerWithCustomerIds();
            if (permissionCheckResult.permissionCase < 0)
            {
                return new List<OptionItemDto>();
            }
            var currentUserwarehouseId = _pbtAppSession.WarehouseId ?? 0;
            var customerIdsParam = string.Join(",", permissionCheckResult.CustomerIds ?? new List<long>());
            return await ConnectDb.GetListAsync<OptionItemDto>(
                "SP_Customers_GetListByPackageFilters",
                CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@PermissionCase", permissionCheckResult.permissionCase),
                    new SqlParameter("@CurrentUserWarehouseId", currentUserwarehouseId),
                    new SqlParameter("@CustomerIds", customerIdsParam),
                    new SqlParameter("@Keyword",  input.Keyword),
                    new SqlParameter("@ExcludeCoverWeightType", input.ExcludeCoverWeightType),
                    new SqlParameter("@UnBagType", input.UnBagType ?? -1),
                    new SqlParameter("@FeatureId", input.FeatureId ?? -1),
                    new SqlParameter("@ServiceId", input.ServiceId ?? -1),
                    new SqlParameter("@WarehouseId", input.WarehouseId ?? -1),
                    new SqlParameter("@WarehouseDestinationId", input.WarehouseDestinationId ?? -1),
                    new SqlParameter("@WarehouseStatus", input.WarehouseStatus ?? -1),
                    new SqlParameter("@ShippingStatus", input.ShippingStatus ?? -1),
                    new SqlParameter("@ShippingLine", input.ShippingLine ?? -1),
                    new SqlParameter("@Status", input.Status ?? -1),
                    new SqlParameter("@FilterType", -1),
                    new SqlParameter("@BagType", input.BagType ?? -1),
                    new SqlParameter("@StartCreateDate", input.StartCreateDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndCreateDate", input.EndCreateDate ?? (object)DBNull.Value),
                    new SqlParameter("@StartExportDate", input.StartExportDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndExportDate", input.EndExportDate ?? (object)DBNull.Value),
                    new SqlParameter("@StartDeliveryDate", input.StartDeliveryDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndDeliveryDate", input.EndDeliveryDate ?? (object)DBNull.Value),
                    new SqlParameter("@StartImportDate", input.StartImportDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndImportDate", input.EndImportDate ?? (object)DBNull.Value),
                    new SqlParameter("@StartMatchDate", input.StartMatchDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndMatchDate", input.EndMatchDate ?? (object)DBNull.Value)
                });
        }

        public async Task<JsonResult> RemoveDeliveryNote(int packageId, int deliveryNoteId)
        {
            try
            {
                var package = await GetByIdAsync(packageId);
                if (package == null)
                {
                    throw new UserFriendlyException("Không tìm thấy kiện hàng với ID: " + packageId);
                }
                if (package.DeliveryNoteId != deliveryNoteId)
                {
                    throw new UserFriendlyException("Kiện hàng không thuộc phiếu xuất kho này.");
                }

                var deliveryNote = await _deliveryNoteRepository.FirstOrDefaultAsync(d => d.Id == deliveryNoteId);
                if (deliveryNote == null)
                {
                    throw new UserFriendlyException("Không tìm thấy phiếu xuất kho với ID: " + deliveryNoteId);
                }


                await ConnectDb.ExecuteNonQueryAsync("SP_Packages_RemoveToDelivery", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@PackageId", package.Id),
                        new SqlParameter("@DeliveryNoteId", package.DeliveryNoteId),
                        new SqlParameter("@packageStatus", (int)PackageDeliveryStatusEnum.InWarehouseVN)
                    }
                );
                // Ghi log cho kiện hàng đã được bỏ phiếu xuất kho
                _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                    package,
                    null,
                    "removed_delivery_note",
                    $"Bỏ kiện hàng #{package.PackageNumber} khỏi phiếu xuất #{deliveryNote.DeliveryNoteCode}",
                    true
                );

                var packageAuditLog = new EntityAuditLogDto()
                {

                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Kiện hàng #{package} - {package.TrackingNumber} xóa khỏi phiếu xuất #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),

                };
                await _entityAuditLogApiClient.SendAsync(packageAuditLog);


                deliveryNote = await _deliveryNoteRepository.FirstOrDefaultAsync(d => d.Id == deliveryNoteId);
                var deliveryNoteAuditLog = new EntityAuditLogDto()
                {

                    EntityId = deliveryNote.Id.ToString(),
                    EntityType = nameof(DeliveryNote),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Phiếu xuất kho #{deliveryNote.DeliveryNoteCode} xóa kiện hàng #{package.PackageNumber} - {package.TrackingNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(deliveryNote),

                };

                await _entityAuditLogApiClient.SendAsync(deliveryNoteAuditLog);

                return new JsonResult(new
                {
                    Success = true,

                    Message = "Đã bỏ phiếu xuất kho khỏi kiện hàng thành công.",
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw ex;
            }
        }


        public Task<PackageDto> GetPackageByTrackingNumber(string trackingNumber)
        {
            return ConnectDb.GetItemAsync<PackageDto>("sp_Package_GetByTrackingNumber",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@trackingNumber",
                    Value = trackingNumber
                } });
        }

        #region Private Methods

        /// <summary>
        /// 1. Admin
        /// 2. Sale Admin
        /// 3. Sale
        /// 4. Customer
        /// 5. WarehouseCN
        /// 6. WarehouseVN
        /// </summary>
        /// <returns></returns>
        private (int permissionCase, List<long> CustomerIds) GetPermissionCheckerWithCustomerIds()
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
                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleCustom))
                {
                    permissionCase = 7;
                    Logger.Info($"Sale truy cập vào danh sách kiện hàng. SaleId: {_pbtAppSession.CustomerId}");
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

        private Task<OrderDto> GetOrderByTrackingNumber(string trackingNumber)
        {
            return ConnectDb.GetItemAsync<OrderDto>("SP_Orders_GetByTrackingNumberPending",
                System.Data.CommandType.StoredProcedure,
                new[] {
                     new SqlParameter() {
                    ParameterName = "@trackingNumber",
                    Value = trackingNumber
                }
                });
        }

        private Task<CustomerWithAddressDto> GetCustomerWithAddressById(long customerId)
        {
            return ConnectDb.GetItemAsync<CustomerWithAddressDto>("sp_Customer_GetById_WithAddress",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@CustomerId",
                    Value = customerId
                } });
        }

        private async Task<CustomerFakeDto> GetRandomCustomerFake()
        {
            return await ConnectDb.GetItemAsync<CustomerFakeDto>("sp_CustomerFake_GetRandom", System.Data.CommandType.StoredProcedure);
        }

        private async Task UpdateOrderSummaryFromPackages(long orderId)
        {
            try
            {
                await ConnectDb.ExecuteNonQueryAsync("SP_Orders_UpdateSummaryFromPackages", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@OrderId", orderId)
                }
            );
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi cập nhật tổng quan đơn hàng từ kiện hàng. OrderId: {orderId}, Error: {ex.Message}", ex);
            }

        }

        private async Task UpdateBagSummary(long bagId)
        {
            try
            {
                await ConnectDb.ExecuteNonQueryAsync("SP_BAG_UpdateWeightAndFee", CommandType.StoredProcedure, new[]{
                                new SqlParameter("@BagId", SqlDbType.Int) { Value = bagId }
                            }
                 );
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi cập nhật thông tin tài chính của bao: " + ex.Message, ex);
            }
        }

        private async Task<WarehouseNameAndCodeDto> GetWarehouseByCustomerId(long customerId)
        {

            var data = await ConnectDb.GetItemAsync<WarehouseNameAndCodeDto>("SP_Warehouses_GetByCustomerId", CommandType.StoredProcedure,
                new[]
                {
                     new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customerId },
                });

            return data;
        }


        #endregion

        [AbpAuthorize(PermissionNames.Function_Package_Finance)]
        public async Task<PackageFinanceDto> GetWithFinanceAsync(int id)
        {
            var data = await Repository.FirstOrDefaultAsync(x => x.Id == id);
            return ObjectMapper.Map<PackageFinanceDto>(data);
        }

        [HttpPost]
        [AbpAuthorize(PermissionNames.Function_Package_Finance)]
        public async Task<JsonResult> UpdateFinanceAsync(PackageFinanceDto input)
        {
            try
            {
                // Lấy thông tin kiện hàng từ database
                var package = await Repository.FirstOrDefaultAsync(input.Id);
                if (package == null)
                {
                    throw new UserFriendlyException("Không tìm thấy kiện hàng.");
                }

                // Cập nhật thông tin tài chính
                package.Weight = input.Weight;
                package.Length = input.Length;
                package.Width = input.Width;
                package.Height = input.Height;

                package.Price = input.Price;
                package.InsuranceValue = input.InsuranceValue;
                package.InsuranceFee = input.IsInsured ? (input.InsuranceValue * package.Price / 100) : 0; // Phí bảo hiểm = Giá trị bảo hiểm * 1%
                package.WoodenPackagingFee = input.IsWoodenCrate && input.WoodenPackagingFee > 0 ? input.WoodenPackagingFee : 0; // Phí đóng gỗ
                package.ShockproofFee = input.IsShockproof && input.ShockproofFee > 0 ? input.ShockproofFee : 0; // Phí chống sốc
                package.DomesticShippingFee = input.IsDomesticShipping && input.DomesticShippingFee > 0 ? input.DomesticShippingFee.Value : 0; // Phí vận chuyển nội địa
                package.DomesticShippingFeeRMB = input.IsDomesticShipping && input.DomesticShippingFeeRMB > 0 ? input.DomesticShippingFeeRMB.Value : 0; // Phí vận chuyển nội địa

                package.IsInsured = input.IsInsured;
                package.IsWoodenCrate = input.IsWoodenCrate;
                package.IsShockproof = input.IsShockproof;
                package.IsDomesticShipping = input.IsDomesticShipping;

                // Tính tổng phí dịch vụ
                package.TotalFee = input.TotalFee;
                var totalServiceFee = (package.InsuranceFee ?? 0) +
                                   (package.WoodenPackagingFee ?? 0) +
                                   (package.ShockproofFee ?? 0) +
                                   (package.DomesticShippingFee);

                // Tính tổng phí (Tổng phí dịch vụ + Phí vận chuyển)
                package.TotalPrice = package.TotalFee + totalServiceFee;

                // Lưu thay đổi vào database
                await Repository.UpdateAsync(package);

                if (package.OrderId.HasValue && package.OrderId > 0)
                {
                    await UpdateOrderSummaryFromPackages(package.OrderId.Value);
                    var order = await _orderRepository.FirstOrDefaultAsync(package.OrderId.Value);

                    // Cập nhật tài chính đơn do thay đổi tài chính kiện
                    var orderDto = ObjectMapper.Map<OrderDto>(order);
                    var orderAuditLog = new EntityAuditLogDto()
                    {

                        EntityId = order.Id.ToString(),
                        EntityType = nameof(Order),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),

                        Title = $"Cập nhật tài chính đơn hàng #{orderDto.Id} - {orderDto.WaybillNumber} do thay đổi tài chính kiện hàng #{package.Id} - {package.PackageNumber}",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(orderDto),
                    };

                    await _entityAuditLogApiClient.SendAsync(orderAuditLog);

                }
                if (package.BagId.HasValue && package.BagId > 0)
                {
                    await UpdateBagSummary(package.BagId.Value);

                }
                // Ghi log thay đổi

                await _entityChangeLoggerAppService.LogChangeAsync<Package>(
                    null,
                    package,
                    "Cập nhật thông tin tài chính",
                    $"Cập nhật tài chính cho kiện hàng {package.PackageNumber}");

                var packageAuditLog = new EntityAuditLogDto()
                {

                    EntityId = package.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),

                    Title = $"Cập nhật tài chính kiện hàng #{package.Id} - {package.TrackingNumber} khi tạo kiện từ đơn hàng #{package.Id} - {package.PackageNumber}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(package),

                };
                await _entityAuditLogApiClient.SendAsync(packageAuditLog);

                return new JsonResult(new
                {
                    Success = true,
                    Message = "Cập nhật thông tin tài chính thành công."
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi cập nhật thông tin tài chính: " + ex.Message, ex);
                throw new UserFriendlyException("Đã xảy ra lỗi khi cập nhật thông tin tài chính. Vui lòng thử lại.");
            }
        }

        [HttpPut]
        public async Task EditStatus(EditPackageStatusInputDto input)
        {
            var package = await Repository.GetAsync(input.Id);
            if (package == null)
            {
                throw new UserFriendlyException("Không tìm thấy kiện hàng.");
            }
            var oldStatus = package.ShippingStatus;
            package.ShippingStatus = input.ShippingStatus;
            switch (input.ShippingStatus)
            {
                case (int)PackageDeliveryStatusEnum.Delivered:
                    package.ReceivedTime = DateTime.Now;
                    break;
                case (int)PackageDeliveryStatusEnum.Completed:
                    package.CompletedTime = DateTime.Now;
                    break;
            }
            await CurrentUnitOfWork.SaveChangesAsync();
            Logger.Info($"Cập nhật trạng thái kiện hàng {package.Id} từ {oldStatus} sang {input.ShippingStatus}");
            // add log 
            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                null,
                package,
                "Cập nhật trạng thái kiện hàng",
                $"Cập nhật trạng thái kiện hàng {package.PackageNumber} từ {oldStatus} sang {input.ShippingStatus}");
        }


        private async Task<int> InsertPreparedPackageViaSpAsync(PackageDto preparedPackage)
        {
            if (preparedPackage == null)
                throw new ArgumentNullException(nameof(preparedPackage));

            var newPackageIdParam = new SqlParameter("@NewPackageId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await ConnectDb.ExecuteNonQueryAsync(
                "SP_Packages_CreateSingle",
                CommandType.StoredProcedure,
                new[]
                {
            new SqlParameter("@WaybillNumber", preparedPackage.WaybillNumber),
            new SqlParameter("@TrackingNumber", preparedPackage.TrackingNumber),
            new SqlParameter("@PackageNumber", preparedPackage.PackageNumber),

            new SqlParameter("@ProductNameCn", (object?)preparedPackage.ProductNameCn ?? DBNull.Value),
            new SqlParameter("@ProductNameVi", (object?)preparedPackage.ProductNameVi ?? DBNull.Value),
            new SqlParameter("@ProductLink", (object?)preparedPackage.ProductLink ?? DBNull.Value),
            new SqlParameter("@ProductGroupTypeId", preparedPackage.ProductGroupTypeId),
            new SqlParameter("@Quantity", preparedPackage.Quantity),
            new SqlParameter("@PriceCN", preparedPackage.PriceCN),

            new SqlParameter("@Weight", preparedPackage.Weight),
            new SqlParameter("@Length", preparedPackage.Length),
            new SqlParameter("@Width", preparedPackage.Width),
            new SqlParameter("@Height", preparedPackage.Height),

            new SqlParameter("@IsWoodenCrate", preparedPackage.IsWoodenCrate),
            new SqlParameter("@IsShockproof", preparedPackage.IsShockproof),
            new SqlParameter("@IsDomesticShipping", preparedPackage.IsDomesticShipping),
            new SqlParameter("@IsInsured", preparedPackage.IsInsured),
            new SqlParameter("@ShippingLineId", preparedPackage.ShippingLineId),

            new SqlParameter("@Price", preparedPackage.Price),
            new SqlParameter("@InsuranceFee", preparedPackage.InsuranceFee),
            new SqlParameter("@TotalPrice", preparedPackage.TotalPrice),
            new SqlParameter("@DomesticShippingFee", preparedPackage.DomesticShippingFee),
            new SqlParameter("@DomesticShippingFeeRMB", preparedPackage.DomesticShippingFeeRMB),
            new SqlParameter("@ShockproofFee", preparedPackage.ShockproofFee),
            new SqlParameter("@TotalFee", preparedPackage.TotalFee),
            new SqlParameter("@UnitPrice", preparedPackage.UnitPrice),
            new SqlParameter("@UnitType", preparedPackage.UnitType),
            new SqlParameter("@WoodenPackagingFee", preparedPackage.WoodenPackagingFee),

            new SqlParameter("@OrderId", preparedPackage.OrderId),
            new SqlParameter("@WarehouseId", preparedPackage.WarehouseId),
            new SqlParameter("@WarehouseCreateId", preparedPackage.WarehouseCreateId),
            new SqlParameter("@WarehouseDestinationId", preparedPackage.WarehouseDestinationId),

            new SqlParameter("@CustomerId", preparedPackage.CustomerId),
            new SqlParameter("@CustomerFakeId", (object?)preparedPackage.CustomerFakeId ?? DBNull.Value),

            new SqlParameter("@WarehouseStatus", preparedPackage.WarehouseStatus),
            new SqlParameter("@ShippingStatus", preparedPackage.ShippingStatus),
            new SqlParameter("@IsQuickBagging", preparedPackage.IsQuickBagging),

            new SqlParameter("@ParentId", (object?)preparedPackage.ParentId ?? DBNull.Value),

            new SqlParameter("@CreatorUserId", AbpSession.UserId),

            newPackageIdParam
                }
            );

            return (int)newPackageIdParam.Value;
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

        private async Task<OrderDto> GetOrderById(long orderId)
        {
            return await ConnectDb.GetItemAsync<OrderDto>("SP_Orders_GetById", CommandType.StoredProcedure, new[] { new SqlParameter() {
                ParameterName = "@Id", Value = orderId
            } });
        }



    }
}