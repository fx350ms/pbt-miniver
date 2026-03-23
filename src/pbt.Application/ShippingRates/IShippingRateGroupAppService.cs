using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.ShippingRates.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.ShippingRates
{
    public interface IShippingRateGroupAppService : IAsyncCrudAppService<ShippingRateGroupDto, long, PagedAndSortedResultRequestDto, ShippingRateGroupDto, ShippingRateGroupDto>
    {
        Task<List<ShippingRateGroupDto>> GetAllListAsync();
        Task<PagedResultDto<ShippingRateGroupDto>> GetDataAsync(PagedAndSortedShipingGroupResultRequestDto input);
        public  Task SetAsDefault(long id);
    }
}