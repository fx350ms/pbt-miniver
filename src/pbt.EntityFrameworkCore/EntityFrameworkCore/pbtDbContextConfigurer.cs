using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace pbt.EntityFrameworkCore
{
    public static class pbtDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<pbtDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<pbtDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
