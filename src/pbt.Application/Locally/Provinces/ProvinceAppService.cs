using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using System.Threading.Tasks;
using Abp.UI;
using pbt.Entities.Locally;
using pbt.Provinces.Dto;
using System.Collections.Generic;

namespace pbt.Provinces
{
    public class ProvinceAppService : AsyncCrudAppService<Province, ProvinceDto, int, PagedResultRequestDto, ProvinceDto, ProvinceDto>, IProvinceAppService
    {

        public ProvinceAppService(IRepository<Province, int> repository)
            : base(repository)
        {
        }

        public async Task<List<ProvinceDto>> GetFull()
        {
            var data = await Repository.GetAllAsync();
            if (data == null)
            {
                throw new UserFriendlyException($"Province is empty");
            }
            return ObjectMapper.Map<List<ProvinceDto>>(data);
        }

         
    }
}
