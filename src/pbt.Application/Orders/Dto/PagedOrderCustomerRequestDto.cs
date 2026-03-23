using Abp.Application.Services.Dto;
using System;

namespace pbt.Orders.Dto
{
    //custom PagedResultRequestDto
    /// <summary>
    /// 
    /// </summary>
    public class PagedOrderCustomerRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// 
        /// </summary>
        public int? CustomerId { get; set; }

    }
}