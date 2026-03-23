using pbt.Orders.Dto;
using pbt.Warehouses.Dto;
using System.Collections.Generic;
using pbt.CustomerAddresss.Dto;

namespace pbt.Web.Models.Orders
{
    public class CreateMyOrderModel
    {
        public CreateMyOrderDto Dto { get; set; }

        public List<WarehouseDto> WarehousesVietNam { get; set; }
        public List<WarehouseDto> WarehousesChina { get; set; }
        public List<CustomerAddressDto> Addresses { get; set; }
        public string WaybillCode { get; set; } 

    }
}
