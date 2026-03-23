using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.ComplaintImages.Dto;
using pbt.Entities;
 

namespace pbt.ComplaintImages
{
    public class ComplaintImageAppService : AsyncCrudAppService<ComplaintImage, ComplaintImageDto, long, PagedResultRequestDto, ComplaintImageDto, ComplaintImageDto>, IComplaintImageAppService
    {

        public ComplaintImageAppService(IRepository<ComplaintImage, long> repository)
            : base(repository)
        {
        }

    }
}
