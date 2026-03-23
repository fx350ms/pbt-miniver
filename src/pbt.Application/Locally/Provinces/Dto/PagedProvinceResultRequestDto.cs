using Abp.Application.Services.Dto;
using System;

namespace pbt.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedProvinceResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
