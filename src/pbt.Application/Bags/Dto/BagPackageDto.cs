using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Bags.Dto
{
    public class BagPackageDto
    {
        public long Id { get; set; }
        public string PackageNumber { get; set; }
        public string BagNumber { get; set; }
        public string WaybillNumber { get; set; }
        public int ItemType { get; set; } // 1: Kiện, 2: Bao
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExportDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public decimal Weight { get; set; }
        public int TotalPackages { get; set; }
    }
}
