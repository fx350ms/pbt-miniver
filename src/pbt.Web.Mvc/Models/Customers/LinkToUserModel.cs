using NPOI.SS.Formula.Functions;
using pbt.Users.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Customers
{
    public class LinkToUserModel
    {
        public long Id { get; set; }
        public IReadOnlyList<UserDto> Users { get; set; }

    }
}
