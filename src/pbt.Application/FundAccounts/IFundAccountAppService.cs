using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.FundAccounts.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.FundAccounts
{
    public interface IFundAccountAppService : IAsyncCrudAppService<FundAccountDto, int, PagedResultRequestDto, FundAccountDto, FundAccountDto>
    {
        public Task<List<FundAccountDto>> GetActiveFundAccountsAsync();
        public Task<FundAccountDto> GetFundAccountByAccountNumberAsync(string accountNumber);
        public Task<List<UserWithPermissionDto>> GetUsersWithFundAccountPermissionAsync(int id);
        public Task<List<FundAccountDto>> GetFundAccountsByCurrentUserAsync();

    }
}