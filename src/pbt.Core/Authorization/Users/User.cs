using System;
using System.Collections.Generic;
using Abp.Authorization.Users;
using Abp.Extensions;
using pbt.Entities;

namespace pbt.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe!@#QWE";

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>(),
                WarehouseId = null,

            };

            user.SetNormalizedNames();

            return user;
        }

        public long? CustomerId { get; set; }
        public int? WarehouseId { get; set; }
        // Navigation property for Customer
        public virtual Customer Customer { get; set; }
        public string SpecialPIN { get; set; }

        // Trường này áp dụng cho Sale để biết được người quản lý trực tiếp của mình là ai
        public long? ParentId { get; set; }

    }
}
