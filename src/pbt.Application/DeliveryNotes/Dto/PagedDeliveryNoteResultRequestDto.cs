using System;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.DeliveryNotes.Dto
{
    //custom PagedResultRequestDto
    public class PagedDeliveryNoteResultRequestDto : PagedResultRequestDto
    {
        private static readonly string DateFormat = "dd/MM/yyyy HH:mm:ss";

        public string Keyword { get; set; }
        public int? Status { get; set; }

        public string ExportStartDateVNStr { get; set; }
        public string ExportEndDateVNStr { get; set; }

        public DateTime? StartExportDateVN
        {
            get
            {
                if (string.IsNullOrEmpty(ExportStartDateVNStr)) return null;
                if (DateTime.TryParseExact(ExportStartDateVNStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
        public DateTime? EndExportDateVN
        {
            get
            {
                if (string.IsNullOrEmpty(ExportEndDateVNStr)) return null;
                if (DateTime.TryParseExact(ExportEndDateVNStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
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
        public long? CustomerId { get; set; }
        public int? ShippingPartnerId { get; set; }

        public decimal? FromWeight { get; set; }
        public decimal? ToWeight { get; set; }

    }
}
