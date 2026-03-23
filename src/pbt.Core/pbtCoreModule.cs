using Abp.Dependency;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using Microsoft.Extensions.Configuration;
using pbt.Authorization;
using pbt.Authorization.Roles;
using pbt.Authorization.Users;
using pbt.Configuration;
using pbt.Core;
using pbt.Localization;
using pbt.MultiTenancy;
using pbt.Timing;
using System.Collections.Generic;
using System.Reflection;


namespace pbt
{
    [DependsOn(typeof(AbpZeroCoreModule) )]

    public class pbtCoreModule : AbpModule
    {
        private readonly IConfiguration _configuration;

        public pbtCoreModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            pbtLocalizationConfigurer.Configure(Configuration.Localization);

            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = pbtConsts.MultiTenancyEnabled;

            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.Settings.Providers.Add<AppSettingProvider>();
             

            Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = pbtConsts.DefaultPassPhrase;
            SimpleStringCipher.DefaultPassPhrase = pbtConsts.DefaultPassPhrase;

            IocManager.Register<IAbpSession, pbtAppSession>(DependencyLifeStyle.Transient);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(pbtCoreModule).GetAssembly());

            //IocManager.Register<ConnectDb>(DependencyLifeStyle.Transient);
            ConnectDb.Initialize(_configuration.GetConnectionString(pbtConsts.ConnectionStringName));

        }

        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
