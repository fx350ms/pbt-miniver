
using pbt.Customers.Dto;
using pbt.Orders.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Orders
{
    public class OrderListIndexModel
    {

        public OrderSummaryDto SummaryDto { get; set; }
        public int OrderStatus { get; set; }
        public List<CustomerDto> Customers = new List<CustomerDto>();
    }
}
