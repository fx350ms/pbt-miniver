using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;


namespace pbt.Entities
{
    public class DeliveryRequestItem : FullAuditedEntity<long>
    {
        public int DeliveryRequestId { get; set; }
        public int ItemId { get; set; }
        public string PackageCode { get; set; }
        public string BagNumber { get; set; }
        public string WaybillNumber { get; set; }
        public string TrackingNumber { get; set; }  
        public int ItemType { get; set; } // 1: Kiện, 2: Bao
        public decimal Weight { get; set; }
    }
}
