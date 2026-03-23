using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;


namespace pbt.Entities
{
    public class ShippingRateTier : FullAuditedEntity<long>
    {
        public int ProductTypeId { get; set; }
        public decimal? FromValue { get; set; }       // Ví dụ: 0
        public decimal? ToValue { get; set; }         // Ví dụ: 20
        public string Unit { get; set; }             // đơn vị tính: "kg" hoặc "m3"
        public decimal PricePerUnit { get; set; }    // Ví dụ: 35.000đ / kg
        public long? ShippingRateId { get; set; }
        
        [ForeignKey("ShippingRateId")]
        public virtual ShippingRate ShippingRate { get; set; } // Liên kết với bảng ShippingRate
    }
}
