using Abp.Application.Services.Dto;
using System;

namespace pbt.Warehouses.Dto
{
    //custom PagedResultRequestDto
    public class PagedWarehouseResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
        
    }
}
