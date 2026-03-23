using pbt.CustomerFakes.Dto;
using pbt.Customers.Dto;
using pbt.ShippingRates.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Packages.Dto
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageDetailDto : PackageDto
    {
        /// <summary>
        /// 
        /// </summary>
        public CustomerFakeDto CustomerFake { get; set; }
        public CustomerDto Customer { get; set; }
        public int? FakePackages { get; set; }
        public string? FakeCompany { get; set; }
        public WarehouseDto? CnWarehouse { get; set; }
        public WarehouseDto? VnWarehouse { get; set; }
        public WarehouseDto? CurrentWarehouse { get; set; }
        public ProductGroupTypeDto ProductGroupType { get; set; }

    }
}