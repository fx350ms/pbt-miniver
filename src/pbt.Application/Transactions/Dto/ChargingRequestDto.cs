using Abp.Application.Services.Dto;
using System;

namespace pbt.Transactions.Dto
{
    public class ChargingRequestDto : EntityDto<long>
    {
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string ReferenceCode { get; set; }
        public string Source { get; set; }
        public int SourceType { get; set; }
        public string Sign { get; set; }
    }
}