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

    //  [permi] 

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
            var shippingGroups = await _shippingRateGroupService.GetAllListAsync();
            var userSales = await _customerService.GetUserSale();
            var model = new CustomerIndexViewModel()
            {
                ShippingRateGroups = shippingGroups,
                UserSales = userSales
            };
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> IndexChild(long? id)
        {
            var customerId = id ?? _pbtAppSession.CustomerId;
            if (customerId == null)
            {
                return Redirect("/Customers");
            }
            var customer = await _customerService.GetAsync(new EntityDto<long>(customerId.Value));
            if (customer == null )
            {
                return NotFound();
            }

            ViewBag.customerId = customerId;
            ViewBag.customerName = customer.Username;

            var userSales = await _customerService.GetUserSale();
            var model = new CustomerIndexViewModel()
            {
                UserSales = userSales
            };
            return View("IndexChilds", model);

        }

        public async Task<ActionResult> EditModal(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var warehouses = await _warehouseService.GetFull();
            ViewBag.WarehouseSelectList = warehouses.Select(x => new SelectListItem() { Text = x.FullAddress, Value = x.Id.ToString() });
            return PartialView("_EditModal", customer);
        }

        public async Task<ActionResult> Finance(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var customers = await _customerService.GetAllForSelectBySaleAsync("");
            var model = new CustomerFinanceIndexModel()
            {
                CurrentCustomer = customer,
                Customers = customers
            };
            return View("Finance", model);
        }

        public async Task<ActionResult> CreateTransaction(long customerId)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(customerId));

            var model = new CreateCustomerTransactionModel()
            {
                CustomerId = customerId,
                Dto = customer,

            };
            return View("CreateTransaction", model);
        }


        public async Task<ActionResult> SelectShippingRate(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            var model = new SelectShippingRateModel()
            {

                CustomerId = id,
                ShippingGroups = await _shippingRateGroupService.GetAllListAsync()
            };
            return PartialView("_SelectShippingRate", customer);
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

        [HttpGet("Customers/AddChildCustomer/{parentId}")]
        public async Task<IActionResult> AddChildCustomer(long parentId)
        {

            // get list customer where parent id not null
            var list = await _customerService.GetAllChildren();
            var model = new CustomerListCustomerChildsDto()
            {
                CustomerId = parentId,
                CustomerDtos = list
            };

            return PartialView("ChildCustomer", model);

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
