using pbt.Commons.Dto;
using pbt.ShippingPartners.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.DeliveryNote
{
    public class DeliveryNoteIndexView
    {
        public List<OptionItemDto> Customers { set; get; }
        public List<ShippingPartnerDto> ShippingPartners { set; get; }
        public int Status { set; get; }
    }
}
