using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
 
using pbt.OrderHistories.Dto;
using Abp.Application.Services.Dto;
using pbt.Complaints.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.AutoMapper;

namespace pbt.OrderHistories
{
    public class OrderHistoryAppService : AsyncCrudAppService<OrderHistory, OrderHistoryDto, long, PagedResultRequestDto, OrderHistoryDto, OrderHistoryDto>, IOrderHistoryAppService
    {

        public OrderHistoryAppService(IRepository<OrderHistory, long> repository)
            : base(repository)
        {

        }

        public async Task<List<OrderHistoryDto>> GetByOrderId(long orderId)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.OrderId == orderId);
            var data = query.ToList();
            return data.MapTo<List<OrderHistoryDto>>();
        }

    }
}
