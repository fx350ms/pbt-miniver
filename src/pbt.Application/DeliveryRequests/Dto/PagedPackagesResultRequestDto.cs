using Abp.Application.Services.Dto;

namespace pbt.DeliveryRequests.Dto
{
    public class PagedPackagesResultRequestDto : PagedResultRequestDto
    {
        public int DeliveryRequestId { get; set; }
    }
}
