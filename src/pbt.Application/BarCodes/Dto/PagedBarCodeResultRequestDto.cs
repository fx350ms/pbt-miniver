using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.BarCodes.Dto
{
    public class PagedBarCodeResultRequestDto : PagedResultRequestDto
    {
        public int? ActionType { get; set; } // Hành động: Nhập/Xuất
        public int? CodeType { get; set; } // Loại mã: bao/vận đơn
        public int? Warehouse { get; set; } // Loại kho: TQ/VN
        public long? UserId { get; set; } // Người quét
        public string Keyword { get; set; } // Từ khóa

        public string CreateStartDateStr { get; set; }
        public string CreateEndDateStr { get; set; }
        public DateTime? StartCreateDate
        {
            get
            {
                if (string.IsNullOrEmpty(CreateStartDateStr))
                    return null;
                if (DateTime.TryParseExact(CreateStartDateStr, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                    return startDate;
                return null;
            }
        }
        public DateTime? EndCreateDate
        {
            get
            {
                if (string.IsNullOrEmpty(CreateEndDateStr))
                    return null;
                if (DateTime.TryParseExact(CreateEndDateStr, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                    return endDate;
                return null;
            }
        }
    }

    public class PagedBarCodeRequestDto : PagedBarCodeResultRequestDto
    {
        public bool ShowAll { get; set; }
    }

}
