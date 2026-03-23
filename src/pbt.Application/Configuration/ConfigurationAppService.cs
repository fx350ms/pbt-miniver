using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using pbt.Configuration.Dto;

namespace pbt.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : pbtAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
