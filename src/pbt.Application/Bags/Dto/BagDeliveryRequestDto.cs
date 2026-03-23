using JetBrains.Annotations;

namespace pbt.Bags.Dto
{
    public class BagDeliveryRequestDto : BagDto
    {
        public int? DeliveryRequestOrderId { get; set; }
        public string DeliveryRequestOrderCode { get; set; } = "";
        public string Address { get; set; }
        public string OrderCode { get; set; }
    }
}