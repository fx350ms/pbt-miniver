using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.FundAccounts;
using pbt.FundAccounts.Dto;
using pbt.Users;


//using pbt.Web.Models.FundAccounts;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    public class FundAccountController : pbtControllerBase
    {
        private readonly IFundAccountAppService _fundAccountService;
         private readonly IUserAppService _userAppService;

        public FundAccountController(IFundAccountAppService fundAccountService, IUserAppService userAppService)
        {
            _fundAccountService = fundAccountService;
            _userAppService = userAppService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> EditModal(int id)
        {
            var fundAccount = await _fundAccountService.GetAsync(new EntityDto<int>( id));
          //  var model = new FundAccountDto() { FundAccount = fundAccount };
            return PartialView("_EditModal", fundAccount);
        }

        public async Task<IActionResult> AssignToUserModal(int id)
        {
            var data = await _fundAccountService.GetUsersWithFundAccountPermissionAsync(id);
            ViewBag.FundAccountId = id;
            return PartialView("_AssignToUserModal", data);
        }
    }
}