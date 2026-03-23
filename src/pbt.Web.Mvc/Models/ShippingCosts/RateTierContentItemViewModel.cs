using pbt.ShippingRates.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.ShippingCosts
{
    public class RateTierContentItemViewModel
    {
        public int ShippingTypeId { get; set; }
        public string LineName { get; set; }
        public int FromWarehouseId { get; set; }
        public string FromName { get; set; }
        public int ToWarehouseId { get; set; }
        public string ToName { get; set; }
        public List<ProductGroupTypeDto> ProductGroupTypes { get; set; }
    }
}

