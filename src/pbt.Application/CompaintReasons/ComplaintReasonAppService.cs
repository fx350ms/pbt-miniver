using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.CompaintReasons.Dto;
using pbt.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.CompaintReasons
{
    public interface IComplaintReasonAppService : IAsyncCrudAppService<ComplaintReasonDto, int, PagedResultRequestDto, ComplaintReasonDto, ComplaintReasonDto>
    {
        public Task<List<ComplaintReasonDto>> GetAllListAsync();
    }

    public class ComplaintReasonAppService : AsyncCrudAppService<ComplaintReason, ComplaintReasonDto, int, PagedResultRequestDto, ComplaintReasonDto, ComplaintReasonDto>, IComplaintReasonAppService
    {
        public ComplaintReasonAppService(IRepository<ComplaintReason, int> repository)
           : base(repository)
        {
        }

        public async Task<List<ComplaintReasonDto>> GetAllListAsync()
        {
            var data =  await Repository.GetAllListAsync();
            return ObjectMapper.Map<List<ComplaintReasonDto>>(data);
        }
    }
}
