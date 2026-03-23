using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.ShippingRates.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.ShippingRates
{
    public interface IShippingRateTierAppService : IAsyncCrudAppService<ShippingRateTierDto, long, PagedShippingRateTierResultRequestDto, ShippingRateTierDto, ShippingRateTierDto>
    {
        public Task UpdateTierList(long shippingRateId, List<ShippingRateTierDto> data);
    }
}
