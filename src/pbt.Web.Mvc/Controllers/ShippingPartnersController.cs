using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Departments;
using pbt.ShippingPartners;
using pbt.Web.Models.Departments;
using pbt.Web.Models.ShipingPartners;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    //  [AbpMvcAuthorize]
    public class ShippingPartnersController : pbtControllerBase
    {
        private readonly IShippingPartnerAppService _shippingPartnerAppService;

        public ShippingPartnersController(IShippingPartnerAppService shippingPartnerAppService)
        {
            _shippingPartnerAppService = shippingPartnerAppService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> EditModal(int id)
        {
            var shipingPartner = await _shippingPartnerAppService.GetAsync(id);
            var model = new CreateUpdateShipingPartner() { ShipingPartner = shipingPartner };
            return PartialView("_EditModal", model);
        }

       
    }
}
