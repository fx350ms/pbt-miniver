using pbt.Commons.Dto;
using pbt.Customers.Dto;
using pbt.Packages.Dto;
using pbt.ShippingPartners.Dto;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Packages
{
    public class PackageFinanceModel
    {
        public PackageFinanceDto Dto { get; set; }
        public int AddressId { get; set; }
        public WarehouseDto CreateWarehouse { get; set; }
        public WarehouseDto ToWarehouse { get; set; }
        public CustomerDto Customer { get; set; }
        public ProductGroupTypeDto ProductGroupType { set; get; }
        public decimal? RMBRate { get; set; }
        public ShippingPartnerDto ShippingPartner { get; set; }
    }
}
