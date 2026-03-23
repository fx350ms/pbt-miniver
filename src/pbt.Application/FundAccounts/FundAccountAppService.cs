using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.FundAccounts.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using pbt.ApplicationUtils;
using pbt.Authorization.Users;
using Microsoft.AspNetCore.Identity;

namespace pbt.FundAccounts
{
    public class FundAccountAppService : AsyncCrudAppService<FundAccount, FundAccountDto, int, PagedResultRequestDto, FundAccountDto, FundAccountDto>, IFundAccountAppService
    {
        private readonly IRepository<FundAccountPermission> _fundAccountPermissionRepository;
        private readonly IRepository<User, long> _userManager;
        public FundAccountAppService(IRepository<FundAccount, int> repository,
             IRepository<FundAccountPermission> fundAccountPermissionRepository,
           IRepository<User, long> userManager
            )
            : base(repository)
        {
            _userManager = userManager;
            _fundAccountPermissionRepository = fundAccountPermissionRepository;
        }

        public override Task<FundAccountDto> UpdateAsync(FundAccountDto input)
        {
            return base.UpdateAsync(input);
        }

        public async Task<List<FundAccountDto>> GetActiveFundAccountsAsync()
        {
            var activeFundAccounts = await Repository.GetAllListAsync(fa => fa.IsActived);
            return ObjectMapper.Map<List<FundAccountDto>>(activeFundAccounts);
        }

        public async Task<FundAccountDto> GetFundAccountByAccountNumberAsync(string accountNumber)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(fa => fa.AccountNumber == accountNumber && fa.AccountType == (int)FundAccountType.Bank && fa.IsActived);
            var fundAccount = query.FirstOrDefault();
            return ObjectMapper.Map<FundAccountDto>(fundAccount);
        }


        public async Task<List<UserWithPermissionDto>> GetUsersWithFundAccountPermissionAsync(int id)
        {
            // Get all users
            try
            {
                var userQuery = await _userManager.GetAllAsync();
                userQuery = userQuery.Where(u => u.IsActive && !u.CustomerId.HasValue);
                var allUsers = userQuery.ToList();

                // Get all fund account permissions
                var fundAccountPermissionQuery = await _fundAccountPermissionRepository.GetAllAsync();
                fundAccountPermissionQuery = fundAccountPermissionQuery.Where(p => p.FundAccountId == id);
                var fundAccountPermissions = fundAccountPermissionQuery.ToList();

                // Map users with permission flag
                var usersWithPermission = allUsers.Select(user => new UserWithPermissionDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.EmailAddress,
                    HasPermission = fundAccountPermissions.Any(permission => permission.UserId == user.Id)
                }).ToList();

                return usersWithPermission;
            }
            catch (System.Exception ex)
            {

            }
            return null;
        }


        public async Task<List<FundAccountDto>> GetFundAccountsByCurrentUserAsync()
        {
            // Get the current logged-in user's ID
            var currentUserId = AbpSession.UserId;
            if (!currentUserId.HasValue)
            {
                throw new Abp.UI.UserFriendlyException("User is not logged in.");
            }

            // Get all fund account permissions for the current user
            var permissions = await _fundAccountPermissionRepository.GetAllListAsync(p => p.UserId == currentUserId.Value);

            // Get the fund account IDs from the permissions
            var fundAccountIds = permissions.Select(p => p.FundAccountId).Distinct().ToList();

            // Retrieve the fund accounts based on the IDs
            var fundAccounts = await Repository.GetAllListAsync(fa => fundAccountIds.Contains(fa.Id));

            // Map the fund accounts to DTOs
            return ObjectMapper.Map<List<FundAccountDto>>(fundAccounts);
        }

    }
}