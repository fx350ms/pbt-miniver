using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Customers;
using pbt.Users;
using pbt.Web.Models.Customers;
using System;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Customers)]
    //  [AbpMvcAuthorize]

    //  [permi] 
        
    public class CustomerTransactionsController : pbtControllerBase
    {
        private readonly ICustomerAppService _customerService;
        private readonly IUserAppService _userService;
        public CustomerTransactionsController(ICustomerAppService customerAppService,
            IUserAppService userService)

        {
            _customerService = customerAppService;
            _userService = userService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> EditModal(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var model = new CreateUpdateCustomer() { Customer = customer };
            return PartialView("_EditModal", model);
        }


        [HttpGet]
        public async Task<IActionResult> Import()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return RedirectToAction("Index");
            }

            try
            {
                await _customerService.ImportCustomersAsync(file);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var excelData = await _customerService.ExportCustomersToExcelAsync();
            var fileName = $"Customers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [HttpGet]
        public async Task<IActionResult> UserSales()
        {
            var list = await _userService.GetUserSales();
            return PartialView("UserSales", list);

        }

        [HttpGet]
        public async Task<IActionResult> LinkToUser(long id)
        {

            var data = await _userService.GetUserSales();
            var model = new LinkToUserModel()
            {
                Id = id,
                Users = data
            };

            return PartialView("_LinkUserModal", model);
        }
    }
}
