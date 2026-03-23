using Abp.Application.Services.Dto;
using System;

namespace pbt.Packages.Dto
{
    public class PagedDeliveryRequestsResultRequestDto : PagedResultRequestDto
    {
        public string? Code { get; set; }
        public int? Status { get; set; }

        public string DateRange { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public long? CustomerId { get; set; }

        private static readonly string DateFormat = "dd-MM-yyyy";
        public DateTime? StartDate
        {
            get
            {
                if (string.IsNullOrEmpty(StartDateStr)) return null;
                if (DateTime.TryParseExact(StartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }

        public DateTime? EndDate
        {
            get
            {
                if (string.IsNullOrEmpty(EndDateStr)) return null;
                if (DateTime.TryParseExact(EndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    return endDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }

    }
}
