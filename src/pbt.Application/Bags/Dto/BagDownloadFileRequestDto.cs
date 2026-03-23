using System;
using Abp.Application.Services.Dto;

namespace pbt.Bags.Dto
{
    public class BagDownloadFileRequestDto : PagedResultRequestDto
    {
        public string Language { get; set; } = "vi";

        public string BagCode { get; set; }
        public string Keyword { get; set; }
        public long? CustomerId { get; set; } = -1;
        public int? WarehouseCreate { get; set; }
        public int? WarehouseDestination { get; set; }
        public string? CreateDate { get; set; }
        public string? FilterType { get; set; }
        public bool IsClosed { get; set; }
        public bool pendingBag { get; set; }
        public string ExportDate { get; set; }
        public string ImportDate { get; set; }
        public int Status { get; set; } = -1;
        public int BagType { get; set; } = -1;
        public int? ShippingPartnerId { get; set; }

        private static readonly string DateFormat = "dd/MM/yyyy HH:mm:ss";
        public bool IsExcel { get; set; }

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

        public string CreateStartDateStr { get; set; }
        public string CreateEndDateStr { get; set; }
        public DateTime? StartCreateDate
        {
            get
            {
                if (string.IsNullOrEmpty(CreateStartDateStr)) return null;
                if (DateTime.TryParseExact(CreateStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
            set => CreateStartDateStr = value?.ToString(DateFormat);
        }
        public DateTime? EndCreateDate
        {
            get
            {
                if (string.IsNullOrEmpty(CreateEndDateStr)) return null;
                if (DateTime.TryParseExact(CreateEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
       
    }
}
