using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Packages.Dto
{
    /// <summary>
    /// Represents a paged result of sale view data.
    /// </summary>
    /// <typeparam name="T">The type of the items in the result.</typeparam>
    public class PagedSaleViewResultDto<T> : PagedResultDto<T> 
    {
        public decimal TotalWeight { get; set; } = 0;
    }
}
