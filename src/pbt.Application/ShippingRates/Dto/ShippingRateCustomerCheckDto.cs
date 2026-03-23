using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ShippingRates
{
     public class ShippingRateCustomerCheckDto : EntityDto<long>
    {
        public long ShippingRateGroupId { get; set; } // ID của bảng giá vận chuyển
        public long CustomerId { get; set; } // ID của khách hàng
        public string CustomerName { get; set; } // Tên khách hàng
        public bool Selected { get; set; }
    }
}
