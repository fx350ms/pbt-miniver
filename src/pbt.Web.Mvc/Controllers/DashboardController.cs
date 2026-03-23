using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using pbt.Controllers;
using pbt.Authorization.Users;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize]
    public class DashboardController : pbtControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DashboardController(UserManager userManager,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<ActionResult> Index()
        {
            //var roles = _httpContextAccessor.HttpContext.User.Claims
            //  .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
            //  .Select(c => c.Value)
            //  .ToArray();

            //var url = "/";
            //// Admin
            //if (roles.Contains("admin"))
            //{
            //    url = "/dashboard";
            //    return Redirect(url);
            //}

            //// Sale admin
            //if (roles.Contains("saleadmin"))
            //{
            //    url = "/SaleAdmin";
            //    return Redirect(url);
            //}

            //// Sale admin
            //if (roles.Contains("customer"))
            //{
            //    url = "/Orders";
            //    return Redirect(url);
            //}


            //// Kho
            //if (roles.Contains("warehouse"))
            //{
            //    return Redirect(url);
            //}
            return View();
        }
    }
}
