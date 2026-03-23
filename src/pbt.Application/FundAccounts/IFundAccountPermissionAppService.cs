using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.FundAccounts.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.FundAccounts
{
    public interface IFundAccountPermissionAppService : IAsyncCrudAppService<FundAccountPermissionDto, int, PagedAndSortedResultRequestDto, CreateFundAccountPermissionDto, FundAccountPermissionDto>
    {
        Task<List<FundAccountPermissionDto>> GetPermissionsByFundAccountIdAsync(int fundAccountId);
        Task RevokePermissionAsync(int fundAccountId, long userId);
    }
}