using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.WarehouseTransfers.Dto
{
    public class CreatePackageWarehouseTransferDto
    {
        public int PackageId { get; set; }
        public int ToWarehouse { get; set; }
        public string Note { get; set; }   
    }
    public class CreateBagWarehouseTransferDto
    {
        public int BagId { get; set; }
        public int ToWarehouse { get; set; }
        public string Note { get; set; }
    }
}
