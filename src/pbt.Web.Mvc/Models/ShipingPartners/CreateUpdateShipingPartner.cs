using pbt.Departments.Dto;
using pbt.ShippingPartners.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.Web.Models.ShipingPartners
{
    public class CreateUpdateShipingPartner
    {
        public ShippingPartnerDto ShipingPartner { get; set; }
    }
}
