using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Users;
using pbt.Web.Models.Users;
using pbt.Authorization.Users;
using Abp.Runtime.Security;
using pbt.Customers;
using pbt.CustomerAddresss;
using pbt.Warehouses;
using pbt.Users.Dto;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Users)]
    public class UsersController : pbtControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly ICustomerAppService _customerAppService;
        private readonly ICustomerAddressAppService _customerAddressAppService;
        private readonly IWarehouseAppService _warehouseAppService;

        private readonly string[] _roles;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UsersController(IUserAppService userAppService,
            ICustomerAppService customerAppService,
            ICustomerAddressAppService customerAddressAppService,
            IWarehouseAppService warehouseAppService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userAppService = userAppService;
            _customerAppService = customerAppService;
            _customerAddressAppService = customerAddressAppService;
            _warehouseAppService = warehouseAppService;
            _roles = httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
        }

        public async Task<ActionResult> Index()
        {
            var roles = (await _userAppService.GetRoles()).Items;
            var warehouses = await _warehouseAppService.GetFull();
            // remove admin role from roles if _roles not admin
            if (!_roles.Contains("admin"))
            {
                roles = roles.Where(r => r.NormalizedName != "ADMIN" && r.NormalizedName != "SALEADMIN").ToList();
            }
            var saleUsers = await _userAppService.GetUsersSaleForLookupByCurrentUser();
            var model = new UserListViewModel
            {
                Roles = roles,
                Warehouses = warehouses,
                SaleUsers = saleUsers,
                SaleAdminUsers = await _userAppService.GetUsersSaleAdmin()
            };
            return View(model);
        }

        public async Task<ActionResult> EditModal(long userId)
        {
            var user = await _userAppService.GetAsync(new EntityDto<long>(userId));
            var roles = (await _userAppService.GetRoles()).Items;
            var warehouses = await _warehouseAppService.GetFull();
            var model = new EditUserModalViewModel
            {
                User = user,
                Roles = roles,
                Warehouses = warehouses
            };
            return PartialView("_EditModal", model);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        public ActionResult ResetPassword(long userId)
        {
            return PartialView("_ResetPassword", new ResetUserPasswordDto() { UserId = userId });
        }

        public async Task<IActionResult> Information()
        {

            var userId = User.Identity.GetUserId().Value; //
            var user = await _userAppService.GetAsync(new EntityDto<long>(userId));
            var customer = await _customerAppService.GetByUserId(user.Id);

            var model = new UserInformationViewModel()
            {
                Customer = customer,
                User = user
            };
            return View(model);
        }

        public async Task<ActionResult> EditAddressModal(long id)
        {
            var dto = await _customerAddressAppService.GetAsync(new EntityDto<long>(id));

            return PartialView("_EditAddressModal", dto);
        }

    }
}
