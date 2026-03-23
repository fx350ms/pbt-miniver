using pbt.Commons.Dto;
using pbt.Customers.Dto;
using pbt.Packages.Dto;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Bags
{
    public class BagCreateViewModel
    {
        public CreateUpdatePackageDto Package { get; set; }
        public int AddressId { get; set; }
        public int ShippingType { get; set; }
        public string Receiver { get; set; }

        public List<WarehouseDto> WarehousesVietNam { get; set; }
        public List<WarehouseDto> WarehousesChina { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public List<OptionItemDto> CustomerSelectListItems { get; set; }
        public List<PackageDetailDto> PackageDetailDtos { get; set; }
        public long? customerId { get; set; }
        public bool? SameCustomer { get; set; }
        public List<ProductGroupTypeDto> ProductGroupTypes { set; get; }
        public decimal? RMBRate { get; set; }
        public int? WarehouseVnId { get; set; }
        public int? WarehouseCnId { get; set; }
    }
}
