using System.Threading.Tasks;
using Abp.Application.Services;
using pbt.Sessions.Dto;

namespace pbt.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
