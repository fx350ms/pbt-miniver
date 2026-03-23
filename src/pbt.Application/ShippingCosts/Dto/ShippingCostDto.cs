using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace pbt.ShippingCosts.Dto
{
    /// <summary>
    /// Giá vốn
    /// </summary>
    public class ShippingCostBaseDto : EntityDto<long>
    {
        public long ShippingCostGroupId { get; set; }
        public int ShippingTypeId { get; set; }
        public int WarehouseFromId { get; set; }
        public int WarehouseToId { get; set; }
        public List<ShippingCostTierDto> Tiers { get; set; } // Danh sách các bậc giá
    }
}