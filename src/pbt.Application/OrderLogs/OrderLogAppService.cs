using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using Abp.Application.Services.Dto;
using pbt.OrderLogs.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using pbt.Complaints.Dto;
using System.Linq;
using Abp.AutoMapper;

namespace pbt.OrderLogs
{
    public class OrderLogAppService : AsyncCrudAppService<OrderLog, OrderLogDto, long, PagedResultRequestDto, OrderLogDto, OrderLogDto>, IOrderLogAppService
    {

        public OrderLogAppService(IRepository<OrderLog, long> repository)
            : base(repository)
        {
        }

        public async Task<List<OrderLogDto>> GetByOrderId(long orderId)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.OrderId == orderId);
            var data = query.ToList();
            return data.MapTo<List<OrderLogDto>>();
        }
    }
}
