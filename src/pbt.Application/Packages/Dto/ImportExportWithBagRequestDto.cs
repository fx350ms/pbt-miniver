using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Packages.Dto
{

   public class ImportExportWithBagRequestDto : PagedResultRequestDto
    {
        private static readonly string DateFormat = "dd/MM/yyyy";
            
        public long CustomerId { get; set; } = -1;
        public string Keyword { get; set; } = string.Empty;  // mã vận đơn

        public int? FromNumberOfPackage { get; set; } = -1;
        public int? ShippingPartnerIntern { get; set; } = -1;
        public int? ShippingPartnerDomestic { get; set; } = -1;
        public int? ToNumberOfPackage { get; set; } = -1;

        public string BagNumber { get; set; } = string.Empty; // Mã bao
        public long? FromWeight { get; set; } = 0; // Trọng lượng từ
        public long? ToWeight { get; set; } = 0; // Trọng lượng đến
        public decimal? ShipFeeFrom { get; set; } = 0; // 
        public decimal? ShipFeeTo { get; set; } = 0; //
        public decimal? WoodPackingFeeFrom { get; set; } = 0; // Phí gỗ 
        public decimal? WoodPackingFeeTo { get; set; } = 0; // Phí gỗ

      
        public int ShippingLine { get; set; } = -1; // Line vận chuyển

        public string ExportStartDateStr { get; set; }
        public string ExportEndDateStr { get; set; }
        public DateTime? StartExportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ExportStartDateStr)) return null;
                if (DateTime.TryParseExact(ExportStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
        public DateTime? EndExportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ExportEndDateStr)) return null;
                if (DateTime.TryParseExact(ExportEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }

        public string ImportStartDateStr { get; set; }
        public string ImportEndDateStr { get; set; }
        public DateTime? StartImportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ImportStartDateStr)) return null;
                if (DateTime.TryParseExact(ImportStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
        public DateTime? EndImportDate
        {
            get
            {
                if (string.IsNullOrEmpty(ImportEndDateStr)) return null;
                if (DateTime.TryParseExact(ImportEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
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
        /// <summary>
        /// Biến này dùng để xác định xem có xuất file excel hay không
        /// </summary>
        public bool IsExcel { get; set; }
        public bool ExcludeCoverWeight { get; set; } = true;

        // StartCreateDate
        public DateTime? StartCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
        
    }
}
