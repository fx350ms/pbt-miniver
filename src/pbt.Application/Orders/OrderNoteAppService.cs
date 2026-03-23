using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.Orders.Dto;
using System.Threading.Tasks;
using System.Linq;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Abp.Runtime.Session;
using pbt.Authorization;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace pbt.Orders
{
    public class OrderNoteAppService : AsyncCrudAppService<
        OrderNote,
        OrderNoteDto,
        long,
        PagedOrderNoteResultRequestDto,
        OrderNoteDto,
        OrderNoteDto>,
        IOrderNoteAppService
    {

        pbtAppSession _pbtAppSession;

        public OrderNoteAppService(IRepository<OrderNote, long> repository,
            pbtAppSession pbtAppSession
            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;

        }

        public async override Task<PagedResultDto<OrderNoteDto>> GetAllAsync(PagedOrderNoteResultRequestDto input)
        {
            var query = CreateFilteredQuery(input);
            query = query.Where(note => note.OrderId == input.OrderId);
            var totalCount = query.Count();
            var items = query
                .OrderByDescending(note => note.Id)
                .PageBy(input)
                .ToList();
            return new PagedResultDto<OrderNoteDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<OrderNoteDto>>(items)
            };
        }

        public async Task<List<OrderNoteDto>> GetAllByOrderIdAsync(long orderId)
        {
            try
            {
                var data = await  Repository.GetAllListAsync(note => note.OrderId == orderId);
                return ObjectMapper.Map<List<OrderNoteDto>>(data);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            
        }

        public override async Task<OrderNoteDto> CreateAsync(OrderNoteDto input)
        {
            try
            {
                input.CreatorUserName = _pbtAppSession.UserName;
                input.CreationTime = DateTime.Now;
                return await base.CreateAsync(input);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            
        }
    }
}