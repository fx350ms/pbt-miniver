using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.ShippingRates.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using NPOI.SS.Formula.Functions;


namespace pbt.ShippingRates
{
    public class ProductGroupTypeAppService : AsyncCrudAppService<ProductGroupType, ProductGroupTypeDto, int, PagedResultRequestDto, ProductGroupTypeDto, ProductGroupTypeDto>, IProductGroupTypeAppService
    {
        
        public ProductGroupTypeAppService(IRepository<ProductGroupType, int> repository)
            : base(repository)
        {
        }

        public async Task<List<ProductGroupTypeDto>> GetAllListAsync()
        {
           

            var productGroupTypes = await Repository.GetAllListAsync();
            return ObjectMapper.Map<List<ProductGroupTypeDto>>(productGroupTypes);
        }

        
    }
}
