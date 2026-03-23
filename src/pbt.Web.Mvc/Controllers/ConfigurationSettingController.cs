using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Controllers;
using pbt.ConfigurationSettings;
using pbt.ConfigurationSettings.Dto;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Abp.Authorization;

namespace pbt.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ConfigurationSettingController : pbtControllerBase
    {
        private readonly IConfigurationSettingAppService _configurationSettingAppService;

        public ConfigurationSettingController(IConfigurationSettingAppService configurationSettingAppService)
        {
            _configurationSettingAppService = configurationSettingAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> EditModal(long id)
        {
            var model = await _configurationSettingAppService.GetAsync(new Abp.Application.Services.Dto.EntityDto<long>(id));
            return PartialView("_EditModal", model);
        }

        public async Task<IActionResult> TestKeyCache(string key)
        {
            var value = await _configurationSettingAppService.GetValueAsync(key);
            return Json(value);
        }


        public async Task<IActionResult> ClearKey(string key)
        {
            await _configurationSettingAppService.ResetCacheByKeyAsync(key);
            return Ok("Done");
        }


        public async Task<IActionResult> ClearAllCacheAsync()
        {
            await _configurationSettingAppService.ClearAllCacheAsync();
            return Ok("Done");
        }

        public async Task<string> GetValue(string key)
        {
            return await _configurationSettingAppService.GetValueAsync(key);
        }


        [AllowAnonymous]
        [AbpAllowAnonymous]
        public async Task<string> GetRMB()
        {
            string key = "ExchangeRateRMB";
            var rs =  await _configurationSettingAppService.GetValueAsync(key);
            return rs;
        }
    }
}