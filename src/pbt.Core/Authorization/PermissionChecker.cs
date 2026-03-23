using Abp.Authorization;
using pbt.Authorization.Roles;
using pbt.Authorization.Users;

namespace pbt.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
