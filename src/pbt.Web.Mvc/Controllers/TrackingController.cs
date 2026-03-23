using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.Dictionary;
using pbt.Orders;
using pbt.Web.Models.Dicsionarys;
using System.Threading.Tasks;
using System;
using System.Linq;
using Abp.Web.Models;
using pbt.Entities;
using pbt.ApplicationUtils;

namespace pbt.Web.Controllers
{

    public class TrackingController : pbtControllerBase
    {
        private readonly IOrderAppService _orderAppService;


        public TrackingController(IOrderAppService orderAppService)
        {
            _orderAppService = orderAppService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string waybill = "")
        {
            if (!string.IsNullOrEmpty(waybill))
            {
                var trackingInfo = await _orderAppService.LookupAsync(waybill, "");
                var model = new TrackingViewModel()
                {
                    WaybillCode = waybill,
                    TrackingResult = trackingInfo
                };
                return View(model);
            }
            else
            {
                return View(new TrackingViewModel());
            }

        }

        [HttpPost]
        public async Task<ActionResult> Lookup(TrackingViewModel data)
        {
            try
            {
               

                var trackingInfo = await _orderAppService.LookupAsync(data.WaybillCode, data.CustomerPhone);
                return PartialView("_LookupResult", trackingInfo);
            }
            catch
            {
                return PartialView("_LookupResult", null);
            }
        }

    }
}
