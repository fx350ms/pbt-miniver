using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class PagedAndSortedOrderResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }

        public string DateRange { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public long? CustomerId { get; set; } 
        public int Status { get; set; } = -1;
        public int OrderType { get; set; } = -1;    
        public int? ShippingLine { get; set; }  // Line vận chuyển: Chính ngạch, tiểu ngạch
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

        //
        public string DeliveryStartDateStr { get; set; }
        public string DeliveryEndDateStr { get; set; }
        public DateTime? StartDeliveryDate
        {
            get
            {
                if (string.IsNullOrEmpty(DeliveryStartDateStr)) return null;
                if (DateTime.TryParseExact(DeliveryStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
        public DateTime? EndDeliveryDate
        {
            get
            {
                if (string.IsNullOrEmpty(DeliveryEndDateStr)) return null;
                if (DateTime.TryParseExact(DeliveryEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
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

    public class PagedMyOrderRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }

        public string DateRange { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public long? CustomerId { get; set; }
        public int Status { get; set; } = -1;
        public int ServiceId { get; set; } = -1;
        public int OrderType { get; set; } = -1;
        public int? ShippingLine { get; set; }  // Line vận chuyển: Chính ngạch, tiểu ngạch
        private static readonly string DateFormat = "dd/MM/yyyy";

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

        //
        public string DeliveryStartDateStr { get; set; }
        public string DeliveryEndDateStr { get; set; }
        public DateTime? StartDeliveryDate
        {
            get
            {
                if (string.IsNullOrEmpty(DeliveryStartDateStr)) return null;
                if (DateTime.TryParseExact(DeliveryStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                {
                    return startDate;
                }
                return null; // Hoặc bạn có thể trả về một giá trị mặc định nếu cần
            }
        }
        public DateTime? EndDeliveryDate
        {
            get
            {
                if (string.IsNullOrEmpty(DeliveryEndDateStr)) return null;
                if (DateTime.TryParseExact(DeliveryEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
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

        public bool IsExportExcel { get; set; } = false;
    }
}
