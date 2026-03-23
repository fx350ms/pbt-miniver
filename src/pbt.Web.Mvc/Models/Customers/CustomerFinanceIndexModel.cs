using pbt.Commons.Dto;
using pbt.Customers.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Customers
{
    public class CustomerFinanceIndexModel
    {
        public CustomerDto CurrentCustomer { get; set; }
        public List<OptionItemDto> Customers { get; set; }
    }
}
