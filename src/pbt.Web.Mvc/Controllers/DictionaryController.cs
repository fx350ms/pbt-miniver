using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.Dictionary;
using pbt.Web.Models.Dicsionarys;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{

    [AbpMvcAuthorize]
    public class DictionaryController : pbtControllerBase
    {
        private readonly IDictionaryAppService _dictionaryAppService;

        public DictionaryController(IDictionaryAppService dictionaryAppService)
        {
            _dictionaryAppService = dictionaryAppService;
        }

        public async Task<IActionResult> Index()
        {

            return View();
        }

        public async Task<ActionResult> EditModal(int id)
        {
            var model = await _dictionaryAppService.GetAsync(new Abp.Application.Services.Dto.EntityDto<int>(id));
            return PartialView("_EditModal", model);
        }
    }
}
