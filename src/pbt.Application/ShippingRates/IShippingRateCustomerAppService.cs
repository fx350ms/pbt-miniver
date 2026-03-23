using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.ShippingRates.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pbt.ShippingRates
{
    public interface IShippingRateCustomerAppService : IAsyncCrudAppService<ShippingRateCustomerDto, long, PagedResultRequestDto, ShippingRateCustomerDto, ShippingRateCustomerDto>
    {
        public Task<List<ShippingRateCustomerCheckDto>> GetCustomersForShippingRate(long groupId);
        public Task AssignCustomersToShippingRate(long groupId, List<long> customerIds);
    }
}
