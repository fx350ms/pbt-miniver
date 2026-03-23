using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace pbt.Controllers
{
    public abstract class pbtControllerBase: AbpController
    {
        protected pbtControllerBase()
        {
            LocalizationSourceName = pbtConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
