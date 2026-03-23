using Abp.Application.Services.Dto;
using System;

namespace pbt.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedUserResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
        public int? WarehouseId { get; set; }
        public int? SaleUserId { get; set; }
        public int? RoleId { get; set; }
    }
}
