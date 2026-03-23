using Abp.Domain.Entities;
using System;

namespace pbt.Entities
{
    
    public class OrderHistory : Entity<long>
    {
        public long OrderId { get; set; } // FK đến Order
        public DateTime CreationTime { get; set; } // Thời gian
        public int Status { get; set; } // Trạng thái của Order
    }
}
