using Abp.Application.Services.Dto;

namespace pbt.ShippingCosts.Dto
{
    public class ShippingCostTierDto : EntityDto<long>
    {
        public int ProductTypeId { get; set; }
        public decimal? FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public string Unit { get; set; }
        public decimal PricePerUnit { get; set; }
        public long? ShippingCostId { get; set; }
    }
}