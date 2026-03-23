using System.Collections.Generic;
using JetBrains.Annotations;
using pbt.Bags.Dto;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Web.Models.Bags
{
    public class BagDetailModel
    {
        public BagDto Dto { get; set; }
        [CanBeNull] public List<WarehouseDto> destinationWarehouses { get; set; }
        [CanBeNull] public List<ShippingPartnerDto> ShippingPartnerDtos { get; set; }
        public decimal? BagCoverWeight { get; set; }
    }
}
