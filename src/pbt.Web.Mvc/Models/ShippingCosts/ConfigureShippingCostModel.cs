using pbt.Warehouses.Dto;
using System.Collections.Generic;
using pbt.ShippingCosts.Dto;

namespace pbt.Web.Models.ShippingCosts
{
    public class ConfigureShippingCostModel
    {
        public long GroupId { get; set; }
        //public List<ShippingTypeDto> ShippingTypes { get; set; }
        public List<WarehouseDto> WarehousesCN { get; set; }
        public List<WarehouseDto> WarehousesVN { get; set; }
        public List<ShippingCostBaseDto> ShippingCosts { get; set; }
        //public List<ProductGroupTypeDto> ProductGroupTypes { get; set; }
    }
}