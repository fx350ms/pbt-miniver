using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Abp.Runtime;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace pbt.Authorization
{
    public class pbtAppSession : ClaimsAbpSession, ITransientDependency
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public pbtAppSession(
            IPrincipalAccessor principalAccessor,
            IMultiTenancyConfig multiTenancy,
            ITenantResolver tenantResolver,
            IAmbientScopeProvider<SessionOverride> sessionOverrideScopeProvider

            ) :
            base(principalAccessor, multiTenancy, tenantResolver, sessionOverrideScopeProvider)
        {

        }

        public long? CustomerId
        {
            get
            {
                var claim = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == "CustomerId");
                if (string.IsNullOrEmpty(claim?.Value))
                {
                    return null;
                }

                return string.IsNullOrEmpty(claim.Value) ? null : Convert.ToInt64(claim.Value);
            }
        }

        public string UserName
        {
            get
            {
                var claim = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

                return claim == null ? string.Empty : claim.Value;
            }
        }

        public int? WarehouseId
        {
            get
            {
                var WarehouseId = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == "WarehouseId");
                if (string.IsNullOrEmpty(WarehouseId?.Value))
                {
                    return null;
                }

                return Convert.ToInt32(WarehouseId.Value);
            }
        }
    }
}
