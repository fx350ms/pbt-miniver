using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.Warehouses;
using pbt.Web.Models.Warehouses;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    //  [AbpMvcAuthorize]
    public class WarehousesController : pbtControllerBase
    {
        private readonly IWarehouseAppService _warehouseAppService;

        public WarehousesController(IWarehouseAppService warehouseAppService)
        {
            _warehouseAppService = warehouseAppService;
        }


        public async Task<IActionResult> Index()
        {
           
            return View();
        }

        public async Task<ActionResult> EditModal(int id)
        {
            var warehouses = await _warehouseAppService.GetAsync(id);
            var model = new CreateUpdateWarehouse() { Warehouse = warehouses };
            return PartialView("_EditModal", model);
        }
    }
}
