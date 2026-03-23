using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    public class Device : FullAuditedEntity<long>
    {
        public string DeviceName { get; set; } // Tên thiết bị
        public string DeviceId { get; set; } // ID thiết bị: Serial Number
        public bool Enable { get; set; }
    }
}
