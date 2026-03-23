using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.CustomerAddresss;
using pbt.Customers;
using pbt.DeliveryRequests;
using pbt.Orders;
using pbt.Packages;
using pbt.Warehouses;
using pbt.Web.Models.DeliveryRequests;
using pbt.Web.Mvc.Models.DeliveryRequests;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_DeliveryRequest)]
    [AbpMvcAuthorize]
    public class DeliveryRequestController : pbtControllerBase
    {
        private readonly pbtAppSession _pbtAppSession;
        private readonly IDeliveryRequestAppService _deliveryRequestAppService;
        private readonly ICustomerAppService _customerAppService;
        private readonly ICustomerAddressAppService _customerAddressAppService;
        private readonly IOrderAppService _orderAppService;
        private readonly IPackageAppService _packageAppService;
        private readonly IWarehouseAppService _warehouseAppService;

        public DeliveryRequestController(
            pbtAppSession pbtAppSession,
            ICustomerAddressAppService customerAddressService,
            IDeliveryRequestAppService deliveryRequestAppService,
            ICustomerAppService customerAppService,
            ICustomerAddressAppService customerAddressAppService,
            IOrderAppService orderAppService,
            IPackageAppService packageAppService,
            IWarehouseAppService warehouseAppService
            )
        {
            _pbtAppSession = pbtAppSession;
            _deliveryRequestAppService = deliveryRequestAppService;
            _customerAppService = customerAppService;
            _customerAddressAppService = customerAddressAppService;
            _orderAppService = orderAppService;
            _packageAppService = packageAppService;
            _warehouseAppService = warehouseAppService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {

            var warehouses = await _warehouseAppService.GetByCountry(2);
            // lấy customer hiện tại
            var model = new CreateUpdateDeliveryRequest()
            {
                Warehouses = warehouses,
                Customers = await _customerAppService.GetCustomersByCurrentUserAsync(),
            };
          
            return View(model);
        }

        public async Task<IActionResult> Payment(int id)
        {
            var dto = await _deliveryRequestAppService.GetAsync(new EntityDto<int>(id));
         
            var model = new DeliveryRequestPaymentModel()
            {
                Dto = dto,
                Packages = await _packageAppService.GetAllByDeliveryRequestAsync(dto.Id)
            };
            return View("Payment", model);
        }

        public async Task<IActionResult> Pending()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPackagesByBagId(int bagId)
        {
            var model = new DeliveryRequestPackagesByBagIdModel()
            {
                BagId = bagId,
                Packages = await _packageAppService.GetAllPackagesListByBagIdAsync(bagId)
            };
            return PartialView("_PackagesByBagId", model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _deliveryRequestAppService.GetAsync(new EntityDto<int>(id));
            
            dto.Weight = _deliveryRequestAppService.GetTotalWeightDeliveryRequest(dto.Id);

            var model = new DeliveryRequestDetailModel()
            {
                Dto = dto,
                Packages = await _packageAppService.GetAllByDeliveryRequestAsync(dto.Id)
            };
            return View("Detail", model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetNewDeliveryRequestCount()
        {
            var count = 0;
            if (await PermissionChecker.IsGrantedAsync(PermissionNames.Pages_DeliveryRequest))
            {
                count  = await _deliveryRequestAppService.GetNewDeliveryRequest();
            }
            return Json(new { count });
        }

      


    }
}
