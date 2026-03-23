using Abp.Domain.Entities.Auditing;
using System;


namespace pbt.Entities
{

    /// <summary>
    /// Lịch sử giao dịch ví hoặc công nợ của khách hàng
    /// </summary>
    public class CustomerTransaction : AuditedEntity<long>
    {
        public long CustomerId { get; set; }
        public int TransactionType { get; set; } 
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string ReferenceCode { get; set; } // Mã tham chiếu
        public decimal BalanceAfterTransaction { get; set; } // Số dư sau giao dịch
        // Navigation Properties
        public string Files { get; set; } // Danh sách file đính kèm. Lưu dạng json với format: Id và Name 
    }
}
