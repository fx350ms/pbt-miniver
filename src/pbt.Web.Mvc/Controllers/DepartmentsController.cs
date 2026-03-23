using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Departments;
using pbt.Web.Models.Departments;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
  //  [AbpMvcAuthorize]
    public class DepartmentsController : pbtControllerBase
    {
        private readonly IDepartmentAppService _departmentService;

        public DepartmentsController(IDepartmentAppService departmentAppService)
        {
            _departmentService = departmentAppService;
        }


        public async Task<IActionResult> Index()
        {
           
            return View();
        }

        public async Task<ActionResult> EditModal(int id)
        {
            var department = await _departmentService.GetAsync(id);
            var model = new CreateUpdateDepartment() { Department = department };
            return PartialView("_EditModal", model);
        }
    }
}
