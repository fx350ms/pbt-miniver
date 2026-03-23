using Abp.Application.Services.Dto;
using System;

namespace pbt.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedDepartmentResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        
    }
}
