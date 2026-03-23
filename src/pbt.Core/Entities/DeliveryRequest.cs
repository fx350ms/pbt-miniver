using Abp.Domain.Entities.Auditing;

namespace pbt.Entities
{
    public class DeliveryRequest : FullAuditedEntity<int>
    {
        public string RequestCode { get; set; }
        public long CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public int Status { get; set; }
        public int ShippingMethod { get; set; }
        public long AddressId { get; set; }
      
        public string Note { get; set; }

        /// <summary>
        /// Tổng số tiền đơn hàng
        /// </summary>
        public decimal TotalAmount { get; set; }

        public int PackageCount { get; set; }

        public int PaymentStatus { get; set; }

        /// <summary>
        /// Phương thức thanh toán
        /// 1. Ví 
        /// 2. Công nợ
        /// </summary>
        public int PaymentMethod { get; set; }
        public string Address { get; set; }

        public bool Paid { get; set; }

        public decimal TotalWeight { get; set; }
        public int TotalPackage { get; set; }
    }
}