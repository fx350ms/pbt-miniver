using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.Record.Chart;
using pbt.Authorization;
using pbt.Controllers;
using pbt.ShippingPartners;
using pbt.ShippingRates;
using pbt.Warehouses;
using pbt.Web.Models.ShippingCosts;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbt.ShippingCosts;
using pbt.ShippingRates.Dto;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    //  [AbpMvcAuthorize]
    public class ShippingCostsController : pbtControllerBase
    {
        private readonly IShippingPartnerAppService _shippingPartnerAppService;
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly IProductGroupTypeAppService _productGroupTypeAppService;
        private readonly IShippingCostBaseAppService _shippingCostBaseAppService;
        private readonly IShippingCostGroupAppService _shippingCostGroupAppService;
        public ShippingCostsController(IShippingPartnerAppService shippingPartnerAppService,
            IWarehouseAppService warehouseAppService,
            IProductGroupTypeAppService productGroupTypeAppService,
            IShippingCostBaseAppService shippingCostBaseAppService,
            IShippingCostGroupAppService shippingCostGroupAppService)
        {
            _shippingPartnerAppService = shippingPartnerAppService;
            _warehouseAppService = warehouseAppService;
            _productGroupTypeAppService = productGroupTypeAppService;
            _shippingCostBaseAppService = shippingCostBaseAppService;
            _shippingCostGroupAppService = shippingCostGroupAppService;
        }

        public async Task<IActionResult> Index()
        {

            var shippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(-1);
            var warehouseCNs = await _warehouseAppService.GetByCountry(2);
            var warehouseVNs = await _warehouseAppService.GetByCountry(1);
            var model = new ShippingCostIndexViewModel
            {
                ShippingPartners = shippingPartners,
                WarehousesVN = warehouseVNs,
                WarehousesCN = warehouseCNs
            };
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> RateTierContentItem(int shippingType, string lineName, int fromWarehouse, string fromName, int toWarehouse, string toName)
        {
            // Lấy dữ liệu cần thiết để hiển thị nhóm giá
            //var productGroupTypes = await _productGroupTypeAppService.GetAllListAsync();
            var productGroupTypes = new List<ProductGroupTypeDto>();
            productGroupTypes.Add(new ProductGroupTypeDto()
            {
                Id = 1,
                IsDefault = true,
                Name = "Hàng hóa chung",
                Note = ""
            });
            var model = new RateTierContentItemViewModel
            {
                ShippingTypeId = shippingType,
                FromWarehouseId = fromWarehouse,
                ToWarehouseId = toWarehouse,
                ProductGroupTypes = productGroupTypes,
                LineName = lineName,
                FromName = fromName,
                ToName = toName
            };

            // Trả về PartialView
            return PartialView("_RateTierContentItem", model);
        }

        public async Task<IActionResult> ConfigCost(long id)
        {
            var warehouseCNs = await _warehouseAppService.GetByCountry(1);
            var warehouseVNs = await _warehouseAppService.GetByCountry(2);
            var dto = await _shippingCostBaseAppService.GetByGroupIdAsync(id);

            var model = new ConfigureShippingCostModel()
            {
                GroupId = id,
                WarehousesCN = warehouseCNs,
                WarehousesVN = warehouseVNs,
                ShippingCosts = dto
            };
            return View(model);
        }


        public async Task<ActionResult> EditModal(long id)
        {
            var shippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(-1);
            var warehouseCNs = await _warehouseAppService.GetByCountry(2);
            var warehouseVNs = await _warehouseAppService.GetByCountry(1);
            var shippingCostGroup = await _shippingCostGroupAppService.GetAsync(new EntityDto<long>(id));
            var model = new ShippingCostIndexViewModel
            {
                ShippingPartners = shippingPartners,
                WarehousesVN = warehouseVNs,
                WarehousesCN = warehouseCNs,
                ShippingCostGroup = shippingCostGroup
            };
            
            return PartialView("_EditModal", model);
        }


    }
}
