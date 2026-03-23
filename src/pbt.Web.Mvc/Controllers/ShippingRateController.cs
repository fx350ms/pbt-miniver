using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.ShippingRates.Dto;
using pbt.ShippingRates;
using System.Threading.Tasks;
using pbt.Warehouses;
using pbt.Web.Models.ShippingRate;
using pbt.ApplicationUtils;
using pbt.Customers;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ShippingRateController : pbtControllerBase
    {
        private readonly IShippingRateAppService _shippingRateAppService;
        private readonly IShippingRateCustomerAppService _shippingRateCustomerAppService;
        private readonly IShippingRateTierAppService _shippingRateTierAppService;
        private readonly IProductGroupTypeAppService _productGroupTypeAppService;
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly ICustomerAppService _customerAppService;

        public ShippingRateController(
            IShippingRateAppService shippingRateAppService,
            IShippingRateCustomerAppService shippingRateCustomerAppService,
            IShippingRateTierAppService shippingRateTierAppService,
            IProductGroupTypeAppService productGroupTypeAppService,
            IWarehouseAppService warehouseAppService,
            ICustomerAppService customerAppService
            )
        {
            _shippingRateAppService = shippingRateAppService;
            _shippingRateCustomerAppService = shippingRateCustomerAppService;
            _shippingRateTierAppService = shippingRateTierAppService;
            _productGroupTypeAppService = productGroupTypeAppService;
            _warehouseAppService = warehouseAppService;
            _customerAppService = customerAppService;
        }

        public async Task<IActionResult> Index()
        {
            //var productGroups = await _productGroupTypeAppService.GetAllAsync(new PagedResultRequestDto());
            //return View(productGroups.Items);
            var customers = await _customerAppService.GetAllForSelectAsync();
            ViewBag.Customers = customers;

            return View();
        }

        public async Task<IActionResult> CreateOrEditModal(long? id)
        {
            var shippingRate = id.HasValue
                ? await _shippingRateAppService.GetAsync(new EntityDto<long>(id.Value))
                : new ShippingRateDto();

            return PartialView("_CreateOrEditModal", shippingRate);
        }

        //public async Task<IActionResult> AssignCustomersModal(long shippingRateId)
        //{
        //    var customers = await _shippingRateCustomerAppService.GetCustomersForShippingRate(shippingRateId);
        //    return PartialView("_AssignCustomersModal", customers.Items);
        //}

        public async Task<IActionResult> ConfigureTiersModal(long id)
        {
            var productTypeList = await _productGroupTypeAppService.GetAllListAsync();
            var warehouseCN = await _warehouseAppService.GetByCountry((int)WarehouseType.Cn);
            var warehouseVN = await _warehouseAppService.GetByCountry((int)WarehouseType.Vn);
            var dto = await _shippingRateAppService.GetByGroupIdAsync(id);
            var model = new ConfigureTiersModel()
            {
                GroupId = id,
                ProductGroupTypes = productTypeList,
                WarehousesCN = warehouseCN,
                WarehousesVN = warehouseVN,
                ShippingRates = dto
            };
            return PartialView("_ConfigureTiersModal", model);
        }


        public async Task<IActionResult> RateTierContentItem(int fromId, string fromName, int toId, string toName, int line, string lineName)
        {
            // Tạo model cho view
            var model = new RateTierContentItemModel
            {
                FromWarehouseId = fromId,
                FromWarehouseName = fromName,
                ToWarehouseId = toId,
                ToWarehouseName = toName,
                LineNumber = line,
                LineName = lineName,
                //   ProductGroupTypes = productTypeList
            };


            if (line == (int)pbt.ApplicationUtils.CustomerLine.Line2)
            {

                // Trả về partial view
                return PartialView("_RateTierContentItem", model);
            }
            else
            {
                var productTypeList = await _productGroupTypeAppService.GetAllListAsync();
                // Tạo model cho view
                model.ProductGroupTypes = productTypeList;

                // Trả về partial view
                return PartialView("_RateTierContentItem", model);

            }
            // Lấy danh sách nhóm sản phẩm từ DB

        }


        public async Task<IActionResult> GetCustomerForAssign(long id)
        {
            var customers = await _shippingRateCustomerAppService.GetCustomersForShippingRate(id);
            return PartialView("_GetCustomerForAssign", new AssignToCustomersModel() { GroupId = id, Customers = customers });
        }
    }
}

