using System.Threading.Tasks;
using pbt.Configuration.Dto;

namespace pbt.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
