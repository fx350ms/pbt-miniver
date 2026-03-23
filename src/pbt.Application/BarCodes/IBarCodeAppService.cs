using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Users.Dto;
using pbt.BarCodes.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.BarCodes
{

    public interface IBarCodeAppService : IAsyncCrudAppService<BarCodeDto, long, PagedResultRequestDto, BarCodeDto, BarCodeDto>
    {
        public Task<int> DeleteByIdsAsync(List<long> ids);
        Task<PagedResultDto<BarCodeDto>> GetDataAsync(PagedBarCodeResultRequestDto input);
    }
}
