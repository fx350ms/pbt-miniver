using Abp.Application.Services.Dto;
using System;

namespace pbt.WarehouseTransfers.Dto
{
    public class PagedWarehouseTransferResultRequestDto : PagedResultRequestDto
    {
        private readonly string DateFormat = "dd/MM/yyyy";
        public string Keyword { get; set; }
        public int? FromWarehouseId { get; set; }
        public int? ToWarehouseId { get; set; }
        public int? Status { get; set; }
        public long? CustomerId { get; set; }

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
                return null;
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
                return null;
            }
        }
    }
}