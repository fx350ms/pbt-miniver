using Abp.Application.Services.Dto;
using System;

namespace pbt.Transactions.Dto
{
    public class CustomerTransactionDto : EntityDto<long>
    {
        public long CustomerId { get; set; }
        public int TransactionType { get; set; } // 1. Ví, 2. Công nợ
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public string ReferenceCode { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public string Files { get; set; } // Danh sách file đính kèm. Lưu dạng json với format: Id và Name 
    }
}