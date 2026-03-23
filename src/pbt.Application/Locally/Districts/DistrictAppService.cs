using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using System.Threading.Tasks;
using Abp.UI;
using pbt.Entities.Locally;
using pbt.Districts.Dto;
using System.Collections.Generic;
using System.Linq;

namespace pbt.Districts
{
    public class DistrictAppService : AsyncCrudAppService<District, DistrictDto, int, PagedResultRequestDto, DistrictDto, DistrictDto>, IDistrictAppService
    {

        public DistrictAppService(IRepository<District, int> repository)
            : base(repository)
        {

        }

        public async Task<List<DistrictDto>> GetFullByProvince(int provinceId)
        {
            var query = Repository.GetAll();
            query = query.Where(u => u.ProvinceId == provinceId);
            var data = query.ToList();
            if (data == null)
            {
                throw new UserFriendlyException($" District is empty");
            }
            return ObjectMapper.Map<List<DistrictDto>>(data);
        }
  
    }
}
