using pbt.Customers.Dto;
using pbt.Orders.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Orders
{
    public class CreateOrderModel
    {
        //public string WaybillCode { get; set; }
        public OrderDto Dto { get; set; } = new OrderDto();
        public List<CustomerDto> Customers { get; set; } 
    }
}
