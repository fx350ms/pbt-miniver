using Abp.Application.Services;
using pbt.MultiTenancy.Dto;

namespace pbt.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

