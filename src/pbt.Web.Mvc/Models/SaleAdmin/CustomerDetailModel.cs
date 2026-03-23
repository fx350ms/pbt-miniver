using pbt.CustomerAddresss.Dto;
using pbt.Customers.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.SaleAdmin
{
    public class CustomerDetailModel
    {
        public long Id { get; set; }
        public CustomerDto Customer { get; set; }
        public CustomerAddressDto Address { get; set; }
    }

}
