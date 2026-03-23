using pbt.Application.WarehouseTransfers.Dto;
using pbt.WarehouseTransfers.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.WarehouseTransfers
{
    public class WarehouseTransferDetailViewModel
    {
        public WarehouseTransferDto Dto { get; set; }
        public List<WarehouseTransferDetailDto> Items { get; set; }
        public string CustomerName { get; set; }

    }
     
}
