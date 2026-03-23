using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.OrderHistories.Dto
{
    public class OrderHistoryDto : EntityDto<long>
    {
        public long OrderId { get; set; } // FK đến Order
        public DateTime CreationTime { get; set; } // Thời gian
        public int Status { get; set; } // Trạng thái của Order
    }
}
