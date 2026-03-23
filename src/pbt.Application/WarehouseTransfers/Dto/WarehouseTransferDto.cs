using Abp.Application.Services.Dto;

namespace pbt.WarehouseTransfers.Dto
{
    public class WarehouseTransferDto : FullAuditedEntityDto<int>
    {
        public string TransferCode { get; set; }
        public int FromWarehouse { get; set; }
        public string FromWarehouseName { get; set; }
        public string FromWarehouseAddress { get; set; }
        public int ToWarehouse { get; set; }
        public string ToWarehouseName { get; set; }
        public string ToWarehouseAddress { get; set; }
        public string Note { get; set; }

        public string Data { get; set; }
        public int Status { get; set; }
        public decimal ShippingFee { get; set; }
        public long CustomerId { get; set; }
        public long TotalQuantity { get; set; }
        public decimal TotalWeight { get; set; }

        public string CustomerName { get; set; }
    }
}