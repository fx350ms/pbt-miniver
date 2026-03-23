using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.WarehouseTransfers.Dto
{
    public class CheckTransferResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
