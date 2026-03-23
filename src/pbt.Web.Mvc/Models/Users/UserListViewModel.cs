using pbt.Roles.Dto;
using pbt.Users.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<RoleDto> Roles { get; set; }
        public List<WarehouseDto> Warehouses { get; set; }
        public List<UserSelectDto> SaleUsers { get; set; }
        public List<UserSelectDto> SaleAdminUsers { get; set; }
    }
}
