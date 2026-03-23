using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;


namespace pbt.Entities
{
    public class ShippingRate : FullAuditedEntity<long>
    {
        public long ShippingRateGroupId { get; set; } // ID của bảng giá vận chuyển (ShippingRateGroup)
        public int ShippingTypeId { get; set; } // Chính ngạch, tiểu ngạch, TMĐT  
        public int WarehouseFromId { get; set; }
        public int WarehouseToId { get; set; }
        public string Note { get; set; }     // Ví dụ: "Không axit ăn mòn", "400-799kg/m3"  
        public bool ManualPricing { get; set; } // Giá sẽ nhập tay khi lên đơn  
        public bool UseCumulativeFormula { get; set; } // Xác định có dùng công thức lũy kế hay không  
        public virtual ICollection<ShippingRateTier> Tiers { get; set; } // Liên kết với bảng ShippingRateTier  
    }
}
