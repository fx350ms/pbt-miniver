using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.Formula.Functions;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Roles;
using pbt.Authorization.Users;
using pbt.Core;
using pbt.Entities;
using pbt.Roles.Dto;
using pbt.Users.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pbt.Users
{
    [AbpAuthorize(PermissionNames.Pages_Users)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _roles;
        private readonly IRepository<Customer, long> _customerRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;


        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IHttpContextAccessor httpContextAccessor,
            IRepository<Customer, long> customerRepository,
            IRepository<Warehouse> warehouseRepository
                )
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _httpContextAccessor = httpContextAccessor;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
            _customerRepository = customerRepository;
            _warehouseRepository = warehouseRepository;
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = true;

            if (!PermissionChecker.IsGranted(PermissionNames.Role_Admin) && PermissionChecker.IsGranted(PermissionNames.Role_SaleAdmin))
            {
                input.RoleNames = new[] { RoleConstants.sale };
                user.ParentId = AbpSession.UserId.Value;
            }

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            CheckErrors(await _userManager.CreateAsync(user, input.Password));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            CurrentUnitOfWork.SaveChanges();

            return MapToEntityDto(user);
        }

        [HttpPost]
        public override async Task<UserDto> UpdateAsync(UserDto input)
        {
            CheckUpdatePermission();

            var user = await _userManager.GetUserByIdAsync(input.Id);

            MapToEntity(input, user);

            CheckErrors(await _userManager.UpdateAsync(user));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            return await GetAsync(input);
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }

        public override async Task<PagedResultDto<UserDto>> GetAllAsync(PagedUserResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = CreateFilteredQuery(input);
            var isSale = !_roles.Contains(RoleConstants.admin) && (_roles.Contains(RoleConstants.saleadmin) || _roles.Contains(RoleConstants.sale));
            if (isSale)
            {
                List<long> saleIds = new List<long>();
                if (_roles.Contains(RoleConstants.saleadmin))
                {
                    saleIds = (await _customerRepository.GetAllAsync()).Where(x => x.SaleId == AbpSession.UserId).Select(y => y.Id).ToList();
                }
                query = query.Where(x => x.Customer.SaleId == AbpSession.UserId || x.Id == AbpSession.UserId || saleIds.Contains(x.Customer.SaleId));
            }

            if (input.WarehouseId.HasValue)
            {
                query = query.Where(x => x.WarehouseId == input.WarehouseId);
            }
            var totalCount = await query.CountAsync();
            query = ApplySorting(query, input);
            query = query.PageBy(input);
            var entities = await query.ToListAsync();
            var data = entities.Select(MapToEntityDto).ToList();
            var warehouseIds = entities.Where(u => u.WarehouseId.HasValue).Select(u => u.WarehouseId.Value).ToList();
            var warehouses = (await _warehouseRepository.GetAllAsync())
                   .Where(u => warehouseIds.Contains(u.Id));
            var warehouseDict = warehouses.ToDictionary(u => u.Id, u => u.Name);

            foreach (var user in data)
            {
                if (user.WarehouseId.HasValue && user.WarehouseId > 0)
                {
                    user.WarehouseName = warehouseDict[user.WarehouseId.Value] ?? "";
                }
            }

            return new PagedResultDto<UserDto>(
                totalCount,
                data
            );
        }


        public async Task<PagedResultDto<UserDto>> GetAllUsersByCurentUserAsync(PagedUserResultRequestDto input)
        {
            CheckGetAllPermission();

            try
            {
                var currentUserId = AbpSession.UserId;
                var permissionCase = -1;

                var userIds = new List<long>();

                // admin thì nhìn thấy tất cả tài khoản
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin))
                {
                    permissionCase = 1;
                    Logger.Info("Admin truy cập vào toàn bộ danh sách tài khoản.");
                }

                else if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                    var users = await ConnectDb.GetListAsync<UserWithHierarchyLevelDto>("SP_AbpUsers_GetHierarchyBySaleAdmin_FixedLevel", CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleAdminUserId", currentUserId)
                   });
                    userIds = users == null || users.Count == 0 ? new() : users.Distinct().Select(u => u.Id).ToList();
                }

                else if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Sale))
                {
                    permissionCase = 3;
                    var users = await ConnectDb.GetListAsync<UserWithHierarchyLevelDto>("SP_AbpUsers_GetHierarchyBySale_FixedLevel", CommandType.StoredProcedure,
                        new[]
                        {
                            new SqlParameter("@SaleUserId", currentUserId)
                        });
                    userIds = users == null || users.Count == 0 ? new() : users.Distinct().Select(u => u.Id).ToList();
                }

                else
                {
                    Logger.Warn($"Không có quyền truy cập danh sách khách hàng");
                    permissionCase = -1;
                }

                if (permissionCase <= 0)
                {
                    return new PagedResultDto<UserDto>(
                        0,
                        new List<UserDto>()
                    );
                }
                var query = CreateFilteredQuery(input);
                query = query.Where(
                    x => permissionCase == 1 // admin
                    || ((permissionCase == 2 || permissionCase == 3) && userIds.Contains(x.Id)) // saleadmin
                    );

                if (input.SaleUserId.HasValue && input.SaleUserId > 0)
                {
                    query = query.Where(x => x.ParentId == input.SaleUserId);
                }

                if (input.RoleId.HasValue && input.RoleId > 0)
                {
                    query = query.Where(x => x.Roles.Any(r => r.RoleId == input.RoleId));
                }

                if (input.WarehouseId.HasValue)
                {
                    query = query.Where(x => x.WarehouseId == input.WarehouseId);
                }
                var totalCount = await query.CountAsync();
                query = ApplySorting(query, input);
                query = query.PageBy(input);
                var entities = await query.ToListAsync();
                var data = entities.Select(MapToEntityDto).ToList();
                var warehouseIds = entities.Where(u => u.WarehouseId.HasValue).Select(u => u.WarehouseId.Value).ToList();
                var warehouses = (await _warehouseRepository.GetAllAsync()).Where(u => warehouseIds.Contains(u.Id));
                var warehouseDict = warehouses.ToDictionary(u => u.Id, u => u.Name);

                var parentIds = data.Where(u => u.ParentId.HasValue && u.ParentId > 0).Select(u => u.ParentId.Value).Distinct().ToList();

                var parentDict = (await Repository.GetAllListAsync(u => parentIds.Contains(u.Id))).ToDictionary(u => u.Id, u => u.UserName);

                foreach (var user in data)
                {
                    if (user.WarehouseId.HasValue && user.WarehouseId > 0)
                    {
                        user.WarehouseName = warehouseDict[user.WarehouseId.Value] ?? "";
                    }
                    if (user.ParentId.HasValue && user.ParentId > 0 && parentDict != null && parentDict.Count > 0)
                    {
                        user.SaleAdminUserName = parentDict.ContainsKey(user.ParentId.Value) ? parentDict[user.ParentId.Value] : "";
                    }
                }

                return new PagedResultDto<UserDto>(
                    totalCount,
                    data
                );

            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách khách hàng theo người dùng hiện tại", ex);
                throw;
            }
        }

        public async Task<List<UserSelectDto>> GetUsersSaleForLookupByCurrentUser()
        {
            try
            {
                var currentUserId = AbpSession.UserId;
                long inputParentId = -1;
                var permissionCase = -1;

                // admin thì nhìn thấy tất cả tài khoản
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin))
                {
                    permissionCase = 1;
                    Logger.Info("Admin truy cập vào toàn bộ danh sách tài khoản.");
                }

                else if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                    inputParentId = currentUserId.Value;
                }
                else
                {
                    return new List<UserSelectDto>();
                }


                var roleNames = string.Join(",", new[] { RoleConstants.saleadmin, RoleConstants.sale });
                var result = await ConnectDb.GetListAsync<UserSelectDto>("SP_AbpUsers_GetSalesByParentIdAndRoles", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@InputParentId",inputParentId),
                        new SqlParameter("@RoleNames",roleNames )
                    });

                return result;

            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách khách hàng theo người dùng hiện tại", ex);
                throw;
            }
        }


        public async Task<List<UserSelectDto>> GetUsersSaleAdmin()
        {
            try
            {
                var currentUserId = AbpSession.UserId;
                long inputParentId = -1;


                // admin thì nhìn thấy tất cả tài khoản
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin))
                {
                    var roleNames = string.Join(",", new[] { RoleConstants.saleadmin });
                    var result = await ConnectDb.GetListAsync<UserSelectDto>("SP_AbpUsers_GetSalesByParentIdAndRoles", CommandType.StoredProcedure,
                        new[]
                        {
                        new SqlParameter("@InputParentId",inputParentId),
                        new SqlParameter("@RoleNames",roleNames )
                        });

                    return result;
                }

                else
                {
                    return new List<UserSelectDto>();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách khách hàng theo người dùng hiện tại", ex);
                throw;
            }
        }


        public async Task<JsonResult> UpdateSaleAdminForUser(UpdateSaleAdminForUserDto input)
        {
            try
            {
                var user = await _userManager.GetUserByIdAsync(input.UserId);
                if (user == null)
                {
                    return new JsonResult(new { Success = false, Message = "User not found." });
                }
                // Kiểm tra user có phải Sale hay không


                user.ParentId = input.SaleAdminUserId;
                await Repository.UpdateAsync(user);

                return new JsonResult(new { Success = true, Message = "Sale Admin updated successfully." });
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating Sale Admin for user", ex);
                return new JsonResult(new { Success = false, Message = "An error occurred while updating Sale Admin." });
            }
        }


        [AbpAuthorize(PermissionNames.Pages_Users_Activation)]
        public async Task Activate(EntityDto<long> user)
        {
            await Repository.UpdateAsync(user.Id, async (entity) =>
            {
                entity.IsActive = true;
            });
        }

        [AbpAuthorize(PermissionNames.Pages_Users_Activation)]
        public async Task DeActivate(EntityDto<long> user)
        {
            await Repository.UpdateAsync(user.Id, async (entity) =>
            {
                entity.IsActive = false;
            });
        }

        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected override UserDto MapToEntityDto(User user)
        {
            var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

            var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

            var userDto = base.MapToEntityDto(user);
            userDto.RoleNames = roles.ToArray();

            return userDto;
        }

        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
            {
                CheckErrors(await _userManager.ChangePasswordAsync(user, input.NewPassword));
            }
            else
            {
                CheckErrors(IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password."
                }));
            }

            return true;
        }



        public async Task<bool> ResetUserPassword(ResetUserPasswordDto input)
        {
            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            var user = await _userManager.FindByIdAsync(input.UserId.ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }
            CheckErrors(await _userManager.ChangePasswordAsync(user, input.NewPassword));
            return true;
        }


        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to reset password.");
            }

            var currentUser = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }

            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return true;
        }

        [AbpAuthorize(PermissionNames.Function_EditUserSpecialPIN)]
        public async Task UpdateSpecialPin(ResetUserPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to reset password.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.SpecialPIN = input.NewPassword;
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        //public async Task<ListResultDto<UserDto>> GetUserSales()
        //{
        //    var query = await Repository.GetAllAsync();
        //    var data = query.ToList();

        //    return new ListResultDto<UserDto>()
        //    {
        //        Items = ObjectMapper.Map<List<UserDto>>(data)
        //    };
        //}

        public async Task<List<UserDto>> GetUserSales()
        {
            var roles = (await _roleRepository.GetAllAsync())
                .Where(c => c.NormalizedName.ToUpper() == RoleConstants.saleadmin.ToUpper() || c.NormalizedName.ToUpper() == RoleConstants.sale.ToUpper())
                .Select(c => c.Id).ToList();
            var query = (await Repository.GetAllAsync()).AsQueryable();
            query = query.Include(i => i.Roles);
            query = query.Where(c => c.Roles.Any(r => roles.Contains(r.RoleId)));
            var users = await query.ToListAsync();
            return ObjectMapper.Map<List<UserDto>>(users);
        }

        public async Task<ListResultDto<UserDto>> GetUserForFundAccount()
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.IsActive && !u.CustomerId.HasValue);
            var data = query.ToList();

            return new ListResultDto<UserDto>()
            {
                Items = ObjectMapper.Map<List<UserDto>>(data)
            };
        }

        public async Task<UserDto> GetByUsername(string username)
        {
            var query = await Repository.GetAllAsync();
            var data = await query.FirstOrDefaultAsync(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
            return data.MapTo<UserDto>();
        }

        /// <summary>
        /// Lấy danh sách user có role/permission warehouse
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// 
        [AbpAllowAnonymous]
        public async Task<List<UserSelectDto>> GetWarehouseUsersAsync()
        {
            var query = await Repository.GetAllAsync();

            var roles = _roleManager.Roles.Where(r => r.Permissions.Any(p =>
            //p.Name == PermissionNames.Pages_Warehouses ||
            p.Name == PermissionNames.Role_Warehouse))
                .Select(r => r.Id).ToList();

            query = query.Where(u => u.IsActive && !u.IsDeleted && u.Roles.Any(r => roles.Contains(r.RoleId)));

            return query.Select(u => new UserSelectDto
            {
                Id = u.Id,
                UserName = u.UserName
            }).ToList();

            //var users = query
            //    .Where(u => u.IsActive && !u.IsDeleted  )

            //    .Where(u => u.Roles.Any(r => r.Role.Permissions.Any(p => p.Name == PermissionNames.Pages_Warehouses)))
            //    .Select(u => new UserSelectDto
            //    {
            //        Id = u.Id,

            //        UserName = u.UserName

            //    })
            //    .ToList();

            return null;
        }
    }
}

