using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
 
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize]
    public class MyWalletController : pbtControllerBase
    {
        //private readonly IWalletTransactionAppService _walletTransactionService;

        //public MyWalletController(IWalletTransactionAppService walletTransactionService)
        //{
        //}


        public async Task<IActionResult> Index()
        {
            return View();
        }


        public async Task<IActionResult> Charge()
        {
            return View();
        }

    }
}
