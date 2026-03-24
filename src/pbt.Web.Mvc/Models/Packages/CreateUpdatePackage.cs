using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc.Rendering;
using pbt.Commons.Dto;
using pbt.Customers.Dto;
using pbt.Packages.Dto;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;
using pbt.WoodenPackings.Dto;

namespace pbt.Web.Models.Packages
{
    public class CreateUpdatePackage
    {
        public CreateUpdatePackageDto Package { get; set; }
        public int AddressId { get; set; }
        public List<WarehouseDto> WarehousesVietNam { get; set; }
        public List<WarehouseDto> WarehousesChina { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public List<CustomerWithWarehouseDto> CustomerSelectListItems { get; set; }
        public List<PackageDetailDto> PackageDetailDtos { get; set; }
        public long? customerId { get; set;}
        public bool? SameCustomer { get; set; }
        public List<ProductGroupTypeDto> ProductGroupTypes { set; get; }
        public List<OptionItemDto> WoodenPackingDtos { set; get; }
        public decimal? RMBRate { get; set; }
        public int? WarehouseVnId { get; set;}
        public int? WarehouseCnId { get; set;}
    }
}
