using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Transactions.Dto
{
    public class PagedCustomerTransactionResultRequestDto : PagedResultRequestDto
    {
        public int TransactionType { get; set; } = -1;
        public int TransactionDirection { get; set; } = -1;
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string Keyword { get; set; }

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
                return null; // Trả về null nếu không parse được
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
                return null; // Trả về null nếu không parse được
            }
        }
    }
}
