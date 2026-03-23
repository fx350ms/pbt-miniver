using Abp.Application.Services.Dto;
using System;

namespace pbt.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedWardResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }

        public int DistrictId { get; set; }
        
    }
}
