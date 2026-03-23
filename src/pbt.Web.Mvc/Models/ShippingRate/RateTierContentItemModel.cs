using pbt.ShippingRates.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.ShippingRate
{
    public class RateTierContentItemModel
    {
        public int LineNumber { get; set; }
        public string LineName { get; set; }
        public int FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; }
        public int ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; }
        public List<ProductGroupTypeDto> ProductGroupTypes { get; set; }
    }
}