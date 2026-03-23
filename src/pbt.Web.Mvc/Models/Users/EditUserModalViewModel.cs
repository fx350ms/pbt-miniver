using System.Collections.Generic;
using System.Linq;
using pbt.Roles.Dto;
using pbt.Users.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Web.Models.Users
{
    public class EditUserModalViewModel
    {
        public UserDto User { get; set; }

        public IReadOnlyList<RoleDto> Roles { get; set; }
        
        public List<WarehouseDto> Warehouses { get; set; }


        public bool UserIsInRole(RoleDto role)
        {
            return User.RoleNames != null && User.RoleNames.Any(r => r == role.NormalizedName);
        }
    }
}
