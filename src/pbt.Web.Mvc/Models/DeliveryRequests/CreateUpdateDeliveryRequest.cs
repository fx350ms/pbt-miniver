using System.Collections.Generic;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Packages.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Web.Models.DeliveryRequests
{
    public class CreateUpdateDeliveryRequest
    {
        public List<CustomerDto> Customers { get; set; }
        public List<WarehouseDto> Warehouses { get; set; }
    }
}
