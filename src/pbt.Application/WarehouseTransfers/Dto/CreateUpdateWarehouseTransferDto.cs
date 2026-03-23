using System.ComponentModel.DataAnnotations;

namespace pbt.WarehouseTransfers.Dto
{
    public class CreateUpdateWarehouseTransferDto
    {
        [Required]
        public string TransferCode { get; set; }
        [Required]
        public int FromWarehouse { get; set; }
        public string FromWarehouseName { get; set; }
        public string FromWarehouseAddress { get; set; }
        [Required]
        public int ToWarehouse { get; set; }
        public string ToWarehouseName { get; set; }
        public string ToWarehouseAddress { get; set; }
        public string Note { get; set; }
        public int Status { get; set; }
    }
}