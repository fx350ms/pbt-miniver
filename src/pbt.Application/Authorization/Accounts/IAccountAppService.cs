using System.Threading.Tasks;
using Abp.Application.Services;
using pbt.Authorization.Accounts.Dto;

namespace pbt.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
