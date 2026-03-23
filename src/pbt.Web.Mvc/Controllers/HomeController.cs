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
    public class HomeController : pbtControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(UserManager userManager,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<ActionResult> Index()
        {
            var roles = _httpContextAccessor.HttpContext.User.Claims
              .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
              .Select(c => c.Value)
              .ToArray();

            var url = "/";
            //// Admin
            if (roles.Contains("admin"))
            {
                url = "/Users";
                return Redirect(url);
            }
            else if (roles.Contains("saleadmin") || roles.Contains("sale"))
            {
                url = "/SaleAdmin";
                return Redirect(url);
            }

            else if (roles.Contains("customer"))
            {
                url = "/Orders";
                return Redirect(url);
            }
            else if (roles.Contains("salecustom"))
            {
                url = "/Bags";
                return Redirect(url);
            }
            else if (roles.Contains("warehouse")|| roles.Contains("warehousetq") || roles.Contains("warehousevn"))
            {
                url = "/Operate/Create";
                return Redirect(url);
            }

            else if (roles.Contains("accounting"))
            {
                url = "/Transaction/Index";
                return Redirect(url);
            }

            else
            {
                url = "/Tracking";
                return Redirect(url);
            }

        }

        public async Task<ActionResult> Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }
}
