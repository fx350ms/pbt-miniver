using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using System.Threading.Tasks;
using Abp.UI;
using pbt.Entities.Locally;
using pbt.Wards.Dto;
using System.Collections.Generic;
using System.Linq;

namespace pbt.Wards
{
    public class WardAppService : AsyncCrudAppService<Ward, WardDto, int, PagedResultRequestDto, WardDto, WardDto>, IWardAppService
    {
        public WardAppService(IRepository<Ward, int> repository)
            : base(repository)
        {
        }

        public async Task<List<WardDto>> GetFullByDistrict(int districtId)
        {
            var query = Repository.GetAll();
            query = query.Where(u => u.DistrictId == districtId);
            var data = query.ToList();
            if (data == null)
            {
                throw new UserFriendlyException($" District is empty");
            }
            return ObjectMapper.Map<List<WardDto>>(data);
        }

         
    }
}
