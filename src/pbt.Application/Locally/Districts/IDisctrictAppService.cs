using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Districts.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace pbt.Districts
{
    public interface IDistrictAppService : IAsyncCrudAppService<DistrictDto, int, PagedResultRequestDto, DistrictDto, DistrictDto>
    {
        Task<List<DistrictDto>> GetFullByProvince(int provinceId);
    }
}
