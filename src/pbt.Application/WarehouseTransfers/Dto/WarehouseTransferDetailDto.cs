using Abp.Application.Services.Dto;

namespace pbt.Application.WarehouseTransfers.Dto
{
    public class WarehouseTransferDetailDto : EntityDto<long>
    {
        public int WarehouseTransferId { get; set; }
        public int ItemId { get; set; }
        public string PackageCode { get; set; }
        public string BagNumber { get; set; }
        public int ItemType { get; set; } // 1: Kiện, 2: Bao
        public decimal Weight { get; set; }
    }
}