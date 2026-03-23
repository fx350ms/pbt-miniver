using System.Collections.Generic;
using pbt.DeliveryNotes.Dto;
using pbt.Bags.Dto;
using pbt.Packages.Dto;

namespace pbt.Web.Models.DeliveryNote
{
    public class PrintDeliveryNoteViewModel
    {
        public DeliveryNoteDto DeliveryNote { get; set; }
        public List<BagDeliveryRequestDto> Bags { get; set; }
        public List<PackageDeliveryRequestDto> Packages { get; set; }
    }
}