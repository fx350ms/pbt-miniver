using pbt.Customers.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Packages
{
    public class PackageIndexViewModel
    {
        public List<WarehouseDto> WarehouseVNs { get; set; }
        public List<CustomerDto> Customers { get; set; }

    }
}
