using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.CustomerAddresss;
using pbt.Orders;
using pbt.Orders.Dto;
using pbt.Warehouses;
using pbt.Web.Models.Orders;
using System.Threading.Tasks;
using pbt.Customers;
using Abp.Application.Services.Dto;
using pbt.Packages;
using pbt.Complaints;
using pbt.OrderHistories;
using pbt.OrderLogs;
using Abp.AspNetCore.Mvc.Authorization;
using pbt.ApplicationUtils;
using pbt.Web.Models.SaleAdmin;
using pbt.CustomerAddresss.Dto;
using pbt.OrderNumbers;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Customers)]
    [AbpMvcAuthorize]
    public class SaleController : pbtControllerBase
    {
        private readonly IOrderAppService _orderService;
        private readonly IWarehouseAppService _warehouseService;
        private readonly ICustomerAddressAppService _customerAddressService;
        private readonly IPackageAppService _packageAppService;
        private readonly IComplaintAppService _complaintAppService;
        private readonly IOrderHistoryAppService _orderHistoryAppService;
        private readonly IOrderLogAppService _orderLogAppService;
        private readonly ICustomerAppService _customerService;
        private readonly pbtAppSession _pbtAppSession;
        private readonly IIdentityCodeAppService _identityCodeAppService;

        public SaleController(IOrderAppService orderService,
                IWarehouseAppService warehouseService,
                ICustomerAddressAppService customerAddressService,
                ICustomerAppService customerService,
                IPackageAppService packageAppService,
                IComplaintAppService complaintAppService,

                IOrderHistoryAppService orderHistoryAppService,
                IOrderLogAppService orderLogAppService,
                IIdentityCodeAppService identityCodeAppService,
             pbtAppSession pbtAppSession
            )
        {
            _orderService = orderService;
            _warehouseService = warehouseService;
            _customerAddressService = customerAddressService;
            _pbtAppSession = pbtAppSession;
            _customerService = customerService;
            _packageAppService = packageAppService;
            _complaintAppService = complaintAppService;
            _orderHistoryAppService = orderHistoryAppService;
            _orderLogAppService = orderLogAppService;
            _identityCodeAppService = identityCodeAppService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SaleAdminIndexModel() { };
            return View(model);
        }


        public async Task<IActionResult> MyOrder()
        {
            var model = new SaleAdminOrderModel() { };
            return View(model);
        }

        public async Task<IActionResult> CustomerDetail(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            CustomerAddressDto address = null;
            if (customer != null && customer.AddressId.HasValue)
            {
                address = await _customerAddressService.GetAsync(new EntityDto<long>(customer.AddressId.Value));
            }
            var model = new CustomerDetailModel()
            {
                Id = id,
                Customer = customer,
                Address = address
            };
            return View(model);
        }


        public async Task<IActionResult> CreateMyOrder()
        {
            var customerId = _pbtAppSession.CustomerId;
            var model = new CreateMyOrderModel()
            {
                Dto = new CreateMyOrderDto()
                {
                    CustomerId = customerId.Value,
                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
            };
            return View(model);
        }


        public async Task<IActionResult> CreateCustomerOrder()
        {
            var customerId = _pbtAppSession.CustomerId;
            var model = new CreateCustomerOrderModel()
            {
                Dto = new CreateUpdateOrderDto()
                {

                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                //   Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
                Customers = !customerId.HasValue ? null : await _customerService.GetChildren(customerId.Value)

            };
            return View(model);
        }

        public async Task<IActionResult> CreateOrderBySale()
        {
            var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("DH");
            var model = new CreateCustomerOrderModel()
            {
                Dto = new CreateUpdateOrderDto()
                {
                    OrderNumber = identityCode.Code,
                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                //  Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
                Customers = await _customerService.GetBySale(AbpSession.UserId.Value) //!customerId.HasValue ? null : await _customerService.GetChildren(customerId.Value)

            };
            return View(model);
        }


        public async Task<IActionResult> GetAddressByCustomerId(long customerId)
        {
            var data = await _customerAddressService.GetByCustomerId(customerId);
            return PartialView("_AddressByCustomerId", data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _orderService.GetAsync(new EntityDto<long>(id));
            var customerAddress = await _customerAddressService.GetAsync(new EntityDto<long>(dto.AddressId));
            var packages = await _packageAppService.GetByOrder(id);
            var complaints = await _complaintAppService.GetByOrderId(id);
            var histories = await _orderHistoryAppService.GetByOrderId(id);
            var logs = await _orderLogAppService.GetByOrderId(id);
            var model = new OrderDetailModel()
            {
                Dto = dto,
                CustomerAddress = customerAddress,
                Packages = packages,
                Complaints = complaints,
                Histories = histories,
                Logs = logs
            };
            return View(model);
        }
    }
}
