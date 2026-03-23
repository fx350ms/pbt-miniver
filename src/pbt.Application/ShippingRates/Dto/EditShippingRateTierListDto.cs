using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ShippingRates.Dto
{
    public class EditShippingRateTierListDto
    {
        public long GroupId { get; set; } // ID của bảng giá vận chuyển
        public List<ShippingRateDto> ShippingRates { get; set; } // Danh sách các bậc giá
    }
}
