using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.OrderLogs.Dto
{
    public class OrderLogDto :EntityDto<long>
    {
        public long OrderId { get; set; } // FK đến Order
        public DateTime CreationTime { get; set; } // Thời gian
        public long ActorId { get; set; } // Tác nhân (VD: Nhân viên, Khách hàng)
        public string ActorName { get; set; } // Tác nhân (VD: Nhân viên, Khách hàng)
        public string Content { get; set; } // Nội dung nhật ký
    }
}
