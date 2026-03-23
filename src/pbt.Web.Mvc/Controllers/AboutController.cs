using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using pbt.Controllers;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize]
    public class AboutController : pbtControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}
