using System.Collections.Generic;
using pbt.Roles.Dto;

namespace pbt.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
