using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.Commons.Dto;
using pbt.ConfigurationSettings;
using pbt.Core;
using pbt.Customers;
using pbt.Customers.Dto;
using pbt.DeliveryRequests.Dto;
using pbt.Entities;
using pbt.OrderHistories;
using pbt.OrderHistories.Dto;
using pbt.OrderLogs;
using pbt.OrderLogs.Dto;
using pbt.Orders.Dto;
using pbt.Packages;
using pbt.Packages.Dto;
using pbt.ShippingRates;
using pbt.ShippingRates.Dto;
using pbt.Waybills;
using pbt.Waybills.Dto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace pbt.Orders
{

    public class OrderAppService : AsyncCrudAppService<Order, OrderDto, long, PagedResultRequestDto, CreateUpdateOrderDto, OrderDto>, IOrderAppService
    {
        private readonly IAbpSession _abpSession;
        private pbtAppSession _pbtAppSession;
        public IOrderHistoryAppService _orderHistoryService;
        public IOrderLogAppService _orderLogService;
        public ICustomerAppService _customerService;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Warehouse> _warehousesRepository;
        private readonly IRepository<CustomerAddress, long> _customerAddressRepository;
        private readonly IRepository<DeliveryRequestOrder, int> _deliveryRequestOrderRepository;
        private readonly IRepository<OrderNote, long> _orderNoteRepository;
        private readonly IRepository<Customer, long> _customerRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _roles;
        // public IWaybillAppService _waybillAppService;
        private readonly IShippingCostAppService _shippingCostService;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly IConfigurationSettingAppService _configurationSettingAppService;

        public OrderAppService(IRepository<Order, long> repository,
            IRepository<Package> packageRepository,
            IRepository<Warehouse> warehousesRepository,
            IRepository<CustomerAddress, long> customerAddressRepository,
            IRepository<DeliveryRequestOrder, int> deliveryRequestOrderRepository,
            IOrderHistoryAppService orderHistoryService,
            IOrderLogAppService orderLogService,
            ICustomerAppService customerService,
            IPackageAppService packageAppService,
             IRepository<OrderNote, long> orderNoteRepository,
            //   IWaybillAppService waybillAppService,
            IHttpContextAccessor httpContextAccessor,
            pbtAppSession pbtAppSession,
            IShippingCostAppService shippingCostService,
             IAbpSession abpSession,
            IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IConfigurationSettingAppService configurationSettingAppService,
            IRepository<Customer, long> customerRepository
            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;
            _orderHistoryService = orderHistoryService;
            _orderLogService = orderLogService;
            _customerService = customerService;
            _packageRepository = packageRepository;
            _warehousesRepository = warehousesRepository;
            _customerAddressRepository = customerAddressRepository;
            _deliveryRequestOrderRepository = deliveryRequestOrderRepository;
            _orderNoteRepository = orderNoteRepository;
            //  _waybillAppService = waybillAppService;
            _httpContextAccessor = httpContextAccessor;
            _shippingCostService = shippingCostService;
            _abpSession = abpSession;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _roles = _httpContextAccessor.HttpContext.User.Claims
           .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
           .Select(c => c.Value)
           .ToArray();
            _configurationSettingAppService = configurationSettingAppService;
            _customerRepository = customerRepository;
        }

        public async Task<List<CustomerDto>> GetCustomerFilter(PagedAndSortedOrderResultRequestDto input)
        {
            var data = await _customerRepository.GetAllAsync();
            var currentUserId = AbpSession.UserId;
            var query = base.CreateFilteredQuery(input);

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
            else
            {
                var currentCustomerId = _pbtAppSession.CustomerId ?? -1;
                // Get all child customers
                var childCustomers = await _customerService.GetChildren(currentCustomerId);
                var childCustomerIds = childCustomers?.Select(c => c.Id).ToList() ?? new List<long>();
                if (currentCustomerId > 0)
                {
                    childCustomerIds.Add(currentCustomerId);
                }
                query = query.Where(x => childCustomerIds.Contains(x.CustomerId.Value) || x.CustomerId == currentCustomerId || x.CreatorUserId == currentUserId);
            }

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                x.OrderNumber.ToUpper().Contains(input.Keyword.ToUpper())
                || x.WaybillNumber.ToUpper().Contains(input.Keyword.ToUpper()));
            }
            if (input.ShippingLine.HasValue && input.ShippingLine > 0)
            {
                query = query.Where(x => x.ShippingLine == input.ShippingLine);
            }

            if (input.CustomerId.HasValue && input.CustomerId > 0)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId);
            }
            if (input.OrderType > 0)
            {
                //   query = query.Where(x => (input.OrderType == 2 && (x.IsCustomerOrder.HasValue && x.IsCustomerOrder.Value)) || (input.OrderType == 2 && !x.IsCustomerOrder));
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime.Date >= input.StartDate.Value.Date);
            }
            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime.Date <= input.EndDate.Value.Date);
            }
            if (input.Status > 0)
            {
                query = query.Where(x => x.OrderStatus == input.Status);
            }
            var orders = await query.ToListAsync();
            var customersIds = orders.Select(x => x.CustomerId ?? 0).ToList();
            return ObjectMapper.Map<List<CustomerDto>>(data.Where(x => customersIds.Contains(x.Id)));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<AllMyOrderItemDto>> GetAllMyOrders(PagedMyOrderRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            try
            {
                var permissionCase = 1;

                var customerDtoIds = new List<CustomerIdDto>();
                // admin và sale admin sẽ nhìn thấy tất cả đơn hàng
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin))
                {
                    permissionCase = 1;
                    Logger.Info("Admin hoặc Sale Admin truy cập vào danh sách đơn hàng.");
                }

                // Sale admin nhìn thấy tất cả các đơn hàng của mình, của sale dưới quyền và của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                    Logger.Info("Sale admin truy cập danh sách đơn hàng");
                    customerDtoIds = await ConnectDb.GetListAsync<CustomerIdDto>("SP_Customers_GetIdsBySaleAdminUserId", CommandType.StoredProcedure,
                    new[]
                    {
                    new SqlParameter("@SaleAdminUserId", currentUserId)
                    });
                }
                // sale chỉ nhìn thấy đơn hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 3;
                    Logger.Info($"Sale truy cập vào danh sách đơn hàng. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = await ConnectDb.GetListAsync<CustomerIdDto>(
                   "SP_Customers_GetIdsBySaleId",
                   CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }
                // Khách hàng chỉ nhìn thấy đơn hàng của mình và khách hàng con
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 4;
                    Logger.Info($"Customer truy cập vào danh sách đơn hàng. CustomerId: {_pbtAppSession.CustomerId}");
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
                    permissionCase = 5;
                    return new PagedResultDto<AllMyOrderItemDto>
                    {
                        Items = new List<AllMyOrderItemDto>(),
                        TotalCount = 0
                    };
                }
                var customerIds = new List<long>();
                if (customerDtoIds != null)
                {
                    customerIds = customerDtoIds.Select(u => u.CustomerId).ToList();
                }

                // Tạo query để lấy danh sách kiện hàng
                var query = base.CreateFilteredQuery(input)
                    // .Include(x => x.Customer) // Bao gồm thông tin khách hàng
                    // .Where(x => x.ParentId == null)
                    .Where(x =>
                       (permissionCase == 1) || // admin   nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4)
                       && customerIds.Contains(x.CustomerId ?? 0)) // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x =>
                        x.WaybillNumber.ToUpper().Contains(input.Keyword.Trim().ToUpper()));
                }
                if (input.Status > 0)
                {
                    query = query.Where(x => x.OrderStatus == input.Status);
                }
                if (input.ServiceId > 0)
                {
                    if (input.ServiceId == 1)
                    {
                        query = query.Where(x => x.IsWoodenCrate || x.UseWoodenPackaging || x.WoodenPackagingFee > 0);
                    }
                    else if (input.ServiceId == 2)
                    {
                        query = query.Where(x => x.UseShockproofPackaging || x.ShockproofFee > 0);
                    }
                    else if (input.ServiceId == 3)
                    {
                        query = query.Where(x => x.IsDomesticShipping || x.UseDomesticTransportation || x.DomesticShipping > 0 || x.DomesticShippingFee > 0);
                    }
                }

                if (input.ShippingLine.HasValue && input.ShippingLine > 0)
                {
                    query = query.Where(x => x.ShippingLine == input.ShippingLine);
                }
                if (input.CustomerId.HasValue && input.CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }

                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime.Date >= input.StartCreateDate.Value.Date);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime.Date <= input.EndCreateDate.Value.Date);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.InTransitTime.HasValue && x.InTransitTime.Value.Date >= input.StartExportDate.Value.Date);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.InTransitTime.HasValue && x.InTransitTime.Value.Date <= input.EndExportDate.Value.Date);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.InTransitToVietnamWarehouseTime.HasValue && x.InTransitToVietnamWarehouseTime.Value.Date >= input.StartImportDate.Value.Date);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.InTransitToVietnamWarehouseTime.HasValue && x.InTransitToVietnamWarehouseTime.Value.Date <= input.EndImportDate.Value.Date);
                }

                if (input.IsExportExcel)
                {
                    var count = query.Count();

                    query.Include(x => x.Customer);
                    var data = ObjectMapper.Map<List<AllMyOrderItemDto>>(query.ToList());

                    var orderIds = string.Join(",", data.Select(x => x.Id).ToList());
                    // Lấy danh sách BagNumber theo OrderIds
                    var bagNumberList = await ConnectDb.GetListAsync<BagNumberByOrderIdDto>(
                     "SP_Packages_GetBagNumberListByOrderIds",
                     CommandType.StoredProcedure,
                      new[]
                     {
                      new SqlParameter("@OrderIds", orderIds)
                     });

                    var orderBagNumbersDict = bagNumberList.GroupBy(x => x.OrderId)
                        .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(b => b.BagNumbers).ToList()));

                    // Gán BagNumber vào data
                    foreach (var item in data)
                    {
                        if (orderBagNumbersDict.ContainsKey(item.Id))
                        {
                            item.BagNumbers = orderBagNumbersDict[item.Id];
                        }
                        else
                        {
                            item.BagNumbers = string.Empty;
                        }
                    }
                    return new PagedResultDto<AllMyOrderItemDto>()
                    {
                        Items = data,
                        TotalCount = count
                    };
                }
                else
                {
                    var count = query.Count();
                    query = query.OrderByDescending(u => u.Id).Skip(input.SkipCount).Take(input.MaxResultCount);
                    query.Include(x => x.Customer);
                    var data = ObjectMapper.Map<List<AllMyOrderItemDto>>(query.ToList());

                    return new PagedResultDto<AllMyOrderItemDto>()
                    {
                        Items = data,
                        TotalCount = count
                    };

                }


            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách đơn hàng của tôi.", ex);
                throw ex;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<PackageWithOrderDto>> GetAllPackageByMyOrders(PagedMyOrderRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            try
            {
                var permissionCase = 1;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var customerDtoIds = new List<CustomerIdDto>();
                // admin và sale admin sẽ nhìn thấy tất cả đơn hàng
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin) || await PermissionChecker.IsGrantedAsync(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 1;
                    Logger.Info("Admin hoặc Sale Admin truy cập vào danh sách kiện hàng.");
                }
                // sale chỉ nhìn thấy đơn hàng của khách hàng do mình quản lý
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
                // Khách hàng chỉ nhìn thấy đơn hàng của mình và khách hàng con
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
                    return new List<PackageWithOrderDto>();
                }
                var customerIds = new List<long>();
                if (customerDtoIds != null)
                {
                    customerIds = customerDtoIds.Select(u => u.CustomerId).ToList();
                }
                var query = base.CreateFilteredQuery(input)
                // Tạo query để lấy danh sách kiện hàng
                 .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3) && customerIds.Contains(x.CustomerId ?? 0)) // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    );

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x =>
                        x.WaybillNumber.ToUpper().Contains(input.Keyword.Trim().ToUpper()));
                }
                if (input.Status > 0)
                {
                    query = query.Where(x => x.OrderStatus == input.Status);
                }

                if (input.ShippingLine.HasValue && input.ShippingLine > 0)
                {
                    query = query.Where(x => x.ShippingLine == input.ShippingLine);
                }
                if (input.CustomerId.HasValue && input.CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }

                if (input.StartCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime.Date >= input.StartCreateDate.Value.Date);
                }
                if (input.EndCreateDate != null)
                {
                    query = query.Where(x => x.CreationTime.Date <= input.EndCreateDate.Value.Date);
                }

                if (input.StartExportDate != null)
                {
                    query = query.Where(x => x.InTransitTime.HasValue && x.InTransitTime.Value.Date >= input.StartExportDate.Value.Date);
                }
                if (input.EndExportDate != null)
                {
                    query = query.Where(x => x.InTransitTime.HasValue && x.InTransitTime.Value.Date <= input.EndExportDate.Value.Date);
                }

                if (input.StartImportDate != null)
                {
                    query = query.Where(x => x.InTransitToVietnamWarehouseTime.HasValue && x.InTransitToVietnamWarehouseTime.Value.Date >= input.StartImportDate.Value.Date);
                }
                if (input.EndImportDate != null)
                {
                    query = query.Where(x => x.InTransitToVietnamWarehouseTime.HasValue && x.InTransitToVietnamWarehouseTime.Value.Date <= input.EndImportDate.Value.Date);
                }

                var orderIds = string.Join(",", query.Select(x => x.Id).ToList());

                var packages = await ConnectDb.GetListAsync<PackageWithOrderDto>(
                     "SP_Packages_GetPackagesWithOrderByOrderIds",
                     CommandType.StoredProcedure,
                      new[]
                     {
                      new SqlParameter("@OrderIds", orderIds)
                     });

                return packages;
                //// var count = query.Count();
                // query = query.OrderByDescending(u => u.Id).Skip(input.SkipCount).Take(input.MaxResultCount);

                // query.Include(x => x.Customer);
                // var data = ObjectMapper.Map<List<OrderDto>>(query.ToList());

                // sw.Stop();
                // Logger.Info($"Thời gian thực thi GetAllMyOrders: {sw.ElapsedMilliseconds} ms");
                // return new PagedResultDto<AllMyOrderItemDto>()
                // {
                //     Items = data,
                //     TotalCount = count
                // };

            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách đơn hàng của tôi.", ex);
                throw ex;
            }

        }

        /// <summary>
        /// Lấy danh sách đơn hàng của tôi để chọn
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<OptionItemDto>> GetAllMyWaybillForSelection(GetAllMyWaybillForSelectionRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            var query = await Repository.GetAllAsync();
            if (PermissionChecker.IsGranted(PermissionNames.Function_ViewAllOrder)
                || _roles.Contains(RoleConstants.sale)
                || _roles.Contains(RoleConstants.saleadmin)
                )
            {
            }
            else
            {
                var currentCustomerId = _pbtAppSession.CustomerId ?? -1;
                // Get all child customers
                var childCustomers = await _customerService.GetChildren(currentCustomerId);
                var childCustomerIds = childCustomers?.Select(c => c.Id).ToList() ?? new List<long>();
                if (currentCustomerId > 0)
                {
                    childCustomerIds.Add(currentCustomerId);
                }
                query = query.Where(x => childCustomerIds.Contains(x.CustomerId.Value) || x.CustomerId == currentCustomerId || x.CreatorUserId == currentCustomerId);
                // Default: only orders created by current user
                //query = query.Where(x => x.CreatorUserId == currentUserId || );
            }

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                x.OrderNumber.ToUpper().Contains(input.Keyword.ToUpper())
                || x.WaybillNumber.ToUpper().Contains(input.Keyword.ToUpper()));
            }

            if (input.Status > 0)
            {
                query = query.Where(x => x.OrderStatus == input.Status);
            }

            var data = await query.Select(x => new OptionItemDto
            {
                id = x.Id.ToString(),
                text = x.WaybillNumber,
            }).ToListAsync();

            return data;
        }

        public async Task<PagedResultDto<OrderDto>> GetAllBySaleAsync(PagedAndSortedOrderResultRequestDto input)
        {
            if (!_roles.Contains(RoleConstants.admin) && !_roles.Contains(RoleConstants.saleadmin) && !_roles.Contains(RoleConstants.sale))
            {
                return new PagedResultDto<OrderDto>()
                {
                    Items = new List<OrderDto>(),
                    TotalCount = 0
                };
            }

            var query = base.CreateFilteredQuery(input);

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                x.OrderNumber.ToUpper().Contains(input.Keyword.ToUpper())
                || x.WaybillNumber.ToUpper().Contains(input.Keyword.ToUpper()));
            }
            if (input.CustomerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId);
            }
            if (input.OrderType > 0)
            {
                // query = query.Where(x => (input.OrderType == 2 && x.IsCustomerOrder) || (input.OrderType == 2 && !x.IsCustomerOrder));
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime.Date >= input.StartDate.Value.Date);
            }
            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime.Date <= input.EndDate.Value.Date);
            }
            if (input.Status > 0)
            {
                query = query.Where(x => x.OrderStatus == input.Status);
            }
          ;

            var count = query.Count();
            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            return new PagedResultDto<OrderDto>()
            {
                Items = query.ToList().MapTo<List<OrderDto>>(),
                TotalCount = count
            };
        }

        public override Task<PagedResultDto<OrderDto>> GetAllAsync(PagedResultRequestDto input)
        {
            return base.GetAllAsync(input);
        }

        public async Task<JsonResult> SyncWeightAndFeeAsync(int id)
        {
            var order = await Repository.GetAsync(id);
            if (order == null)
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy đơn hàng" });
            }


            await ConnectDb.ExecuteNonQueryAsync(
                "SP_Orders_UpdateSummaryFromPackages",
                CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@OrderId", id)
                });

            return new JsonResult(new { success = true, message = "Đồng bộ thành công" });
        }

        public async Task<OrderDto> CreateMyOrderAsync(CreateUpdateOrderDto input)
        {

            input.OrderType = (int)OrderType.Regular;
            input.OrderStatus = (int)OrderStatus.Sent;
            input.CustomerId = _pbtAppSession.CustomerId.Value;
            if (input.AddressId <= 0)
            {
                throw new Exception("Bạn chưa chọn địa chỉ");
            }
            // Nếu user hiện tại là Sale thì gán UserId hiện tại vào saleId, gán customerId bởi input
            // Nếu user hiện tại không phải Sale thì tìm sale tương ứng -> gán vào
            if (_pbtAppSession.CustomerId.HasValue)
            {
                // Đây là trường hợp tài khoản là khách hàng 
                var customer = await _customerService.GetAsync(new EntityDto<long>(_pbtAppSession.CustomerId.Value));
                if (customer != null)
                {
                    input.UserSaleId = customer.SaleId;
                }
            }
            else
            {
                input.UserSaleId = _pbtAppSession.UserId.Value;
            }

            var customerInfo = await _customerService.GetAsync(new EntityDto<long>(_pbtAppSession.CustomerId.Value));
            if (customerInfo == null)
            {
                throw new UserFriendlyException($"Không tìm thấy thông tin khách hàng");
            }

            input.CustomerName = customerInfo.FullName;

            // lấy danh sách package theo input.WaybillNumber
            var packages = await (await _packageRepository.GetAllAsync()).Where(x => x.TrackingNumber == input.WaybillNumber).ToListAsync();

            // lặp packages để lấy thông tin
            foreach (var package in packages)
            {
                ShippingCostResult costPerKg;
                var ShippingCostDto = new ShippingCostDto()
                {
                    CustomerId = customerInfo.Id,
                    CNWarehouseId = 4,//input.CNWarehouseId,
                    VNWarehouseId = 7,//input.VNWarehouseId,
                    ShippingLineId = package.ShippingLineId,
                    ProductGroupTypeId = package.ProductGroupTypeId,
                    Weight = package.Weight,
                    Dimension = package.Length * package.Width * package.Height / 1000000,
                    CostPerKg = package.costPerKg ?? 0
                };
                costPerKg = await _shippingCostService.CalculateShippingCostAsync(ShippingCostDto);
                package.TotalFee = costPerKg.ShippingFee;
                package.TotalPrice += costPerKg.ShippingFee;
                // lấy thông tin từng package để gán vào order
                input.TotalCost += package.TotalPrice ?? 0;
                input.CostPrice += package.TotalPrice ?? 0;
                input.GoodsValue += package.Price ?? 0;
                input.DomesticShipping += package.DomesticShippingFee;
                input.BubbleWrapFee += package.ShockproofFee ?? 0;
                input.Insurance += package.InsuranceFee ?? 0;
                input.WoodenPackagingFee += package.WoodenPackagingFee ?? 0;
                input.AmountDue += package.TotalPrice ?? 0;
                input.ECommerceShipping += costPerKg.ShippingFee;
            }

            if (input.UseInsurance && input.PriceInsurance > 0)
            {
                decimal insurancePercentage = await _customerService.GetInsurancePercentage(input.CustomerId.Value);
                var insurance = (input.PriceInsurance ?? 0) * insurancePercentage / 100;
                input.Insurance = insurance;
                input.TotalCost += insurance;
                input.CostPrice += insurance;
                input.AmountDue += insurance;
            }

            var createdDto = await base.CreateAsync(input);
            // set package order id
            foreach (var package in packages)
            {
                package.OrderId = createdDto.Id;
                package.WarehouseId = createdDto.VNWarehouseId;
                await _packageRepository.UpdateAsync(package);
                // add log
                _entityChangeLoggerAppService.LogChangeAsync<Package>(
                    null
                    , package
                    , "updated"
                    , $"Cập nhật đơn hàng {createdDto.OrderNumber} cho kiện hàng {package.TrackingNumber}"
                    , true
                );
            }

            var orderHistory = new OrderHistoryDto()
            {
                CreationTime = DateTime.Now,
                OrderId = createdDto.Id,
                Status = (int)OrderStatus.Sent
            };
            await _orderHistoryService.CreateAsync(orderHistory);
            var orderLog = new OrderLogDto()
            {
                ActorId = _pbtAppSession.UserId.Value,
                ActorName = _pbtAppSession.UserName,
                CreationTime = DateTime.Now,
                OrderId = createdDto.Id,
                Content = $"Đơn hàng {createdDto.OrderNumber} đã được tạo "
            };

            await _orderLogService.CreateAsync(orderLog);

            //var waybillDto = await _waybillAppService.GetByCode(createdDto.WaybillNumber);

            //if (waybillDto != null)
            //{
            //    waybillDto.OrderId = createdDto.Id;
            //    waybillDto.OrderCode = createdDto.OrderNumber;
            //    waybillDto.Status = (int)WaybillStatus.Sent;
            //    await _waybillAppService.UpdateAsync(waybillDto);
            //}

            return createdDto;

        }

        public async Task<OrderDto> CreateCustomerOrderAsync(CreateUpdateOrderDto input)
        {

            input.OrderType = (int)OrderType.Regular;
            input.OrderStatus = (int)OrderStatus.Sent;
            if (input.AddressId <= 0)
            {
                throw new Exception("Bạn chưa chọn địa chỉ");
            }
            // Nếu user hiện tại là Sale thì gán UserId hiện tại vào saleId, gán customerId bởi input
            // Nếu user hiện tại không phải Sale thì tìm sale tương ứng -> gán vào
            if (_pbtAppSession.CustomerId.HasValue)
            {
                // Đây là trường hợp tài khoản là khách hàng 
                var customer = await _customerService.GetAsync(new EntityDto<long>(_pbtAppSession.CustomerId.Value));
                if (customer != null)
                {
                    input.UserSaleId = customer.SaleId;
                }
            }
            else
            {
                input.UserSaleId = _pbtAppSession.UserId.Value;
            }

            var customerInfo = await _customerService.GetAsync(new EntityDto<long>(input.CustomerId.Value));
            if (customerInfo == null)
            {
                throw new UserFriendlyException($"Không tìm thấy thông tin khách hàng");
            }

            input.CustomerName = customerInfo.FullName;

            // lấy danh sách package theo input.WaybillNumber
            var packages = await (await _packageRepository.GetAllAsync()).Where(x => x.TrackingNumber == input.WaybillNumber).ToListAsync();

            // lặp packages để lấy thông tin
            foreach (var package in packages)
            {
                // lấy thông tin từng package để gán vào order
                input.TotalCost += package.TotalPrice ?? 0;
                input.CostPrice += package.TotalPrice ?? 0;
                input.GoodsValue += package.Price ?? 0;
                input.DomesticShipping += package.DomesticShippingFee;
                input.BubbleWrapFee += package.ShockproofFee ?? 0;
                input.Insurance += package.InsuranceFee ?? 0;
                input.WoodenPackagingFee += package.WoodenPackagingFee ?? 0;
                input.AmountDue += package.TotalPrice ?? 0;
            }
            if (input.UseInsurance && input.PriceInsurance > 0)
            {
                decimal insurancePercentage = await _customerService.GetInsurancePercentage(input.CustomerId.Value);
                var insurance = (input.PriceInsurance ?? 0) * insurancePercentage / 100;
                input.Insurance = insurance;
                input.TotalCost += insurance;
                input.CostPrice += insurance;
                input.AmountDue += insurance;
            }

            if (customerInfo.WarehouseId != null)
            {
                input.VNWarehouseId = (int)customerInfo.WarehouseId;
            }
            var createdDto = await base.CreateAsync(input);

            // set package order id
            foreach (var package in packages)
            {
                package.OrderId = createdDto.Id;
                package.WarehouseId = createdDto.VNWarehouseId;
                await _packageRepository.UpdateAsync(package);
                // add log
                _entityChangeLoggerAppService.LogChangeAsync<Package>(
                    null
                    , package
                    , "updated"
                    , $"Cập nhật đơn hàng {createdDto.OrderNumber} cho kiện hàng {package.TrackingNumber}"
                    , true
                );
            }

            var orderHistory = new OrderHistoryDto()
            {
                CreationTime = DateTime.Now,
                OrderId = createdDto.Id,
                Status = (int)OrderStatus.Sent
            };
            await _orderHistoryService.CreateAsync(orderHistory);
            var orderLog = new OrderLogDto()
            {
                ActorId = _pbtAppSession.UserId.Value,
                ActorName = _pbtAppSession.UserName,
                CreationTime = DateTime.Now,
                OrderId = createdDto.Id,
                Content = $"Đơn hàng {createdDto.OrderNumber} đã được tạo "
            };

            await _orderLogService.CreateAsync(orderLog);

            //var waybillDto = await _waybillAppService.GetByCode(createdDto.WaybillNumber);

            //if (waybillDto != null)
            //{
            //    waybillDto.OrderId = createdDto.Id;
            //    waybillDto.OrderCode = createdDto.OrderNumber;
            //    waybillDto.Status = (int)WaybillStatus.Sent;
            //    await _waybillAppService.UpdateAsync(waybillDto);
            //}
            //var waybill = new WaybillDto()
            //{
            //    OrderId = createdDto.Id,

            //    WaybillCode = input.WaybillNumber,
            //    Status = (int)WaybillStatus.New,
            //  //  CustomerId = createdDto.CustomerId,
            //    CreationTime = DateTime.Now
            //};

            //var waybillCreated = await _waybillAppService.CreateAsync(waybill);
            //await LinkToWaybill(createdDto.Id, waybillCreated.Id, createdDto.WaybillNumber);

            return createdDto;

        }

        public async Task<OrderDto> CreateOrderBySaleAsync(CreateUpdateOrderDto input)
        {

            input.OrderType = (int)OrderType.Regular;
            input.OrderStatus = (int)OrderStatus.Sent;
            if (input.AddressId <= 0)
            {
                throw new Exception("Bạn chưa chọn địa chỉ");
            }

            input.UserSaleId = _pbtAppSession.UserId.Value;
            var customerInfo = await _customerService.GetAsync(new EntityDto<long>(input.CustomerId.Value));
            if (customerInfo == null)
            {
                throw new UserFriendlyException($"Không tìm thấy thông tin khách hàng");
            }

            input.CustomerName = customerInfo.FullName;

            // lấy danh sách package theo input.WaybillNumber
            var packages = await (await _packageRepository.GetAllAsync()).Where(x => x.TrackingNumber == input.WaybillNumber).ToListAsync();

            // lặp packages để lấy thông tin
            foreach (var package in packages)
            {
                // lấy thông tin từng package để gán vào order
                input.TotalCost += package.TotalPrice ?? 0;
                input.CostPrice += package.TotalPrice ?? 0;
                input.GoodsValue += package.Price ?? 0;
                input.DomesticShipping += package.DomesticShippingFee;
                input.BubbleWrapFee += package.ShockproofFee ?? 0;
                input.Insurance += package.InsuranceFee ?? 0;
                input.WoodenPackagingFee += package.WoodenPackagingFee ?? 0;
                input.AmountDue += package.TotalPrice ?? 0;
            }
            if (input.UseInsurance && input.PriceInsurance > 0)
            {
                decimal insurancePercentage = await _customerService.GetInsurancePercentage(input.CustomerId.Value);
                var insurance = (input.PriceInsurance ?? 0) * insurancePercentage / 100;
                input.Insurance = insurance;
                input.TotalCost += insurance;
                input.CostPrice += insurance;
                input.AmountDue += insurance;
            }
            var createdDto = await base.CreateAsync(input);

            // set package order id
            foreach (var package in packages)
            {
                package.OrderId = createdDto.Id;
                package.WarehouseId = createdDto.VNWarehouseId;
                await _packageRepository.UpdateAsync(package);
                // add log
                _entityChangeLoggerAppService.LogChangeAsync<Package>(
                    null
                    , package
                    , "updated"
                    , $"Cập nhật đơn hàng {createdDto.OrderNumber} cho kiện hàng {package.TrackingNumber}"
                    , true
                );
            }

            var orderHistory = new OrderHistoryDto()
            {
                CreationTime = DateTime.Now,
                OrderId = createdDto.Id,
                Status = (int)OrderStatus.Sent
            };
            await _orderHistoryService.CreateAsync(orderHistory);
            var orderLog = new OrderLogDto()
            {
                ActorId = _pbtAppSession.UserId.Value,
                ActorName = _pbtAppSession.UserName,
                CreationTime = DateTime.Now,
                OrderId = createdDto.Id,
                Content = $"Đơn hàng {createdDto.OrderNumber} đã được tạo "
            };

            await _orderLogService.CreateAsync(orderLog);


            return createdDto;

        }

        public async Task<List<OrderDto>> GetFull()
        {
            var data = await Repository.GetAllAsync();
            if (data == null)
            {
                throw new UserFriendlyException($"Bag is empty");
            }
            return ObjectMapper.Map<List<OrderDto>>(data);

        }

        public async Task<List<OrderDto>> GetByStatus(int status)
        {
            if (_pbtAppSession.CustomerId.HasValue)
            {
                var customerId = _pbtAppSession.CustomerId;
                var query = Repository.GetAll();
                query = query.Where(u => u.CustomerId == customerId);
                query = query.Where(u => u.OrderStatus == status);

                var data = query.ToList();
                return ObjectMapper.Map<List<OrderDto>>(data);
            }
            return null;
        }
        /// <summary>
        /// lấy danh sách đơn chưa tạo yêu cầu giao
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<OrderDto>> GetByCustomer(PagedOrderCustomerRequestDto input)
        {
            if (input.CustomerId == null)
            {
                return new PagedResultDto<OrderDto>();
            }

            var query = Repository.GetAll()
                .Where(u => u.CustomerId == input.CustomerId &&
                            u.OrderStatus == (int)OrderStatus.InVietnamWarehouse &&
                            !_deliveryRequestOrderRepository.GetAll().Any(r => r.OrderId == u.Id &&
                                                                               (r.DeliveryRequest.Status !=
                                                                                   (int)DeliveryRequestStatus.Cancel  
                                                                                   )));
            var count = await query.CountAsync();
            var data = await query.ToListAsync();

            var orderDto = ObjectMapper.Map<List<OrderDto>>(data);
            var orderIds = orderDto.Select(x => x.Id).ToList();

            var packages = await _packageRepository.GetAll()
                .Where(x => x.OrderId.HasValue && orderIds.Contains((long)x.OrderId))
                .GroupBy(x => x.OrderId)
                .Select(g => new { OrderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.OrderId, x => x.Count);

            var warehouses = await _warehousesRepository.GetAll()
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var addresses = await _customerAddressRepository.GetAll()
                .ToDictionaryAsync(x => x.Id, x => x.FullAddress);

            foreach (var dto in orderDto)
            {
                dto.PackageCount = packages.GetValueOrDefault(dto.Id);
                dto.VNWarehouseName = warehouses.GetValueOrDefault(dto.VNWarehouseId);
                dto.AddressName = addresses.GetValueOrDefault(dto.AddressId);
            }

            return new PagedResultDto<OrderDto>
            {
                Items = orderDto,
                TotalCount = count
            };
        }

        public async Task<PagedResultDto<OrderDto>> GetPageByCustomer(PagedResultRequestByCustomerDto input)
        {
            var query = Repository.GetAll();
            query = query.Where(u => u.CustomerId == input.CustomerId);
            var count = query.Count();
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            var data = query.ToList();
            return new PagedResultDto<OrderDto>()
            {
                Items = data.MapTo<List<OrderDto>>(),
                TotalCount = count
            };
        }

        public async Task<OrderSummaryDto> GetSummary()
        {
            if (_pbtAppSession.CustomerId.HasValue)
            {
                var customerId = _pbtAppSession.CustomerId.Value;
                var userId = _pbtAppSession.UserId;
                var query = await Repository.GetAllAsync();

                var queryTotal = query.Where(u => u.CreatorUserId == userId || u.CustomerId == customerId);
                var queryMyOrder = 0;// query.Where(u => (u.CreatorUserId == userId || u.CustomerId == customerId) && !u.IsCustomerOrder);
                var queryCustomerOrder = 0;// query.Where(u => (u.CreatorUserId == userId || u.CustomerId == customerId) && u.IsCustomerOrder);
                var queryPending = query.Where(u => u.CreatorUserId == userId || u.CustomerId == customerId)
                    .Where(u => u.OrderStatus == (int)OrderStatus.Sent);


                var queryProcessing = query.Where(u => u.CreatorUserId == userId || u.CustomerId == customerId)
                    .Where(u => u.OrderStatus == (int)OrderStatus.InChinaWarehouse
                     || u.OrderStatus == (int)OrderStatus.InTransit
                     || u.OrderStatus == (int)OrderStatus.InVietnamWarehouse
                     || u.OrderStatus == (int)OrderStatus.OutForDelivery
                     || u.OrderStatus == (int)OrderStatus.Delivered
                    );
                var queryCompleted = query.Where(u => u.CreatorUserId == userId || u.CustomerId == customerId)
               .Where(u => u.OrderStatus == (int)OrderStatus.OrderCompleted);

                var queryCancel = query.Where(u => u.CreatorUserId == userId || u.CustomerId == customerId)
                .Where(u => u.OrderStatus == (int)OrderStatus.Cancelled);

                var dto = new OrderSummaryDto()
                {
                    Total = queryTotal.Count(),
                    CustomerOrderTotal = 0,// queryCustomerOrder.Count(),
                    MyOrderTotal = 0,// queryMyOrder.Count(),
                    TotalCancel = queryCancel.Count(),
                    TotalCompleted = queryCompleted.Count(),
                    TotalPending = queryPending.Count(),
                    TotalProcessing = queryProcessing.Count()

                };
                return dto;
            }
            return new OrderSummaryDto();
        }

        // cancel order
        public async Task<int> CancelOrderAsync(long orderId)
        {
            var order = await Repository.GetAsync(orderId);
            if (order.OrderStatus != (int)OrderStatus.Sent)
            {
                throw new UserFriendlyException("Không thể hủy đơn hàng đã được xử lý");
            }

            order.OrderStatus = (int)OrderStatus.Cancelled;
            await Repository.UpdateAsync(order);

            // Log the cancellation action
            var orderLog = new OrderLogDto()
            {
                ActorId = _pbtAppSession.UserId.Value,
                ActorName = _pbtAppSession.UserName,
                CreationTime = DateTime.Now,
                OrderId = order.Id,
                Content = $"Đơn hàng {order.OrderNumber} đã được hủy "
            };

            await _orderLogService.CreateAsync(orderLog);
            return 1;
        }


        public async Task<int> EditAsync(CreateUpdateOrderDto input)
        {
            var order = await Repository.GetAsync(input.Id);
            if (order.OrderStatus != (int)OrderStatus.Sent)
            {
                throw new UserFriendlyException("Không thể chỉnh sửa đơn hàng đã được xử lý");
            }

            // Validate the input data
            if (input.AddressId <= 0)
            {
                throw new UserFriendlyException("Bạn chưa chọn địa chỉ");
            }

            // Update order properties
            order.ShippingLine = input.ShippingLine;
            order.AddressId = input.AddressId;
            order.CustomerId = input.CustomerId;

            order.UseInsurance = input.UseInsurance;
            order.PriceInsurance = input.PriceInsurance ?? 0;

            if (input.CustomerId != null)
            {
                decimal insurancePercentage = await _customerService.GetInsurancePercentage(input.CustomerId.Value);
                var insurance = (input.PriceInsurance ?? 0) * insurancePercentage / 100;
                order.TotalCost = order.TotalCost - order.Insurance + insurance;
                order.Insurance = insurance;
            }
            order.UseShockproofPackaging = input.UseShockproofPackaging;
            order.UseWoodenPackaging = input.UseWoodenPackaging;
            order.UseDomesticTransportation = input.UseDomesticTransportation;

            order.AddressId = input.AddressId;
            order.CNWarehouseId = input.CNWarehouseId;
            order.VNWarehouseId = input.VNWarehouseId;

            // Save changes to the repository
            await Repository.UpdateAsync(order);

            // Log the update action
            var orderLog = new OrderLogDto()
            {
                ActorId = _pbtAppSession.UserId.Value,
                ActorName = _pbtAppSession.UserName,
                CreationTime = DateTime.Now,
                OrderId = order.Id,
                Content = $"Đơn hàng {order.OrderNumber} đã được cập nhật "
            };

            await _orderLogService.CreateAsync(orderLog);
            return 1;
        }

        [AllowAnonymous]
        [AbpAllowAnonymous]
        public async Task<TrackingDto> LookupAsync(string input, string phone)
        {
            CustomerDto customer = null;
            CustomerAddress customerAddress = null;
            var order = await (await Repository.GetAllAsync()).Where(x => x.WaybillNumber == input.Trim()).FirstOrDefaultAsync();
            if (order == null)
            {
                return null;
            }
            if (order.OrderStatus > 0)
            {
                if (order.CustomerId.HasValue && order.CustomerId > 0)
                {
                    customer = await _customerService.GetCustomerById(order.CustomerId.Value);
                }
                if (order.AddressId.HasValue && order.AddressId > 0)
                {
                    customerAddress = _customerAddressRepository.Get(order.AddressId.Value);
                }
                // Retrieve the packages associated with the order
                var packages = await _packageRepository.GetAll()
                .Where(p => p.OrderId == order.Id)
                .ToListAsync();

                // Map the order and customer information to a TrackingDto
                var trackingDto = new TrackingDto
                {
                    Order = ObjectMapper.Map<OrderDto>(order),

                    TrackingNumber = order.WaybillNumber,
                    Status = order.OrderStatus,
                    CustomerName = customer == null ? string.Empty : customer.FullName,
                    CustomerPhone = customer == null ? string.Empty : customer.PhoneNumber,
                    CustomerAddress = customerAddress?.FullAddress,
                    PackageCount = packages.Count,
                    Packages = ObjectMapper.Map<List<PackageDto>>(packages)
                };
                return trackingDto;
            }
            else
            {
                return new TrackingDto
                {
                    TrackingNumber = order.WaybillNumber,
                    Status = order.OrderStatus,
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        public async Task UpdateDeliveryFee(UpdateDeliveryFeeDto input)
        {
            var package = await _packageRepository.GetAsync(input.Id);
            var oldPackage = package;
            if (package.OrderId == null)
            {
                throw new UserFriendlyException("Kiện chưa có đơn hàng");
            }
            if (package.Weight == null)
            {
                throw new UserFriendlyException("Kiện chưa có trọng lượng");
            }
            var order = await Repository.GetAsync(package.OrderId.Value);

            if (package == null)
            {
                throw new UserFriendlyException("Kiện không tồn tại");
            }

            package.UnitPrice = input.UnitPrice;
            if (input.Weight != package.Weight)
            {
                package.WeightUpdateReason += input.WeightUpdateReason;
            }
            package.Weight = input.Weight;
            package.TotalFee = input.UnitPrice * package.Weight;

            if (package.TotalFee != input.TotalFee)
            {
                package.TotalFee = input.TotalFee;
            }

            order.TotalCost = order.TotalCost - order.ECommerceShipping + (package.TotalFee ?? 0);
            order.AmountDue = order.AmountDue - order.ECommerceShipping + (package.TotalFee ?? 0);
            order.UnitPrice = input.UnitPrice;
            order.ECommerceShipping = package.TotalFee ?? 0;

            await _packageRepository.UpdateAsync(package);
            // add log
            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                oldPackage
                , package
                , "updated"
                , $"Cập nhật phí vận chuyển cho kiện hàng {package.TrackingNumber}"
                , false
            );

            await Repository.UpdateAsync(order);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task UpdateInsurance(UpdateInsuranceDto input)
        {
            var package = await _packageRepository.GetAsync(input.Id);
            var oldPackage = package;
            if (package == null)
            {
                throw new UserFriendlyException("Kiện không tồn tại");
            }

            if (input.InsuranceType == "rmb")
            {
                string key = "ExchangeRateRMB";
                var rsString = await _configurationSettingAppService.GetValueAsync(key);
                long rs = 1;
                if (long.TryParse(rsString, out long result))
                {
                    rs = result;
                    input.InsuranceValue = rs * input.InsuranceValue;
                }
            }
            var order = await Repository.GetAsync(package.OrderId.Value);
            if (order == null)
            {
                throw new UserFriendlyException("Đơn hàng không tồn tại");
            }

            decimal insurancePercentage = await _customerService.GetInsurancePercentage(order.CustomerId.Value);

            package.IsInsured = input.InsuranceValue != 0;
            package.InsuranceValue = input.InsuranceValue;

            var insuranceAmount = input.InsuranceValue * insurancePercentage / 100;

            order.UseInsurance = input.InsuranceValue != 0;
            order.TotalCost = order.TotalCost - order.Insurance + insuranceAmount;
            order.AmountDue = order.AmountDue - order.Insurance + insuranceAmount;
            order.Insurance = insuranceAmount;
            await _packageRepository.UpdateAsync(package);
            // add log
            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                oldPackage
                , package
                , "updated"
                , $"Cập nhật bảo hiểm cho kiện hàng {package.TrackingNumber}"
                , false
            );
            await Repository.UpdateAsync(order);
        }

        public async Task UpdateWoodenPackagingFee(UpdateWoodenPackagingFeeDto input)
        {
            var package = await _packageRepository.GetAsync(input.Id);
            var oldPackage = package;
            if (package == null)
            {
                throw new UserFriendlyException("Kiện không tồn tại");
            }
            var order = await Repository.GetAsync(package.OrderId.Value);
            if (order == null)
            {
                throw new UserFriendlyException("Đơn hàng không tồn tại");
            }

            package.IsInsured = true;
            package.WoodenPackagingFee = input.WoodenPackagingFee;

            //order.TotalCost = order.TotalCost - order.WoodenPackagingFee + input.WoodenPackagingFee;
            //order.AmountDue = order.AmountDue - order.WoodenPackagingFee + input.WoodenPackagingFee;
            order.WoodenPackagingFee = input.WoodenPackagingFee;

            await _packageRepository.UpdateAsync(package);
            // add log
            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                oldPackage
                , package
                , "updated"
                , $"Cập nhật phí đóng gỗ cho kiện hàng {package.TrackingNumber}"
                , false
            );
            await Repository.UpdateAsync(order);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task UpdateGoodsValue(UpdateGoodsValueDto input)
        {
            var package = await _packageRepository.GetAsync(input.Id);
            var oldPackage = package;
            if (package.OrderId != null)
            {
                var order = await Repository.GetAsync(package.OrderId.Value);
                var oldOrder = ObjectMapper.Map<Order>(ObjectMapper.Map<OrderDto>(order));
                if (order == null)
                {
                    throw new UserFriendlyException("Order not found.");
                }
                order.GoodsValue = input.GoodsValue;

                package.Price = input.GoodsValue;
                package.IsPriceUpdate = true;

                await Repository.UpdateAsync(order);
                // add log
                _entityChangeLoggerAppService.LogChangeAsync<Order>(
                    oldOrder
                    , order
                    , "updated"
                    , $"Sửa đơn hàng"
                    , false
                );
                await _packageRepository.UpdateAsync(package);
                // add log
                _entityChangeLoggerAppService.LogChangeAsync<Package>(
                    oldPackage
                    , package
                    , "updated"
                    , $"Cập nhật giá trị hàng hóa cho kiện hàng {package.TrackingNumber}"
                    , false
                );
            }
        }

        public async Task UpdateFee(UpdateFeeDto input)
        {
            var package = await _packageRepository.GetAsync(input.Id);
            var oldPackage = package;
            var order = await Repository.GetAsync(package.OrderId.Value);
            if (package == null)
            {
                throw new UserFriendlyException("Kiện không tồn tại");
            }
            if (order == null)
            {
                throw new UserFriendlyException("Đơn hàng không tồn tại");
            }

            if (input.BubbleWrapFee.HasValue)
            {
                package.ShockproofFee = input.BubbleWrapFee.Value;
                package.IsShockproof = input.BubbleWrapFee != 0;
                order.TotalCost = (decimal)(order.TotalCost - order.BubbleWrapFee + input.BubbleWrapFee);
                order.AmountDue = (decimal)(order.AmountDue - order.BubbleWrapFee + input.BubbleWrapFee);
                order.BubbleWrapFee = input.BubbleWrapFee.Value;
            }

            if (input.DomesticShipping.HasValue)
            {

                package.DomesticShippingFee = input.DomesticShipping.Value;
                package.IsDomesticShipping = true;
                //order.TotalCost = (decimal)(order.TotalCost - order.DomesticShipping + input.DomesticShipping);
                //order.AmountDue = (decimal)(order.AmountDue - order.DomesticShipping + input.DomesticShipping);
                //order.DomesticShipping = input.DomesticShipping.Value;
            }
            await _packageRepository.UpdateAsync(package);
            // add log
            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                oldPackage
                , package
                , "updated"
                , $"Cập nhật phí vận chuyển cho kiện hàng {package.TrackingNumber}"
                , false
            );
            await Repository.UpdateAsync(order);
        }


        public async Task<int> LinkToWaybill(long orderId, long waybillId, string waybillNumber)
        {
            try
            {
                var result = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                   "EXEC SP_OrderWaybill_Link  @WaybillId, @WaybillNumber, @OrderId",
                   new SqlParameter("@WaybillId", waybillId),
                   new SqlParameter("@WaybillNumber", waybillNumber),
                    new SqlParameter("@OrderId", orderId)
                );
                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<OrderDto> GetByWaybillAsync(string waybilNumber)
        {
            if (string.IsNullOrEmpty(waybilNumber))
            {
                throw new UserFriendlyException("Mã vận đơn không được để trống");
            }
            var order = await Repository.GetAll()
                .Where(x => x.WaybillNumber == waybilNumber)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                throw new UserFriendlyException("Không tìm thấy đơn hàng với mã vận đơn này");
            }
            return ObjectMapper.Map<OrderDto>(order);
        }


        public async Task<PagedResultDto<OrderWaybillDto>> GetOrderWaybillAsync(PagedAndSortedOrderResultRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            var currentUserWarehouseId = _pbtAppSession.WarehouseId;
            var permissionCheckResult = GetPermissionCheckerWithCustomerIds();

            if (permissionCheckResult.permissionCase == -1)
            {
                return new PagedSaleViewResultDto<OrderWaybillDto>
                {
                    Items = new List<OrderWaybillDto>(),
                    TotalCount = 0,
                    TotalWeight = 0
                };
            }
            var permissionCase = permissionCheckResult.permissionCase;
            var customerIds = permissionCheckResult.CustomerIds;
            var query = base.CreateFilteredQuery(input);
            // Tạo query để lấy danh sách kiện hàng
            query = query
                 .Where(x =>

                   (permissionCase == 1) || // admin  nhìn thấy tất cả
                   ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4 || permissionCase == 7) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value)) ||
                   permissionCase == 5 ||
                   permissionCase == 6
                // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                );

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                x.OrderNumber.ToUpper().Contains(input.Keyword.ToUpper())
                || x.WaybillNumber.ToUpper().Contains(input.Keyword.ToUpper()));
            }
            if (input.ShippingLine.HasValue && input.ShippingLine > 0)
            {
                query = query.Where(x => x.ShippingLine == input.ShippingLine);
            }

            if (input.CustomerId.HasValue && input.CustomerId > 0)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId);
            }
            if (input.OrderType > 0)
            {
                //   query = query.Where(x => (input.OrderType == 2 && (x.IsCustomerOrder.HasValue && x.IsCustomerOrder.Value)) || (input.OrderType == 2 && !x.IsCustomerOrder));
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime.Date >= input.StartDate.Value.Date);
            }
            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime < input.EndDate.Value.Date.AddDays(1));
            }
            if (input.Status > 0)
            {
                query = query.Where(x => x.OrderStatus == input.Status);
            }

            var count = query.Count();
            query = query.OrderByDescending(x => x.CreationTime);
            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var data = ObjectMapper.Map<List<OrderWaybillDto>>(query.ToList());

            return new PagedResultDto<OrderWaybillDto>()
            {
                Items = data,
                TotalCount = count
            };
        }

        public async Task<JsonResult> CreateListAsync(CreateOrderListDto input)
        {

            if (!input.CustomerId.HasValue || input.CustomerId <= 0)
            {
                return new JsonResult(new
                {
                    StatusCode = -1,
                    Message = "Chưa chọn khách hàng",
                    SuccessCount = 0,
                    ErrorCount = 0,
                    CustomerName = ""
                });
            }
            if (string.IsNullOrEmpty(input.WaybillCodes))
            {
                return new JsonResult(new
                {
                    StatusCode = -1,
                    Message = "Chưa nhập mã vận đơn",
                    SuccessCount = 0,
                    ErrorCount = 0,
                    CustomerName = ""
                });
            }

            Logger.Info($"Start CreateListAsync by waybill. Input: {JsonConvert.SerializeObject(input)}");

            var successCount = new SqlParameter
            {
                ParameterName = "@SuccessCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var errorCount = new SqlParameter
            {
                ParameterName = "@ErrorCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var statusCode = new SqlParameter
            {
                ParameterName = "@StatusCode",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var message = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = SqlDbType.NVarChar,
                Size = 500,
                Direction = ParameterDirection.Output
            };
            var customerName = new SqlParameter
            {
                ParameterName = "@customerName",
                SqlDbType = SqlDbType.NVarChar,
                Size = 256,
                Direction = ParameterDirection.Output
            };

            var exeResult = await ConnectDb.ExecuteNonQueryAsync("SP_Orders_CreateListByWaybill", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@WaybillCodes", input.WaybillCodes),
                    new SqlParameter("@Note", input.Note ?? (object)DBNull.Value),
                    new SqlParameter("@CustomerId", input.CustomerId ?? (object)DBNull.Value),
                    new SqlParameter("@CreatorUserName", _pbtAppSession.UserName ?? (object)DBNull.Value),
                    successCount,
                    errorCount,
                    statusCode,
                    message,
                    customerName
                }
            );

            Logger.Info($"End CreateListAsync by waybill. StatusCode: {statusCode.Value}, CustomerName: {customerName.Value}, Message: {message.Value}, SuccessCount: {successCount.Value}, ErrorCount: {errorCount.Value}");

            return new JsonResult(new
            {
                StatusCode = (int)statusCode.Value,
                Message = message.Value.ToString(),
                SuccessCount = (int)successCount.Value,
                ErrorCount = (int)errorCount.Value,
                CustomerName = customerName.Value.ToString()
            });
        }

        /// <summary>
        /// Lấy danh sách đơn hàng con của một đơn hàng cha
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<List<OrderDto>> GetChildOrders(long orderId)
        {
            var childOrders = await Repository.GetAllAsync();
            var filteredOrders = childOrders.Where(x => x.ParentId == orderId).ToList();
            return ObjectMapper.Map<List<OrderDto>>(filteredOrders);

        }


        [HttpPost]
        public async Task<ImportOrderResultDto> Import([FromForm] ImportOrderDto input)
        {
            var result = new ImportOrderResultDto()
            {
                Total = 0,
                SuccessCount = 0,
                FailedCount = 0,
                Messages = new List<string>()
            };
            if (input == null || input.Attachments == null || input.Attachments.Count == 0)
            {
                result.Messages.Add("<strong><i class='fa fa-times-circle text-danger'></i> File không hợp lệ.</strong>");
                return result;
            }

            var file = input.Attachments[0];

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                IWorkbook workbook;
                if (Path.GetExtension(file.FileName).Equals(".xls"))
                {
                    workbook = new HSSFWorkbook(stream); // Định dạng .xls
                }
                else if (Path.GetExtension(file.FileName).Equals(".xlsx"))
                {
                    workbook = new XSSFWorkbook(stream); // Định dạng .xlsx
                }
                else
                {
                    result.Messages.Add("<strong><i class='fa fa-times-circle text-danger'></i> File không hợp lệ.</strong>");
                    return result;
                }

                var sheet = workbook.GetSheetAt(0); // Lấy sheet đầu tiên
                for (int i = 1; i <= sheet.LastRowNum; i++) // Bỏ qua hàng tiêu đề
                {
                    result.Total++;
                    var row = sheet.GetRow(i);
                    if (row == null)
                    {
                        result.FailedCount++;
                        result.Messages.Add($"<i class='fa fa-times-circle text-danger'></i> Hàng <strong>{i + 1}</strong> không hợp lệ.");
                        continue;
                    }

                    var waybillNumber = row.GetCell(1)?.ToString();
                    if (string.IsNullOrEmpty(waybillNumber))
                    {
                        result.FailedCount++;
                        result.Messages.Add($"<i class='fa fa-times-circle text-danger'></i> Hàng <strong>{i + 1}</strong> không có mã vận đơn.");
                        continue;
                    }

                    var shippingLine = row.GetCell(3)?.ToString();
                    if (shippingLine != "TMDT" && shippingLine != "LO")
                    {
                        result.FailedCount++;
                        result.Messages.Add($"<i class='fa fa-times-circle text-danger'></i> Hàng <strong>{i + 1}</strong> không đúng line vận chuyển.");
                        continue;
                    }

                    var customerName = row.GetCell(2)?.ToString();
                    var customer = await _customerService.GetCustomerByUserNameWithCacheAsync(customerName);
                    if (customer == null)
                    {
                        result.FailedCount++;
                        result.Messages.Add($"<i class='fa fa-times-circle text-danger'></i> Không tìm thấy khách hàng với tên: <strong>{customerName}</strong>.");
                        continue;
                    }


                    var orderExisted = await Repository.FirstOrDefaultAsync(x => x.WaybillNumber == waybillNumber);
                    if (orderExisted != null)
                    {
                        result.FailedCount++;
                        result.Messages.Add($"<i class='fa fa-times-circle text-danger'></i> Đơn hàng với mã vận đơn <strong>{waybillNumber}</strong> đã tồn tại.");
                        continue;
                    }

                    var insurance = string.IsNullOrEmpty(row.GetCell(4)?.ToString()) ? 0 : Convert.ToDecimal(row.GetCell(4)?.ToString());
                    var useInsurance = insurance > 0;

                    var UseWoodenPackaging = row.GetCell(5)?.ToString() == "Có";
                    var UseShockproofPackaging = row.GetCell(6)?.ToString() == "Có";

                    var orderDto = new CreateUpdateOrderDto
                    {
                        WaybillNumber = waybillNumber,
                        ShippingLine = shippingLine == "LO" ? 1 : 2,
                        CustomerId = customer.Id,
                        CustomerName = customer.FullName,
                        OrderStatus = (int)OrderStatus.New, // Mặc định là New
                        UseInsurance = useInsurance,
                        Insurance = insurance,
                        UseWoodenPackaging = UseWoodenPackaging,
                        UseShockproofPackaging = UseShockproofPackaging

                    };

                    try
                    {
                        await base.CreateAsync(orderDto); // Chờ cho đơn hàng được tạo
                        result.SuccessCount++;
                        result.Messages.Add($"<i class='fa fa-check-circle text-success'></i> Đơn hàng với mã vận đơn <strong>{waybillNumber}</strong> đã được tạo thành công.");
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Messages.Add($"<i class='fa fa-times-circle text-danger'></i> Lỗi khi tạo đơn hàng với mã vận đơn <strong>{waybillNumber}</strong>: {ex.Message}.");
                    }
                }
            }


            return result;
        }


        public async Task MarkAsDeliveredAsync(long orderId)
        {
            //// Lấy thông tin đơn hàng
            //var order = await Repository.GetAllIncluding(o => o.Packages)
            //    .FirstOrDefaultAsync(o => o.Id == orderId);

            //if (order == null)
            //{
            //    throw new UserFriendlyException("Không tìm thấy đơn hàng.");
            //}

            //// Kiểm tra trạng thái đơn hàng
            //if (order.OrderStatus != (int)OrderStatus.OutForDelivery)
            //{
            //    throw new UserFriendlyException("Chỉ có thể đánh dấu đơn hàng đang giao là đã giao.");
            //}

            //// Cập nhật trạng thái đơn hàng
            //order.OrderStatus = (int)OrderStatus.Delivered;
            //order.DeliveredTime = DateTime.Now;

            //// Cập nhật trạng thái kiện hàng
            //foreach (var package in order.Packages)
            //{
            //    package.ShippingStatus = (int)PackageDeliveryStatusEnum.Delivered;
            //    package.ReceivedTime = DateTime.Now;
            //}

            //// Lưu thay đổi
            //await CurrentUnitOfWork.SaveChangesAsync();
        }


        public async Task MarkAsCompletedAsync(long orderId)
        {
            //// Lấy thông tin đơn hàng
            //var order = await Repository.GetAllIncluding(o => o.Packages)
            //    .FirstOrDefaultAsync(o => o.Id == orderId);

            //if (order == null)
            //{
            //    throw new UserFriendlyException("Không tìm thấy đơn hàng.");
            //}

            //// Kiểm tra trạng thái đơn hàng
            //if (order.OrderStatus != (int)OrderStatus.Delivered)
            //{
            //    throw new UserFriendlyException("Chỉ có thể đánh dấu đơn hàng đang giao là đã giao.");
            //}

            //// Cập nhật trạng thái đơn hàng
            //order.OrderStatus = (int)OrderStatus.OrderCompleted;
            //order.ComplaintTime = DateTime.Now;

            //// Cập nhật trạng thái kiện hàng
            //foreach (var package in order.Packages)
            //{
            //    package.ShippingStatus = (int)PackageDeliveryStatusEnum.Completed;
            //    package.CompletedTime = DateTime.Now;
            //}

            //// Lưu thay đổi
            //await CurrentUnitOfWork.SaveChangesAsync();
        }


        public async Task<JsonResult> UpdateNewCustomerForOrder(long orderId, long newCustomerId)
        {

            var execResult = await ConnectDb.ExecuteNonQueryAsync(
                "SP_Orders_UpdateCustomerAndPackages",
                CommandType.StoredProcedure,
                new[]
                {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@NewCustomerId", newCustomerId)
                }
            );
            if (execResult > 0)
            {
                Logger.Info($"Cập nhật khách hàng mới cho đơn hàng thành công. OrderId: {orderId}, NewCustomerId: {newCustomerId}");
                return new JsonResult(new { success = true, message = "Cập nhật khách hàng mới cho đơn hàng thành công." });
            }
            else
            {
                Logger.Warn($"Cập nhật khách hàng mới cho đơn hàng thất bại hoặc không có thay đổi. OrderId: {orderId}, NewCustomerId: {newCustomerId}");
                return new JsonResult(new { success = false, message = "Cập nhật khách hàng mới cho đơn hàng thất bại hoặc không có thay đổi." });
            }

        }

        public async Task<List<WaybillForRematchDto>> GetWaybillByIds(string orderIds)
        {
            if (string.IsNullOrEmpty(orderIds))
                return new List<WaybillForRematchDto>();
            var result = await ConnectDb.GetListAsync<WaybillForRematchDto>("SP_Orders_GetWaybillByOrderIds", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("OrderIds", orderIds)
                });

            return result;
        }


        [AbpAuthorize(PermissionNames.Function_Order_Rematch)]
        public async Task<JsonResult> Rematch(string WaybillNumbers, long customerId)
        {
            try
            {
                var statusCodePr = new SqlParameter
                {
                    ParameterName = "@StatusCode",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                var messageCodePr = new SqlParameter
                {
                    ParameterName = "@Message",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1, // NVARCHAR(MAX)
                    Direction = ParameterDirection.Output
                };

                 await ConnectDb.ExecuteNonQueryAsync(
                              "SP_Orders_RebindCustomer_ByWaybillNumbers",
                              CommandType.StoredProcedure,
                              new[]
                              {
                                new SqlParameter("@WaybillNumbers", WaybillNumbers),
                                new SqlParameter("@NewCustomerId", customerId),
                                statusCodePr,
                                messageCodePr
                              }
                          );
                var statusCode = (int)statusCodePr.Value;
                var message = messageCodePr.Value.ToString();

                return new JsonResult(new { success = true, message = message });

            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi rematch khách hàng mới cho đơn hàng. WaybillNumbers: {WaybillNumbers}, NewCustomerId: {customerId}", ex);
                return new JsonResult(new { success = false, message = "Lỗi khi rematch khách hàng mới cho đơn hàng." });
            }
          

        }
        /// <summary>
        /// 1. Admin
        /// 2. Sale Admin
        /// 3. Sale
        /// 4. Customer
        /// 5. WarehouseCN
        /// 6. WarehouseVN
        /// 7. SaleCustom
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
                    || PermissionChecker.IsGranted(PermissionNames.Function_ViewAllOrder)
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
    }
}
