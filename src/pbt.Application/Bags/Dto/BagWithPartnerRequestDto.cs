using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Bags.Dto
{
    public class BagWithPartnerRequestDto : PagedResultRequestDto
    {

        private static readonly string DateFormat = "dd/MM/yyyy HH:mm:ss";

        public string Keyword { get; set; }

        // 2. Đối tác vận chuyển - name="ShippingPartnerId"
        // Sử dụng long? cho ID và cho phép giá trị null (hoặc -1 từ form)
        public long? ShippingPartnerId { get; set; }

        public int ShippingLine { get; set; } = -1; // Line vận chuyển
        // 4. Loại bao hàng - name="BagType"
        public int? BagType { get; set; }

        // 5. Trạng thái bao hàng - name="Status"
        public int? Status { get; set; }

        // 6. Khách hàng - name="CustomerId"
        public long? CustomerId { get; set; }

        // --- Các trường ngày tháng (Date Range) ---

        // 7. Ngày tạo - Dùng chuỗi để nhận trực tiếp từ hidden field của form
        // name="CreateStartDateStr" và name="CreateEndDateStr"

        public string CreateStartDateStr { get; set; }
        public string CreateEndDateStr { get; set; }

        public DateTime? StartCreateDate
        {
            get
            {
                if (string.IsNullOrEmpty(CreateStartDateStr)) return null;
                // Áp dụng logic thêm thời gian mặc định 00:00:00 nếu chỉ có ngày (chuỗi length <= 10)
                string dateStr = CreateStartDateStr.Length > 10 ? CreateStartDateStr : CreateStartDateStr + " 00:00:00";
                if (DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null;
            }
        }

        /// <summary>Ngày tạo (Kết thúc) - Parse từ CreateEndDateStr</summary>
        public DateTime? EndCreateDate
        {
            get
            {
                if (string.IsNullOrEmpty(CreateEndDateStr)) return null;
                // Áp dụng logic thêm thời gian mặc định 23:59:59 nếu chỉ có ngày (chuỗi length <= 10)
                string dateStr = CreateEndDateStr.Length > 10 ? CreateEndDateStr : CreateEndDateStr + " 23:59:59";
                if (DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null;
            }
        }

        // 8. Ngày xuất (ExportDateCN) - Dùng chuỗi
        // name="ExportStartDateStr" và name="ExportEndDateStr"
        public string ExportStartDateStr { get; set; }
        public string ExportEndDateStr { get; set; }

        /// <summary>Ngày xuất CN (Bắt đầu) - Parse từ ExportStartDateStr</summary>
        public DateTime? StartExportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ExportStartDateStr)) return null;
                string dateStr = ExportStartDateStr.Length > 10 ? ExportStartDateStr : ExportStartDateStr + " 00:00:00";
                if (DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null;
            }
        }

        /// <summary>Ngày xuất CN (Kết thúc) - Parse từ ExportEndDateStr</summary>
        public DateTime? EndExportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ExportEndDateStr)) return null;
                string dateStr = ExportEndDateStr.Length > 10 ? ExportEndDateStr : ExportEndDateStr + " 23:59:59";
                if (DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null;
            }
        }

        // 9. Ngày nhập (ImportDateVN) - Dùng chuỗi
        // name="ImportStartDateStr" và name="ImportEndDateStr"
        public string ImportStartDateStr { get; set; }
        public string ImportEndDateStr { get; set; }

        /// <summary>Ngày nhập VN (Bắt đầu) - Parse từ ImportStartDateStr</summary>
        public DateTime? StartImportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ImportStartDateStr)) return null;
                string dateStr = ImportStartDateStr.Length > 10 ? ImportStartDateStr : ImportStartDateStr + " 00:00:00";
                if (DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null;
            }
        }

        /// <summary>Ngày nhập VN (Kết thúc) - Parse từ ImportEndDateStr</summary>
        public DateTime? EndImportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ImportEndDateStr)) return null;
                string dateStr = ImportEndDateStr.Length > 10 ? ImportEndDateStr : ImportEndDateStr + " 23:59:59";
                if (DateTime.TryParseExact(dateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null;
            }
        }



        public string ExportVNStartDateStr { get; set; }
        public string ExportVNEndDateStr { get; set; }
        public DateTime? StartExportVNDate
        {
            get
            {
                if (string.IsNullOrEmpty(ExportVNStartDateStr)) return null;
                if (DateTime.TryParseExact(ExportVNStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
        public DateTime? EndExportVNDate
        {
            get
            {
                if (string.IsNullOrEmpty(ExportVNEndDateStr)) return null;
                if (DateTime.TryParseExact(ExportVNEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }

    }
}
