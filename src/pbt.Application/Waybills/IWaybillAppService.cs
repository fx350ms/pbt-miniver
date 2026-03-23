using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Users.Dto;
using pbt.Waybills.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.Waybills
{

    public interface IWaybillAppService : IAsyncCrudAppService<WaybillDto, long, PagedResultRequestDto, WaybillDto, WaybillDto>
    {
        public Task<List<string>> GetUnmatchedWaybillCodesAsync(string keyword);

        public Task<WaybillDto> GetByCode(string code);

        public Task<int> CreateListAsync(CreateWaybillListDto input);
        //public Task<List<WaybillDto>> GetFull(string customerId);

        //public Task<WaybillDto> CreateSimple(CreateWaybillSimpleDto input);
        //Task<List<WaybillDto>> SearchWaybill(string keyword, string customerId);
        //public Task<List<WaybillDto>> getChildWaybills(long waybillId);

    }
}
