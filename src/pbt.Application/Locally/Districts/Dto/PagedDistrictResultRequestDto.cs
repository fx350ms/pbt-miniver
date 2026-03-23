using Abp.Application.Services.Dto;
using System;

namespace pbt.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedDistrictResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }

        public int ProvinceId { get; set; }
        
    }
}
