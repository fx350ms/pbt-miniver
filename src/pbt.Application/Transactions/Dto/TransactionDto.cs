using Abp.Application.Services.Dto;
using System;

namespace pbt.Transactions.Dto
{
    public class TransactionDto : FullAuditedEntityDto<long>
    {
        public string TransactionId { get; set; } // Mã GD
        public string OrderId { get; set; } // Mã đơn
        public string TransactionContent { get; set; } // Nội dung giao dịch
        public string ExpensePurpose { get; set; } // Mục đích chi (Phí giao hàng, phí ship nội địa TQ, Tất cả)
        public long? ApproverUserId { get; set; } // Người duyệt
        public long? ConfirmerUserId { get; set; } // Người xác nhận GD
        public long? RecipientPayer { get; set; } // Người nhận/nộp (khách hàng)
        public decimal Amount { get; set; } // Giá trị
        public decimal TotalAmount { get; set; } // Tổng số tiền sau khi được cộng/trừ
        public string Currency { get; set; } // Đơn vị tiền tệ
        public int Status { get; set; } // Trạng thái (Duyệt, Chờ duyệt)
        public int TransactionType { get; set; } // Loại giao dịch (Nạp ví, thanh toán cho đơn)
        public int ExecutionSource { get; set; } // Nguồn thực hiện (hệ thống/admin)
        public string Notes { get; set; } // Ghi chú
        public int FundAccountId { get; set; } // Id quỹ
        public int TransactionDirection { get; set; } // Hướng giao dịch (Thu/Chi)
        public string RefCode { get; set; } // Mã tham chiếu
        public long? MessageId { get; set; } // Id tin nhắn

          // Các trường bổ sung
        public bool IsUpdateTransaction { get; set; } // Cộng/trừ tiền tài khoản
        public int CustomerTransactionUpdateType { get; set; } // 1. Không làm gì, 2. Công nợ, 3. Ví
        public string Files { get; set; } // Danh sách file đính kèm. Lưu dạng json với format: Id và Name 


        public string CustomerName { get; set; } // Tên khách hàng
    }
}