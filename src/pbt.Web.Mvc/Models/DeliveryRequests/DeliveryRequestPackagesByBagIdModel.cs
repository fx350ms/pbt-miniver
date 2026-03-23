using System.Collections.Generic;
using pbt.Packages.Dto;

namespace pbt.Web.Mvc.Models.DeliveryRequests
{
    public class DeliveryRequestPackagesByBagIdModel
    {
        public int BagId { get; set; }
        public List<PackageByBagDetailDto> Packages { get; set; }
    }
 
}