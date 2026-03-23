using Abp.Application.Services.Dto;
using Abp.Application.Services;

using pbt.OrderHistories.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.OrderHistories
{
    public interface IOrderHistoryAppService : IAsyncCrudAppService<OrderHistoryDto, long, PagedResultRequestDto, OrderHistoryDto, OrderHistoryDto>
    {
        public Task<List<OrderHistoryDto>> GetByOrderId(long orderId);
    }
}
