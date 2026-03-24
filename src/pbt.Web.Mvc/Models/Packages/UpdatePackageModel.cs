using System.Collections.Generic;
using pbt.Commons.Dto;
using pbt.Customers.Dto;
using pbt.Packages.Dto;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Web.Models.Packages
{
    public class UpdatePackageModel
    {
        public PackageDetailDto Dto { get; set; }
        public int AddressId { get; set; }
        public List<WarehouseDto> WarehousesVietNam { get; set; }
        public List<WarehouseDto> WarehousesChina { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public List<PackageDetailDto> PackageDetailDtos { get; set; }
        public long? customerId { get; set; }
        public bool? SameCustomer { get; set; }
        public List<ProductGroupTypeDto> ProductGroupTypes { set; get; }
        public decimal? RMBRate { get; set; }
        public List<OptionItemDto> WoodenPackingDtos { set; get; }
    }

    public class EditPackageByAdminModel
    {
        public PackageEditByAdminDto Dto { get; set; }
        public int AddressId { get; set; }
        public List<WarehouseDto> WarehousesVietNam { get; set; }
        public List<WarehouseDto> WarehousesChina { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public List<PackageDetailDto> PackageDetailDtos { get; set; }
        public long? customerId { get; set; }
        public bool? SameCustomer { get; set; }
        public List<ProductGroupTypeDto> ProductGroupTypes { set; get; }
        public decimal? RMBRate { get; set; }
        public List<OptionItemDto> WoodenPackingDtos { set; get; }
    }
}
