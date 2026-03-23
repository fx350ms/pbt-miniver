using System.Collections.Generic;
using Abp.Application.Services.Dto;
using pbt.Bags.Dto;
using pbt.Packages.Dto;

namespace pbt.DeliveryNotes.Dto
{
    public class DeliveryNoteItemsDto : EntityDto<int>
    {
        public List<BagDeliveryRequestDto> Bags { get; set; }
        public List<PackageDeliveryRequestDto> Packages { get; set; }
        
    }
}
