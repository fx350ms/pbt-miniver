using pbt.Entities;
using pbt.ShippingRates;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.ShippingRate
{
    public class ConfigureTiersModel
    {
        public long GroupId { get; set; } // Id của bảng ShippingRate
        public List<ProductGroupTypeDto> ProductGroupTypes { get; set; } // Danh sách các nhóm sản phẩm
        public List<WarehouseDto> WarehousesCN { get; set; } // Danh sách các kho hàng Trung quốc
        public List<WarehouseDto> WarehousesVN { get; set; } // Danh sách các kho hàng Việt nam
        public List<ShippingRateDto> ShippingRates { get; set; } // Danh sách các bảng ShippingRate
    }

    public class AssignToCustomersModel
    {
        public long GroupId { get; set; } // Id của bảng ShippingRate
        public  List<ShippingRateCustomerCheckDto> Customers { get; set; } = new List<ShippingRateCustomerCheckDto>();// Danh sách các khách hàng có thể chọn  
    }
}
