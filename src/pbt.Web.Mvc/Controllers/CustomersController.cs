using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Customers;
using pbt.Customers.Dto;
using pbt.Users;
using pbt.Web.Models.Customers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using pbt.Warehouses;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using pbt.ShippingRates;

namespace pbt.Web.Controllers
{
    //  [AbpMvcAuthorize]
    [Authorize]
    public class CustomersController : pbtControllerBase
    {
        private readonly ICustomerAppService _customerService;
        private readonly IUserAppService _userService;
        private readonly IWarehouseAppService _warehouseService;
        private readonly IShippingRateGroupAppService _shippingRateGroupService;

        private readonly pbtAppSession _pbtAppSession;
        public CustomersController(ICustomerAppService customerAppService,
            IShippingRateGroupAppService shippingRateGroupService,
            IUserAppService userService,
            IWarehouseAppService warehouseAppService,
            pbtAppSession pbtAppSession
            )

        {
            _customerService = customerAppService;
            _userService = userService;
            _warehouseService = warehouseAppService;
            _pbtAppSession = pbtAppSession;
            _shippingRateGroupService = shippingRateGroupService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
          
            var model = new CustomerIndexViewModel()
            {
               
            };
            return View(model);
        }
 
        public async Task<ActionResult> EditModal(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var warehouses = await _warehouseService.GetByCountry(2);
            ViewBag.WarehouseSelectList = warehouses.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
            return PartialView("_EditModal", customer);
        }

        
        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var excelData = await _customerService.ExportCustomersToExcelAsync();
            var fileName = $"Customers_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

  
 

        [HttpGet]
        public async Task<IActionResult> ResetPassword(long id)
        {


            var model = new ChangePasswordDto()
            {
                CustomerId = id
            };

            return PartialView("_ResetPassword", model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateDebtLimit(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var model = new UpdateMaxDebtDto()
            {
                CustomerId = id,
                MaxDebt = customer.MaxDebt,
            };

            return PartialView("_UpdateDebtLimit", model);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerChilds(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var model = new CustomerListCustomerChildsDto()
            {
                CustomerId = id,
                CustomerDtos = await _customerService.GetChildren(customer.Id)
            };

            return PartialView("_CustomerChilds", model);
        }
    }
}
