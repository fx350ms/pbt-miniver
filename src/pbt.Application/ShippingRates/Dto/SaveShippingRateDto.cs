using System.Collections.Generic;

namespace pbt.ShippingRates.Dto
{
    public class SaveShippingRateDto
    {
        public long ShippingRateGroupId { get; set; } // ID of the shipping rate group
        public List<ShippingRateDto> ShippingRates { get; set; } // List of shipping rates
    }
}