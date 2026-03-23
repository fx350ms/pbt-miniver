using pbt.DeliveryRequests.Dto;
using pbt.Packages.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.DeliveryRequests
{
    public class DeliveryRequestDetailModel
    {
        public DeliveryRequestDto Dto { get; set; }
        public List<PackageDto> Packages { get; set; }
    }
}
