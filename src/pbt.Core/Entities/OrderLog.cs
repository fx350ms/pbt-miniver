using Abp.Domain.Entities;
using System;

namespace pbt.Entities
{
    public class OrderLog : Entity<long>
    {
        public long OrderId { get; set; } // FK đến Order
        public DateTime CreationTime { get; set; } // Thời gian
        public long ActorId { get; set; } // Tác nhân (VD: Nhân viên, Khách hàng)
        public string ActorName { get; set; } // Tác nhân (VD: Nhân viên, Khách hàng)
        public string Content { get; set; } // Nội dung nhật ký
    }
}
