using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.CustomerFakes;
using pbt.Web.Models.CustomerFakes;
using System;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_CustomerFakes)]
    //  [AbpMvcAuthorize]
    public class CustomerFakesController : pbtControllerBase
    {
        private readonly ICustomerFakeAppService _customerService;

        public CustomerFakesController(ICustomerFakeAppService customerAppService)
        {
            _customerService = customerAppService;
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<ActionResult> EditModal(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var model = new CreateUpdateCustomerFake() { CustomerFake = customer };
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
                await _customerService.ImportCustomerFakesAsync(file);
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var excelData = await _customerService.ExportCustomerFakesToExcelAsync();
            var fileName = $"CustomerFakes_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

    }
}
