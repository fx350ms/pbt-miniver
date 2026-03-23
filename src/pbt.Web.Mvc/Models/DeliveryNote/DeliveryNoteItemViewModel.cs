using pbt.DeliveryNotes.Dto;
using pbt.ShippingPartners.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.DeliveryNote
{
    public class DeliveryNoteItemViewModel
    {
        public long CustomerId { get; set; }
        public DeliveryNoteDto DeliveryNote { get; set; }
        public List<ShippingPartnerDto> ShippingPartners { set; get; }
    }
}
