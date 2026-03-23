using DocumentFormat.OpenXml.Office2010.ExcelAc;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Bags
{
    public class BagIndexViewModel
    {
        public List<WarehouseDto> WarehousesVN { get; set; }
        public List<WarehouseDto> WarehousesCN { get; set; }
        public List<ShippingPartnerDto> ShippingPartners { get; set; }
    }
}
