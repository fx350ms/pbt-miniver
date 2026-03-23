using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomerListCustomerChildsDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 
        /// </summary>
        public List<CustomerDto> CustomerDtos { get; set; }
        /// <summary>
        /// Id of the parent customer
        /// </summary>
        public long CustomerId { get; set; }
    }
}
