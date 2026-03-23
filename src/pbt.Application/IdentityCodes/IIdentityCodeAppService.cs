using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Entities;
using pbt.OrderNumbers.Dto;

namespace pbt.OrderNumbers
{
    public interface IIdentityCodeAppService : IAsyncCrudAppService<IdentityCodeDto, long, PagedResultRequestDto, IdentityCodeDto, IdentityCodeDto>
    {
        public Task<IdentityCodeDto> GenerateNewSequentialNumberAsync(string prefix);
    }
}