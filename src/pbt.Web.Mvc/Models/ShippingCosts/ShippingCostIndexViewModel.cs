using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;
using pbt.ShippingCosts.Dto;

namespace pbt.Web.Models.ShippingCosts
{
    public class ShippingCostIndexViewModel
    {
        public List<ShippingPartnerDto> ShippingPartners { get; set; }
        public List<WarehouseDto> WarehousesCN { get; set; }
        public List<WarehouseDto> WarehousesVN { get; set; }
        public ShippingCostGroupDto ShippingCostGroup { get; set; }
    }
}
