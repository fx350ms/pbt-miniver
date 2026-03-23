using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.WarehouseTransfers.Dto
{
    public class TransferWarehouseScanCodeDto
    {
        public string Code { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public long CustomerId { get; set; }
        public bool LockCustomer { get; set; }
    }
}
