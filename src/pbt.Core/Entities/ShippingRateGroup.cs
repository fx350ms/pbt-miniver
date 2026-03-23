using Abp.Domain.Entities.Auditing;
using System.Collections.Generic;


namespace pbt.Entities
{
    public class ShippingRateGroup : FullAuditedEntity<long>
    {
        public string Name { get; set; } // Tên bảng giá  
        public string Note { get; set; }     // Ví dụ: "Không axit ăn mòn", "400-799kg/m3"  
        public bool IsActived { get; set; } // Trạng thái kích hoạt bảng giá  
        public bool IsDefaultForCustomer { get; set; } // Trạng thái mặc định bảng giá cho khách hàng  
        public virtual ICollection<ShippingRate> ShippingRates { get; set; } // Liên kết với bảng ShippingRateTier  
        public virtual ICollection<ShippingRateCustomer> Customers { get; set; } // Liên kết với bảng ShippingRateCustomer  
    }
}
