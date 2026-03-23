using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.ComplaintImages.Dto;

namespace pbt.ComplaintImages
{
    public interface IComplaintImageAppService : IAsyncCrudAppService<ComplaintImageDto, long, PagedResultRequestDto, ComplaintImageDto, ComplaintImageDto>
    {
       
    }
}
