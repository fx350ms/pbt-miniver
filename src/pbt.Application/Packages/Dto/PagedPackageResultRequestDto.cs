using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using System;

namespace pbt.Packages.Dto
{
    //custom PagedResultRequestDto
    /// <summary>
    /// 
    /// </summary>
    public class PagedPackageResultRequestDto : PagedResultRequestDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string Keyword { get; set; }
        public string WarehouseTransferCode { get; set; }
        public int? WarehouseId { get; set; } = -1;
        public int? WarehouseDestinationId { get; set; } = -1;
        public int? WarehouseStatus { get; set; }
        public int? ShippingStatus { get; set; }
        public string? CreateDate { get; set; }
        public string Process { get; set; }
        public int? BagId { get; set; }
        public int FilterType { get; set; } = -1;
        public int? FeatureId { get; set; }
        // public int? ServiceId { get; set; }
        public int? BagType { get; set; } = -1;
        public int? UnBagType { get; set; } = -1;
        public int? ServiceId { get; set; }
        public int? Status { get; set; }
        public int? ShippingLine { get; set; }


        public long? CustomerId { get; set; }

        public string Services { get; set; }
        private static readonly string DateFormat = "dd/MM/yyyy HH:mm:ss";
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
            set
            {
                CreateStartDateStr = value?.ToString(DateFormat);
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


        public string MatchStartDateStr { get; set; }
        public string MatchEndDateStr { get; set; }
        public DateTime? StartMatchDate
        {
            get
            {
                if (string.IsNullOrEmpty(MatchStartDateStr))
                    return null;
                if (DateTime.TryParseExact(MatchStartDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var startDate))
                    return startDate;
                return null;
            }
        }
        public DateTime? EndMatchDate
        {
            get
            {
                if (string.IsNullOrEmpty(MatchEndDateStr))
                    return null;
                if (DateTime.TryParseExact(MatchEndDateStr, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var endDate))
                    return endDate;
                return null;
            }
        }
        /// <summary>
        /// Indicates whether the result should be in Excel format.
        /// </summary>
        public bool IsExcel { get; set; }
        public bool ExcludeCoverWeight { get; set; } = true;
        public int ExcludeCoverWeightType { get; set; } = (int)ExcludeCoverWeightTypeFilter.ExcludeCoverWeight;
    }

    public class PagedPackageByCustomerRequestDto : PagedResultRequestDto
    {
        public long? CustomerId { get; set; }
    }


    public class PagedPackageByBagIdRequestDto : PagedResultRequestDto
    {
        public int BagId { get; set; }
    }

}
