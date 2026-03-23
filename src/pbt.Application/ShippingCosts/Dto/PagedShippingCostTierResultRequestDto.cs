using Abp.Application.Services.Dto;

namespace pbt.ShippingCosts.Dto
{
    public class PagedShippingCostTierResultRequestDto : PagedResultRequestDto
    {
        public long ShippingRateId { get; set; }
    }
}
