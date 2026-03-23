using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ShippingRates.Dto
{
    public class ShippingRateTierDto : EntityDto<long>
    {
        public long ShippingRateId { get; set; }
        public int ProductTypeId { get; set; } // Liên kết với nhóm sản phẩm
        public decimal? FromValue { get; set; } // Giá trị bắt đầu
        public decimal? ToValue { get; set; } // Giá trị kết thúc
        public string Unit { get; set; } // Đơn vị tính: "kg" hoặc "m3"
        public decimal PricePerUnit { get; set; } // Giá trên mỗi đơn vị
    }
}
