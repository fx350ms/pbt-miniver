using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Customers;
using pbt.Departments;
using pbt.Orders;
using pbt.Web.Models.Departments;
using pbt.Web.Models.Orders;
using pbt.Web.Models.Waybills;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    [Authorize]
    public class WaybillController : pbtControllerBase
    {
        private readonly ICustomerAppService _customerService;
        private readonly IOrderAppService _orderService;
        private readonly IOrderNoteAppService _orderNoteAppService;

        public WaybillController(ICustomerAppService customerAppService,
            IOrderNoteAppService orderNoteAppService,
            IOrderAppService orderService)
        {
            _customerService = customerAppService;
            _orderService = orderService;
            _orderNoteAppService = orderNoteAppService;
        }


        public async Task<IActionResult> Index()
        {
            var model = new WaybillIndexModel()
            {
                Customers = await _customerService.GetCustomersByCurrentUserAsync(),
            };
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = new WaybillIndexModel()
            {
                Customers = await _customerService.GetCustomersByCurrentUserAsync(),
            };
            return View(model);
        }


        public async Task<IActionResult> ReMatch(string orderIds, long currentCustomerId)
        {
            var model = new RematchWaybillModel()
            {
                CurrentCustomerId = currentCustomerId,
                Customers = await _customerService.GetCustomersByCurrentUserAsync(),
                Waybills = await _orderService.GetWaybillByIds(orderIds)
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderNotes(long orderId)
        {
            var notes = await _orderNoteAppService.GetAllByOrderIdAsync(orderId);
            return PartialView("_WaybillNotes", notes);
        }

    }
}
