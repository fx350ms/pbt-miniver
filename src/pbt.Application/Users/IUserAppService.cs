using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Roles.Dto;
using pbt.Users.Dto;

namespace pbt.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task DeActivate(EntityDto<long> user);
        Task Activate(EntityDto<long> user);
        Task<ListResultDto<RoleDto>> GetRoles();
        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<bool> ChangePassword(ChangePasswordDto input);

        Task<List<UserDto>> GetUserSales();

        Task<UserDto> GetByUsername(string username);
        Task<ListResultDto<UserDto>> GetUserForFundAccount();

        Task<List<UserSelectDto>> GetWarehouseUsersAsync();
        Task<bool> ResetUserPassword(ResetUserPasswordDto input);
        Task<PagedResultDto<UserDto>> GetAllUsersByCurentUserAsync(PagedUserResultRequestDto input);
        Task<List<UserSelectDto>> GetUsersSaleForLookupByCurrentUser();
        Task<List<UserSelectDto>> GetUsersSaleAdmin();
    }
}
