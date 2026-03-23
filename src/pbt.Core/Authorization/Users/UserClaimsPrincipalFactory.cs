using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using pbt.Authorization.Roles;
using Abp.Domain.Uow;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System;

namespace pbt.Authorization.Users
{
    public class UserClaimsPrincipalFactory : AbpUserClaimsPrincipalFactory<User, Role>
    {
        public UserClaimsPrincipalFactory(
            UserManager userManager,
            RoleManager roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IUnitOfWorkManager unitOfWorkManager)
            : base(
                  userManager,
                  roleManager,
                  optionsAccessor,
                  unitOfWorkManager)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var claim = await base.CreateAsync(user);
            claim.Identities.First().AddClaim(new Claim("CustomerId", Convert.ToString(user.CustomerId)));
            claim.Identities.First().AddClaim(new Claim("WarehouseId", Convert.ToString(user.WarehouseId)));
            return claim;
        }
 

    }
}
