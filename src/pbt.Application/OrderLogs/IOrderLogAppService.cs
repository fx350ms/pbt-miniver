using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.OrderLogs.Dto;
using pbt.Complaints.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.OrderLogs
{
    public interface IOrderLogAppService : IAsyncCrudAppService<OrderLogDto, long, PagedResultRequestDto, OrderLogDto, OrderLogDto>
    {
        public Task<List<OrderLogDto>> GetByOrderId(long orderId);
    }
}
