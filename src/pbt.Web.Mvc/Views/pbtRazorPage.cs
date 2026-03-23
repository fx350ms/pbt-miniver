using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace pbt.Web.Views
{
    public abstract class pbtRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected pbtRazorPage()
        {
            LocalizationSourceName = pbtConsts.LocalizationSourceName;
        }
    }
}
