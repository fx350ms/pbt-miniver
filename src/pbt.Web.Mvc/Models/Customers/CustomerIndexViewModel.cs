using pbt.Entities;
using pbt.ShippingRates.Dto;
using System.Collections.Generic;
using pbt.Users.Dto;

namespace pbt.Web.Models.Customers
{
    public class CustomerIndexViewModel
    {
        public List<ShippingRateGroupDto> ShippingRateGroups { get; set; } = new List<ShippingRateGroupDto>();
        public List<UserDto> UserSales { get; set; } 
    }
}
