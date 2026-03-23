using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ShippingRates.Dto
{
    public class ShippingRateDto : EntityDto<long>
    {
        public long ShippingRateGroupId { get; set; } // ID của bảng giá vận chuyển
        public int ShippingTypeId { get; set; } // Chính ngạch, tiểu ngạch, TMĐT
        public int WarehouseFromId { get; set; }
        public int WarehouseToId { get; set; }
        public string Note { get; set; } // Ghi chú
        public bool ManualPricing { get; set; } // Giá nhập tay khi lên đơn
        public bool UseCumulativeFormula { get; set; } // Công thức lũy kế

        public List<ShippingRateTierDto> Tiers { get; set; } // Danh sách các bậc giá
    }
}