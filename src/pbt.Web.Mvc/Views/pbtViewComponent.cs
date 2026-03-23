using Abp.AspNetCore.Mvc.ViewComponents;

namespace pbt.Web.Views
{
    public abstract class pbtViewComponent : AbpViewComponent
    {
        protected pbtViewComponent()
        {
            LocalizationSourceName = pbtConsts.LocalizationSourceName;
        }
    }
}
