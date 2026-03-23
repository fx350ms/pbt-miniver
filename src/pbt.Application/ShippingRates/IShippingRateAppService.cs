using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.ShippingRates.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.ShippingRates
{
    public interface IShippingRateAppService : IAsyncCrudAppService<ShippingRateDto, long, PagedResultRequestDto, ShippingRateDto, ShippingRateDto>
    {
        Task<ShippingCostResult> CalculateShippingCostForFinanceAsync(CalculateShippingInputDto input);
        public Task<List<ShippingRateDto>> GetByGroupIdAsync(long groupId);
    }

}
