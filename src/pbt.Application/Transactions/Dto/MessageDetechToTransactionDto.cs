using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Transactions.Dto
{
    public class MessageDetechToTransactionDto
    {
        public string AccountNumber { get; set; } // Số tài khoản nhận
        public decimal Amount { get; set; } // Số tiền cộng vào tài khoản
        public DateTime TransactionTime { get; set; } // Thời gian giao dịch
        public decimal BalanceAfterTransaction { get; set; } // Số dư sau giao dịch
        public string ReferenceCode { get; set; } // Mã tham chiếu giao dịch
        public string Sender { get; set; } // Người chuyển khoản
        public string Content { get; set; } // Nội dung giao dịch
        public int MessageType { get; set; }
        public bool IsCorrectSyntax { get; set; }
    }
}
