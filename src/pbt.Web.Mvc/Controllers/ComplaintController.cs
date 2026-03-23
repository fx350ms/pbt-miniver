using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using System.Threading.Tasks;
using pbt.Complaints;
using pbt.Web.Models.Departments;
using pbt.Web.Models.Complaint;
using pbt.Complaints.Dto;
using pbt.Orders;
using pbt.ApplicationUtils;
using pbt.CompaintReasons;
using pbt.Orders.Dto;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Complaint)]
    //  [AbpMvcAuthorize]
    public class ComplaintController : pbtControllerBase
    {
        private readonly IComplaintAppService _complaintService;
        private readonly IOrderAppService _orderService;
        private readonly IComplaintReasonAppService _complaintReasonService;
        public ComplaintController(IComplaintAppService complaintService,
            IOrderAppService orderService,
            IComplaintReasonAppService complaintReasonService
            )
        {
            _complaintService = complaintService;
            _orderService = orderService;
            _complaintReasonService = complaintReasonService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var model = new CreateUpdateComplaintViewModel()
            {
                Complaint = new CreateUpdateComplaintDto() { },
                Orders = await _orderService.GetAllMyWaybillForSelection(
                    new GetAllMyWaybillForSelectionRequestDto()
                    {
                        Status = (int)OrderStatus.Delivered
                    }),
                ComplaintReasons = (await _complaintReasonService.GetAllListAsync())
            };
            return View(model);
        }

        //[HttpPost]
        //public async Task<ActionResult> Create(ComplaintDto input)
        //{

        //}

    }
}
