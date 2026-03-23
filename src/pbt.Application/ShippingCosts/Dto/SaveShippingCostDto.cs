using System.Collections.Generic;
using pbt.ShippingCosts.Dto;

namespace pbt.ShippingRates.Dto
{
    public class SaveShippingCostDto
    {
        public long ShippingCostGroupId { get; set; } // ID of the shipping rate group
        public List<ShippingCostBaseDto> ShippingCosts { get; set; } // List of shipping rates
    }
}