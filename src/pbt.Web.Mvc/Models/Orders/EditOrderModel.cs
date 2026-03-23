using pbt.Orders.Dto;
using pbt.CustomerAddresss;
using pbt.Warehouses;
using System.Collections.Generic;
using JetBrains.Annotations;
using pbt.Warehouses.Dto;
using pbt.CustomerAddresss.Dto;

namespace pbt.Web.Models.Orders
{
    public class EditOrderModel
    {
        public OrderDto Dto { get; set; }
        [CanBeNull] public CustomerAddressDto CustomerAddress { get; set; }
        [CanBeNull] public List<WarehouseDto> WarehousesVietNam { get; set; }
        [CanBeNull] public List<WarehouseDto> WarehousesChina { get; set; }
        [CanBeNull] public string CustomerName { get; set; }    
        
    }
}
