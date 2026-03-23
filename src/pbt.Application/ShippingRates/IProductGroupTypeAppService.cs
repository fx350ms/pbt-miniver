using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.ShippingRates.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.ShippingRates
{
    public interface IProductGroupTypeAppService : IAsyncCrudAppService<ProductGroupTypeDto, int, PagedResultRequestDto, ProductGroupTypeDto, ProductGroupTypeDto>
    {
        public Task<List<ProductGroupTypeDto>> GetAllListAsync();
    }
}
