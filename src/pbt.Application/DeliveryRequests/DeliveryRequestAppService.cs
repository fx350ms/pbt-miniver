using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.Commons.Dto;
using pbt.Core;
using pbt.CustomerAddresss.Dto;
using pbt.Customers.Dto;
using pbt.DeliveryNotes.Dto;
using pbt.DeliveryRequests.Dto;
using pbt.Entities;
using pbt.OrderNumbers;
using pbt.Orders.Dto;
using pbt.Packages.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace pbt.DeliveryRequests
{
    [AbpAuthorize(PermissionNames.Pages_DeliveryRequest)]
    [Audited]
    public class DeliveryRequestAppService :
        AsyncCrudAppService<DeliveryRequest, DeliveryRequestDto, int, PagedDeliveryRequestsResultRequestDto,
            DeliveryRequestDto, DeliveryRequestDto>, IDeliveryRequestAppService
    {
        private IRepository<Order, long> _orderRepository;
        private IRepository<DeliveryRequestOrder, int> _deliveryRequestOrderRepository;
        private IRepository<Customer, long> _customersRepository;
        private IRepository<CustomerAddress, long> _customerAddressRepository;
        private IRepository<Package, int> _packageRepository;
        private IRepository<DeliveryNote, int> _deliveryNoteRepository;
        private IRepository<CustomerTransaction, long> _walletTransactionRepository;
        IIdentityCodeAppService _identityCodeAppService;
        private readonly pbtAppSession _pbtAppSession;
        private IRepository<Bag, int> _bagRepository;
        private IRepository<Warehouse, int> _warehousesRepository;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly string[] _roles;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeliveryRequestAppService(
            IRepository<DeliveryRequest, int> repository,
            IRepository<Order, long> orderRepository,
            IRepository<DeliveryRequestOrder, int> deliveryRequestOrderRepository,
            IRepository<Customer, long> customersRepository,
            IRepository<CustomerAddress, long> customerAddressRepository,
            IRepository<Package, int> packageRepository,
            IIdentityCodeAppService identityCodeAppService,
            IRepository<CustomerTransaction, long> walletTransactionRepository,
            pbtAppSession pbtAppSession,
            IRepository<DeliveryNote, int> deliveryNoteRepository,
            IRepository<Bag, int> bagRepository,
            IRepository<Warehouse, int> warehouseRepository,
            IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IHttpContextAccessor httpContextAccessor

        ) : base(repository)
        {
            _orderRepository = orderRepository;
            _deliveryRequestOrderRepository = deliveryRequestOrderRepository;
            _customersRepository = customersRepository;
            _customerAddressRepository = customerAddressRepository;
            _packageRepository = packageRepository;
            _identityCodeAppService = identityCodeAppService;
            _walletTransactionRepository = walletTransactionRepository;
            _pbtAppSession = pbtAppSession;
            _deliveryNoteRepository = deliveryNoteRepository;
            _bagRepository = bagRepository;
            _warehousesRepository = warehouseRepository;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _httpContextAccessor = httpContextAccessor;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
        }

        public static T DeepCopy<T>(T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }


        public async Task<DeliveryRequestDto> CreateDeliveryRequestAsync(CreateUpdateDeliveryRequestDto input)
        {
            try
            {
                if (input.Orders.Count <= 0 && input.Bags.Count <= 0) throw new Exception("");
                //var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == input.Orders.First());
                //if (order == null) return false;

                var orders = new List<Order>();
                decimal totalAmount = 0;
                int totalPackage = 0;
                decimal bagCoverFee = 0;
                if (input.Bags.Count > 0)
                {
                    var bag = (await _bagRepository.GetAllAsync())
                        .Include(x => x.Packages).ThenInclude(x => x.Order)
                        .Where(x => input.Bags.Contains(x.Id));
                    bagCoverFee = bag.Sum(x => x.WeightCoverFee ?? 0);
                    orders.AddRange(bag.Select(x => x.Packages.FirstOrDefault().Order).ToList());
                }
                totalAmount += bagCoverFee;
                if (input.Orders.Count > 0)
                {
                    orders.AddRange(await _orderRepository.GetAllListAsync(x => input.Orders.Contains(x.Id)));
                }
                long addressId = -1;

                foreach (var order in orders)
                {
                    //kiểm tra trạng thái của order, chỉ lấy order có trạng thái là ở kho việt nam
                    // nếu có order không hợp lệ thì throw exception 
                    if (input.AddressId > 0)
                    {
                        addressId = input.AddressId;
                    }
                    else
                    {
                        if (addressId == -1)
                        {
                            addressId = order.AddressId ?? 0;
                        }
                    }

                    // Tính tổng tiền đơn hàng 
                    totalAmount += order.TotalCost;
                    totalPackage += order.PackageCount ?? 0;
                }

                var requestCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("YCG");
                var deliveryRequest = new DeliveryRequestDto
                {
                    RequestCode = requestCode.Code,
                    CustomerId = input.CustomerId,
                    Status = (int)DeliveryRequestStatus.New,
                    ShippingMethod = input.ShippingMethod,
                    AddressId = addressId,
                    Note = input.Note,
                    PaymentMethod = (int)PaymentMethod.Debt,
                    PaymentStatus = (int)PaymentStatus.Unpaid,
                    TotalAmount = totalAmount,
                    TotalPackage = totalPackage,
                };
                var dtoResult = await CreateAsync(deliveryRequest); //await Repository.InsertAndGetIdAsync(deliveryRequest);
                var deliveryRequestOrders = orders.Select(orderId => new DeliveryRequestOrder
                {
                    DeliveryRequestId = dtoResult.Id,
                    OrderId = orderId.Id
                }).ToList();

                // update delivery request id to bags and packages
                foreach (var bagId in input.Bags)
                {
                    var bag = await _bagRepository.FirstOrDefaultAsync(x => x.Id == bagId);
                    bag.DeliveryRequestId = dtoResult.Id;
                    await _bagRepository.UpdateAsync(bag);
                }
                // get package from order
                var packages = (await _packageRepository.GetAllAsync())
                    .Where(x => input.Orders.Contains(x.OrderId ?? 0));
                foreach (var package in packages)
                {
                    package.DeliveryRequestId = dtoResult.Id;
                    await _packageRepository.UpdateAsync(package);
                }

                await _deliveryRequestOrderRepository.InsertRangeAsync(deliveryRequestOrders);
                return dtoResult;
            }
            catch (Exception ex)
            {
                Logger.Error("Error creating delivery request", ex);
                throw new Exception("Error creating delivery request", ex);
            }
        }

        public async Task<PagedResultDto<DeliveryRequestDto>> GetDeliveryRequestFilter(
            PagedDeliveryRequestsResultRequestDto input)
        {
            // get current role
            var currentRole = PermissionChecker.IsGranted(PermissionNames.Role_Customer);
            List<long> AllCustomers = new List<long>();
            // get current customer id
            if (AbpSession.UserId != null)
            {
                var currentCustomerId = _pbtAppSession.CustomerId;
                if (currentCustomerId != null)
                {
                    AllCustomers.Add((long)currentCustomerId);
                }
                // get all child customer
                var childCustomers = (await _customersRepository.GetAllListAsync(x => x.ParentId == currentCustomerId)).Select(x => x.Id).ToList();
                AllCustomers.AddRange(childCustomers);
            }
            var query = (await Repository.GetAllAsync());
            if (currentRole)
            {
                query = query.Where(x => AllCustomers.Contains(x.CustomerId));
            }

            if ((_roles.Contains(RoleConstants.sale) || _roles.Contains(RoleConstants.saleadmin)) && !_roles.Contains(RoleConstants.admin)
               )
            {
                var userId = AbpSession.UserId;
                List<long> saleIds = new List<long>();
                if (_roles.Contains(RoleConstants.saleadmin))
                {
                    saleIds = (await _customersRepository.GetAllAsync()).Where(x => x.SaleId == userId).Select(y => y.Id).ToList();
                }
            }

            // get current user warehouse id
            var warehouseId = _pbtAppSession.WarehouseId;

            if (_roles.Contains(RoleConstants.warehouseVN) && !_roles.Contains(RoleConstants.admin))
            {
            }

            //.WhereIf(!string.IsNullOrEmpty(input.Code), u => u.RequestCode.ToString().Contains(input.Code))
            //.WhereIf(input.Status.HasValue, u => u.Status == input.Status);
            if (input.Status != null && input.Status > 0)
            {
                query = query.Where(u => u.Status == input.Status);
            }
            if (!string.IsNullOrEmpty(input.Code))
            {
                query = query.Where(u => u.RequestCode.ToUpper().Contains(input.Code.ToUpper()));
            }

            if (input.CustomerId.HasValue)
            {
                query = query.Where(u => u.CustomerId == input.CustomerId);
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime >= input.StartDate);
            }
            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime <= input.EndDate);
            }

            int total = query.Count();
            // query = query.Take(input.MaxResultCount).Skip(input.SkipCount);
            query = query.OrderByDescending(x => x.Id);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            var data = query.ToList();

            var customerIds = new List<long>();
            var customerAddressIds = new List<long>();
            foreach (var deliveryRequest in data)
            {
                customerIds.Add(deliveryRequest.CustomerId);
                customerAddressIds.Add(deliveryRequest.AddressId);
            }

            var customers = _customersRepository.GetAll().Where(x => customerIds.Contains(x.Id)).ToList();
            var customerAddresses = _customerAddressRepository.GetAll().Where(x => customerAddressIds.Contains(x.Id)).ToList();


            var packageDto = ObjectMapper.Map<List<DeliveryRequestDto>>(data);
            var result = new List<DeliveryRequestDto>(packageDto);

            foreach (var deliveryRequest in result)
            {
                var customer = customers.FirstOrDefault(x => x.Id == deliveryRequest.CustomerId);
                var address = customerAddresses.FirstOrDefault(x => x.Id == deliveryRequest.AddressId);

                //deliveryRequest.Weight = GetTotalWeightDeliveryRequest(deliveryRequest.Id);
                //deliveryRequest.TotalPackage = GetTotalPackageDeliveryRequest(deliveryRequest.Id);
            }

            return new PagedResultDto<DeliveryRequestDto>()
            {
                Items = result,
                TotalCount = total,
            };
        }

        public async Task<int> GetNewDeliveryRequest()
        {
            var currentRole = PermissionChecker.IsGranted(PermissionNames.Role_Customer);
            List<long> AllCustomers = new List<long>();
            // get current customer id
            if (AbpSession.UserId != null)
            {
                var currentCustomerId = _pbtAppSession.CustomerId;
                if (currentCustomerId != null)
                {
                    AllCustomers.Add((long)currentCustomerId);
                }
                // get all child customer
                var childCustomers = (await _customersRepository.GetAllListAsync(x => x.ParentId == currentCustomerId)).Select(x => x.Id).ToList();
                AllCustomers.AddRange(childCustomers);
            }
            var query = (await Repository.GetAllAsync())
                .Where(u => u.Status == (int)DeliveryRequestStatus.New);


            if (currentRole)
            {
                query = query.Where(x => AllCustomers.Contains(x.CustomerId));
            }
            // get current user warehouse id

            return query.Count();

        }

        public decimal? GetTotalWeightDeliveryRequest(int deliveryRequestId)
        {
            return _deliveryRequestOrderRepository.GetAll()
                .Where(x => x.DeliveryRequestId == deliveryRequestId)
                .Join(_packageRepository.GetAll(),
                    dro => dro.OrderId,
                    p => p.OrderId,
                    (dro, p) => p.Weight)
                .Sum();
        }

        public int? GetTotalPackageDeliveryRequest(int deliveryRequestId)
        {
            return _deliveryRequestOrderRepository.GetAll()
                .Where(x => x.DeliveryRequestId == deliveryRequestId)
                .Join(_packageRepository.GetAll(),
                    dro => dro.OrderId,
                    p => p.OrderId,
                    (dro, p) => p.Id)
                .Count();
        }

        public async Task<PagedResultDto<PackageDto>> GetPackages(PagedPackagesResultRequestDto input)
        {
            var query = _deliveryRequestOrderRepository.GetAll()
                .Where(x => x.DeliveryRequestId == input.DeliveryRequestId)
                .Join(_packageRepository.GetAll(),
                    dro => dro.OrderId,
                    p => p.OrderId,
                    (dro, p) => p);
            int total = query.Count();

            query = query.Take(input.MaxResultCount).Skip(input.SkipCount);
            var data = ObjectMapper.Map<List<PackageDto>>(query.ToList());

            return new PagedResultDto<PackageDto>()
            {
                Items = data,
                TotalCount = total,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deliveryRequestId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> CancelDeliveryRequest(int Id)
        {
            return await UpdateDeliveryRequest(Id, (int)DeliveryRequestStatus.Cancel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<JsonResult> UpdateDeliveryRequest(int id, int status)
        {
            try
            {
                var deliveryRequest = await Repository.GetAsync(id);
                // lấy danh sách bao và kiện của deliveryRequest này
                var deliveryRequestOrders = await _deliveryRequestOrderRepository
                    .GetAllListAsync(dro => dro.DeliveryRequestId == id);
                var lstOrders = deliveryRequestOrders.Select(x => x.OrderId).ToList();
                var packages = (await _packageRepository.GetAllListAsync())
                    .Where(x => x.OrderId != null && lstOrders.Contains((long)x.OrderId)).ToList();
                foreach (var package in packages)
                {
                    var deliveryNote = await _deliveryNoteRepository.FirstOrDefaultAsync(x => x.Id == package.DeliveryNoteId);
                    if (deliveryNote != null && deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                    {
                        return new JsonResult(new { success = false, message = "Không thể hủy yêu cầu giao hàng vì một số gói hàng đã được giao." });
                    }
                }
                if (deliveryRequest == null)
                {
                    return new JsonResult(new { success = false, message = "Yêu cầu giao hàng không tồn tại." });
                }
                deliveryRequest.Status = status;
                await Repository.UpdateAsync(deliveryRequest);
                return new JsonResult(new { success = true, message = "Yêu cầu giao hàng đã được cập nhật thành công." });
            }
            catch (Exception ex)
            {
                Logger.Error("Error canceling delivery request", ex);
                return new JsonResult(new { success = false, message = "Đã xảy ra lỗi khi hủy yêu cầu giao hàng." });
            }
        }


        public async Task<PagedResultDto<ItemInHouseVnDto>> GetOrdersInVietnamWarehouseByCustomerId(PagedResultRequestByCustomerDto input)
        {
            // get package in house vn
            var packageQuery = await _packageRepository.GetAllAsync();
            packageQuery = packageQuery
                .Include(x => x.Bag)
                .Include(x => x.Order)
                .Where(u => u.CustomerId == input.CustomerId
                            && u.ShippingStatus == (int)PackageDeliveryStatusEnum.InWarehouseVN
                            && u.Bag != null && u.Bag.BagType == (int)BagTypeEnum.InclusiveBag
                            // không có kiện nào trong delivery request
                            && !_deliveryRequestOrderRepository.GetAll()
                                .Any(dro => dro.OrderId == u.OrderId)
                );
            var count = packageQuery.Count();
            packageQuery = packageQuery.OrderByDescending(x => x.Id);
            var packageList = await packageQuery.ToListAsync();

            var data = ObjectMapper.Map<List<PackageDto>>(packageList);

            var orderIds = data.Select(x => x.Id).ToList();

            var warehouses = await _warehousesRepository.GetAll()
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var addresses = await _customerAddressRepository.GetAll()
                .ToDictionaryAsync(x => x.Id, x => x.FullAddress);

            var result = new List<ItemInHouseVnDto>();
            foreach (var item in data)
            {
                result.Add(new ItemInHouseVnDto()
                {
                    CodeType = "package",
                    ItemCode = item.TrackingNumber,
                    Id = item.Id,
                    TotalCost = item.TotalPrice ?? 0,
                    VNWarehouseName = item.WarehouseId != null && warehouses.ContainsKey(item.WarehouseId.Value) ? warehouses[item.WarehouseId.Value] : "",
                    PackageCount = 1,

                });
            }

            // get bag in vn
            var bags = await (await _bagRepository.GetAllAsync())
                .Include(x => x.Packages).ThenInclude(x => x.Order)
                .Where(x => x.CustomerId == input.CustomerId)
                .Where(x => x.BagType == (int)BagTypeEnum.SeparateBag)
                .Where(x => x.DeliveryNoteId == null)
                .Where(x => x.ShippingStatus == (int)BagShippingStatus.GoToWarehouse)
                // điều kiện không có kiện nào trong bag của delivery request
                .Where(x => !_deliveryRequestOrderRepository.GetAll()
                    .Any(dro => x.Packages != null && dro.OrderId == x.Packages.FirstOrDefault().OrderId))
                .ToListAsync();
            var bagDtos = ObjectMapper.Map<List<BagDto>>(bags);
            var customerAddresses = (await _customerAddressRepository.GetAllAsync())
                .FirstOrDefault(x => x.CustomerId == input.CustomerId && x.IsDefault);
            foreach (var bagDto in bagDtos)
            {
                var totalPackages = bags.FirstOrDefault(x => x.Id == bagDto.Id)?.Packages;
                if (totalPackages != null) bagDto.TotalPackages = totalPackages.Count();
                var itemBag = new ItemInHouseVnDto()
                {
                    ItemCode = bagDto.BagCode,
                    CodeType = "bag",
                    VNWarehouseName = bagDto.WarehouseDestinationName,
                    PackageCount = bagDto.TotalPackages,
                    AddressName = customerAddresses?.FullAddress,
                    Id = bagDto.Id
                };
                if (totalPackages != null)
                {
                    decimal totalBagCost = 0;
                    foreach (var package in totalPackages)
                    {
                        //var order = package.Order;
                        //if (order != null)
                        //{
                        //    itemBag.AmountDue += order.AmountDue;
                        //    itemBag.TotalCost += order.TotalCost;
                        //    itemBag.Paid += order.Paid;
                        //}
                        totalBagCost += package.TotalPrice ?? 0;

                    }
                    itemBag.AmountDue += totalBagCost + (bagDto.WeightCoverFee ?? 0);
                    itemBag.TotalCost += totalBagCost + (bagDto.WeightCoverFee ?? 0);
                }
                result.Add(itemBag);
            }

            return new PagedResultDto<ItemInHouseVnDto>()
            {
                Items = result,
                TotalCount = count
            };
        }


        public async Task<JsonResult> GetByCustomerAndWarehouseIdAsync(long customerId, int warehouseId)
        {

            var deliveryRequestIdOutputParam = new SqlParameter
            {
                ParameterName = "@DeliveryRequestId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };
            var requestCodeOutputParam = new SqlParameter
            {
                ParameterName = "@RequestCode",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = 50,
                Direction = System.Data.ParameterDirection.Output
            };
            var statusCodeOutputParam = new SqlParameter
            {
                ParameterName = "@StatusCode",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };
            var messageOutputParam = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = -1,
                Direction = System.Data.ParameterDirection.Output
            };

            await ConnectDb.ExecuteNonQueryAsync("SP_DeliveryRequests_GetOrCreateDraft", System.Data.CommandType.StoredProcedure,
                new[]{
                    new SqlParameter(){ ParameterName = "@CustomerId", Value = customerId , SqlDbType = SqlDbType.BigInt },
                    new SqlParameter(){ ParameterName = "@WarehouseId", Value = warehouseId , SqlDbType = SqlDbType.Int },
                    deliveryRequestIdOutputParam,
                    requestCodeOutputParam,
                    statusCodeOutputParam,
                    messageOutputParam
                }
            );

            var statusCode = (int)statusCodeOutputParam.Value;
            var message = messageOutputParam.Value.ToString();
            if (statusCode <= 0)
            {
                return new JsonResult(new { success = false, message = message });
            }

            var deliveryRequestId = (int)deliveryRequestIdOutputParam.Value;
            var deliveryCode = requestCodeOutputParam.Value.ToString();
            return new JsonResult(new
            {
                success = true,
                data = new
                {
                    Id = deliveryRequestId,
                    RequestCode = deliveryCode
                }
            });
        }


        public async Task<PagedResultDto<DeliveryRequestItemDto>> GetDeliveryRequestItemsByRequestId(DeliveryRequestInputItemInputDto input)
        {
            var data = await ConnectDb.GetListAsync<DeliveryRequestItemDto>(
                "SP_DeliveryRequestItems_GetByRequestId",
                System.Data.CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter(){ ParameterName = "@RequestId", Value = input.DeliveryRequestId, SqlDbType = SqlDbType.Int },
                }
            );
            return new PagedResultDto<DeliveryRequestItemDto>()
            {
                Items = data,
                TotalCount = data.Count()
            };
        }

        public async Task<PagedResultDto<BagPackageDto>> GetDeliveryRequestItemsForCreateRequestAsync(BagPackageTransferRequestDto input)
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
                    "SP_BagsPackages_GetItemsForCreateRequest",
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


        public async Task<JsonResult> AddItemToDeliveryRequestAsync(DeliveryRequestItemDto item)
        {

            var statusCodeOutputParam = new SqlParameter
            {
                ParameterName = "@StatusCode",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };
            var messageOutputParam = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = -1,
                Direction = System.Data.ParameterDirection.Output
            };
            await ConnectDb.ExecuteNonQueryAsync("SP_DeliveryRequests_AddItem", System.Data.CommandType.StoredProcedure,
                new[]{
                    new SqlParameter(){ ParameterName = "@DeliveryRequestId", Value = item.DeliveryRequestId , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@ItemId", Value = item.ItemId , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@ItemType", Value = item.ItemType , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@CreatorUserId", Value = AbpSession.UserId ?? (object)DBNull.Value , SqlDbType = SqlDbType.BigInt },
                    statusCodeOutputParam,
                    messageOutputParam
                }
            );

            var statusCode = (int)statusCodeOutputParam.Value;
            var message = messageOutputParam.Value.ToString();
            if (statusCode <= 0)
            {
                return new JsonResult(new { success = false, message = message });
            }
            return new JsonResult(new { success = true, message = "Thêm vật phẩm vào yêu cầu giao hàng thành công." });
        }

        public async Task<JsonResult> RemoveItemFromDeliveryRequestAsync(int deliveryRequestItemId)
        {
            await ConnectDb.ExecuteNonQueryAsync("SP_DeliveryRequests_RemoveItem", System.Data.CommandType.StoredProcedure,
                new[]{
                    new SqlParameter(){ ParameterName = "@DeliveryRequestItemId", Value = deliveryRequestItemId , SqlDbType = SqlDbType.Int }
                }
            );

            return new JsonResult(new { success = true, message = "Đã xóa khỏi yêu cầu giao" });
        }

        public async Task<JsonResult> SubmitDeliveryRequest(SubmitDeliveryRequestDto input)
        {
            var statusCodeOutputParam = new SqlParameter
            {
                ParameterName = "@StatusCode",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };
            var messageOutputParam = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size = -1,
                Direction = System.Data.ParameterDirection.Output
            };

            await ConnectDb.ExecuteNonQueryAsync("SP_DeliveryRequests_SubmitRequest", System.Data.CommandType.StoredProcedure,
                new[]{
                    new SqlParameter(){ ParameterName = "@Id", Value = input.Id , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@ShippingMethod", Value = input.ShippingMethod , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@Address", Value = input.Address , SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter(){ ParameterName = "@Note", Value = input.Note ?? (object)DBNull.Value , SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter(){ ParameterName = "@SubmitUserId", Value = AbpSession.UserId ?? (object)DBNull.Value , SqlDbType = SqlDbType.BigInt },
                    new SqlParameter(){ ParameterName = "@Status", Value = DeliveryRequestStatus.Submited , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@BagStatus", Value = (int) BagShippingStatus.DeliveryRequest , SqlDbType = SqlDbType.Int },
                    new SqlParameter(){ ParameterName = "@PackageStatus", Value = (int) PackageDeliveryStatusEnum.DeliveryRequest , SqlDbType = SqlDbType.Int },
                    statusCodeOutputParam,
                    messageOutputParam
                }
            );

            var statusCode = (int)statusCodeOutputParam.Value;
            var message = messageOutputParam.Value.ToString();
            if (statusCode <= 0)
            {
                return new JsonResult(new { success = false, message = message });
            }
            return new JsonResult(new { success = true, message = "Gửi yêu cầu giao hàng thành công." });
        }
    }
}