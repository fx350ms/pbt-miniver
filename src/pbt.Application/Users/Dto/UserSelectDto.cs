using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Users.Dto
{
    public class UserSelectDto : EntityDto<long>
    {
        public string UserName { get; set; }
    }

    public class UpdateSaleAdminForUserDto
    {
        public long  UserId { get;set; }
        public long SaleAdminUserId { get; set; }    
    }
}
