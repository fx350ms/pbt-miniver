using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using pbt.EntityFrameworkCore.Seed;

namespace pbt.EntityFrameworkCore
{
    [DependsOn(
        typeof(pbtCoreModule), 
        typeof(AbpZeroCoreEntityFrameworkCoreModule))]
    public class pbtEntityFrameworkModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<pbtDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        pbtDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                        options.DbContextOptions.EnableSensitiveDataLogging();
                    }
                    else
                    {
                        pbtDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                        options.DbContextOptions.EnableSensitiveDataLogging();
                    }
                   
                });
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(pbtEntityFrameworkModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}
