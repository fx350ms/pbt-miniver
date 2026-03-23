using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Wards.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace pbt.Wards
{
    public interface IWardAppService : IAsyncCrudAppService<WardDto, int, PagedResultRequestDto, WardDto, WardDto>
    {

        Task<List<WardDto>> GetFullByDistrict(int districtId);
    }
}
