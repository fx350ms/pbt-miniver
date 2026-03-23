using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    public class ChargingRequest : Entity<long>
    {
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string ReferenceCode { get; set; } // Mã tham chiếu
        public string Source { get; set; } // Nguồn truy cập (Id thiết bị điện thoại, IP của máy truy cập)
        public int SourceType { get; set; } // IP, Serial, Máy nội bộ ....
    }
}
