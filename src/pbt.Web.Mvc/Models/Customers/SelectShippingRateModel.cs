
using pbt.ShippingRates.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Customers
{
    public class SelectShippingRateModel
    {
        public long CustomerId { get; set; }

        public List<ShippingRateGroupDto> ShippingGroups { get; set; } = new List<ShippingRateGroupDto>();
    }
}
