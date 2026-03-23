using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.ApplicationUtils;
using pbt.CustomerAddresss.Dto;
using pbt.Customers.Dto;

namespace pbt.DeliveryRequests.Dto
{
    public class DeliveryRequestItemDto : FullAuditedEntityDto<int>
    {

        public int DeliveryRequestId { get; set; }
        public int ItemId { get; set; }
        public string PackageCode { get; set; }
        public string BagNumber { get; set; }
        public int ItemType { get; set; } // 1: Kiện, 2: Bao
        public decimal Weight { get; set; }

        public int TotalPackages { get; set; }
        public string WaybillNumber { get; set; }
        public string TrackingNumber { get; set; }
    }
}
