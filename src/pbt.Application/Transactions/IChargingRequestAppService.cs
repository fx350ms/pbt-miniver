using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Transactions.Dto;
using System.Threading.Tasks;

namespace pbt.Transactions
{
    public interface IChargingRequestAppService : IAsyncCrudAppService<ChargingRequestDto, long, PagedResultRequestDto, ChargingRequestDto, ChargingRequestDto>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<ChargingRequestDto> ProcessAsync(ChargingRequestDto input);
    }
}