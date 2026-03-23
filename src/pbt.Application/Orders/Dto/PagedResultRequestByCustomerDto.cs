using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class PagedResultRequestByCustomerDto : PagedResultRequestDto
    {
        public long CustomerId { get; set; }
    }
}
