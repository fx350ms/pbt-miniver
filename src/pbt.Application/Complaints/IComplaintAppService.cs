using Abp.Application.Services;
using pbt.Complaints.Dto;
using System.Threading.Tasks;
using pbt.Users.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace pbt.Complaints
{
    public interface IComplaintAppService : IAsyncCrudAppService<ComplaintDto, long, PagedComplaintResultRequestDto, ComplaintDto, ComplaintDto>
    {
        public Task<ComplaintDto> CreateWithImagesAsync([FromForm] ComplaintDto input);

        public Task<List<ComplaintDto>> GetByOrderId(long orderId);
    }
}
