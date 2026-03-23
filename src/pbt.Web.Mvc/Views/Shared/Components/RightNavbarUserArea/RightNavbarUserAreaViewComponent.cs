using System.Threading.Tasks;
using Abp.Configuration.Startup;
using pbt.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace pbt.Web.Views.Shared.Components.RightNavbarUserArea
{
    public class RightNavbarUserAreaViewComponent : pbtViewComponent
    {
        private readonly ISessionAppService _sessionAppService;
        private readonly IMultiTenancyConfig _multiTenancyConfig;

        public RightNavbarUserAreaViewComponent(
            ISessionAppService sessionAppService,
            IMultiTenancyConfig multiTenancyConfig)
        {
            _sessionAppService = sessionAppService;
            _multiTenancyConfig = multiTenancyConfig;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new RightNavbarUserAreaViewModel
            {
                LoginInformations = await _sessionAppService.GetCurrentLoginInformations(),
                IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
            };

            return View(model);
        }
    }


    //public class RightNavbarUserWalletAreaViewComponent : pbtViewComponent
    //{
    //    private readonly ISessionAppService _sessionAppService;
    //    private readonly IMultiTenancyConfig _multiTenancyConfig;

    //    public RightNavbarUserWalletAreaViewComponent(
    //        ISessionAppService sessionAppService,
    //        IMultiTenancyConfig multiTenancyConfig)
    //    {
    //        _sessionAppService = sessionAppService;
    //        _multiTenancyConfig = multiTenancyConfig;
    //    }

    //    public async Task<IViewComponentResult> InvokeAsync()
    //    {
    //        var model = new RightNavbarUserAreaViewModel
    //        {
    //            LoginInformations = await _sessionAppService.GetCurrentLoginInformations(),
    //            IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled,
    //        };

    //        return View(model);
    //    }
    //}
}

