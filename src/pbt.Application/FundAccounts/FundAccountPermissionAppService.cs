using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using pbt.Entities;
using pbt.FundAccounts.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pbt.Authorization.Users;

namespace pbt.FundAccounts
{
    public class FundAccountPermissionAppService : AsyncCrudAppService<FundAccountPermission, FundAccountPermissionDto, int, PagedAndSortedResultRequestDto, CreateFundAccountPermissionDto, FundAccountPermissionDto>, IFundAccountPermissionAppService
    {
        private readonly IRepository<FundAccountPermission, int> _fundAccountPermissionRepository;
        private readonly IRepository<FundAccount, int> _fundAccountRepository;
        private readonly IRepository<User, long> _userRepository;

        public FundAccountPermissionAppService(
            IRepository<FundAccountPermission, int> repository,
            IRepository<FundAccount, int> fundAccountRepository,
            IRepository<User, long> userRepository)
            : base(repository)
        {
            _fundAccountPermissionRepository = repository;
            _fundAccountRepository = fundAccountRepository;
            _userRepository = userRepository;
        }

        public override async Task<FundAccountPermissionDto> CreateAsync(CreateFundAccountPermissionDto input)
        {
            // Check if the permission already exists
            var existingPermission = await _fundAccountPermissionRepository.FirstOrDefaultAsync(p => p.FundAccountId == input.FundAccountId && p.UserId == input.UserId);
            if (existingPermission != null)
            {
                throw new Abp.UI.UserFriendlyException("Permission already exists for this user and fund account.");
            }

            // Create the permission
            var permission = ObjectMapper.Map<FundAccountPermission>(input);
            await _fundAccountPermissionRepository.InsertAsync(permission);

            return MapToEntityDto(permission);
        }

        public async Task<List<FundAccountPermissionDto>> GetPermissionsByFundAccountIdAsync(int fundAccountId)
        {
            var permissions = await _fundAccountPermissionRepository.GetAllListAsync(p => p.FundAccountId == fundAccountId);
            var permissionDtos = ObjectMapper.Map<List<FundAccountPermissionDto>>(permissions);

            // Populate additional fields for display purposes
            foreach (var dto in permissionDtos)
            {
                var fundAccount = await _fundAccountRepository.FirstOrDefaultAsync(dto.FundAccountId);
                var user = await _userRepository.FirstOrDefaultAsync(dto.UserId);

                dto.FundAccountName = fundAccount?.AccountName;
                dto.UserName = user?.UserName;
            }

            return permissionDtos;
        }

        public async Task RevokePermissionAsync(int fundAccountId, long userId)
        {
            var permission = await _fundAccountPermissionRepository.FirstOrDefaultAsync(p => p.FundAccountId == fundAccountId && p.UserId == userId);
            if (permission != null)
            {
                await _fundAccountPermissionRepository.DeleteAsync(permission);
            }
        }

        public async Task AssignUsersToFundAccount(AssignUsersToFundAccountDto input)
        {
            var existingPermissions = await Repository.GetAllListAsync(p => p.FundAccountId == input.FundAccountId);

            // Find users to add (newly selected users)
            var usersToAdd = input.UserIds.Except(existingPermissions.Select(p => p.UserId)).ToList();

            // Find users to remove (deselected users)
            var usersToRemove = existingPermissions.Where(p => !input.UserIds.Contains(p.UserId)).ToList();

            // Add new permissions
            foreach (var userId in usersToAdd)
            {
                await Repository.InsertAsync(new FundAccountPermission
                {
                    FundAccountId = input.FundAccountId,
                    UserId = userId
                });
            }

            // Remove deselected permissions
            foreach (var permission in usersToRemove)
            {
                await Repository.DeleteAsync(permission);
            }
        }
    }
}