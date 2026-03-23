using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace pbt.Entities
{
    public class ShippingRateCustomer : Entity<long>
    {
        public long ShippingRateGroupId { get; set; } // Liên kết với bảng ShippingRateGroup
        public long CustomerId { get; set; }     // Liên kết với bảng Customer
        public virtual ShippingRateGroup ShippingRateGroup { get; set; }
        public virtual Customer Customer { get; set; }
    }
}