using pbt.Customers.Dto;
using pbt.Orders.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Orders
{
    public class OrderSearchModel
    {
        public int OrderStatus { get; set; }
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    }
}
