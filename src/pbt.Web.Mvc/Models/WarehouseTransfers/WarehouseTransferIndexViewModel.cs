using pbt.Customers.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.WarehouseTransfers
{
    public class WarehouseTransferIndexViewModel
    {
        public List<WarehouseDto> Warehouses { get; set; }
        public List<CustomerDto> Customers { get; set; }
        public long? CurrentUserWarehouseId { get; set; }
    }
}
