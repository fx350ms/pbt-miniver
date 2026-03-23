using System.Collections.Generic;
using pbt.Roles.Dto;

namespace pbt.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}