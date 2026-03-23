using Abp.Application.Services.Dto;
using System;

namespace pbt.Dictionary.Dto
{
    //custom PagedResultRequestDto
    public class PagedDictionaryResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        
    }
}
