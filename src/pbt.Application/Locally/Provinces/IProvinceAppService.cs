using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Provinces.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace pbt.Provinces
{
    public interface IProvinceAppService : IAsyncCrudAppService<ProvinceDto, int, PagedResultRequestDto, ProvinceDto, ProvinceDto>
    {

        Task<List<ProvinceDto>> GetFull();

    }
}
