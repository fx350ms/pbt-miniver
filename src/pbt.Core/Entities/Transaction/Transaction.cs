using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Microsoft.AspNetCore.Mvc.Razor;
using System;

namespace pbt.Entities
{

    /// <summary>
    /// Lịch sử giao dịch của quỹ tiền
    /// </summary>
    public class Transaction : FullAuditedEntity<long>
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
        public int TransactionType { get; set; } // Loại giao dịch (Nạp ví, thanh toán cho đơn). \
        public int ExecutionSource { get; set; } // Nguồn thực hiện (hệ thống/admin)
        public string Notes { get; set; } // Ghi chú
        public int FundAccountId { get; set; } // Id quỹ
        public int TransactionDirection { get; set; } // Hướng giao dịch (Thu/Chi)
        public string RefCode { get; set; } // Mã tham chiếu
        public long? MessageId { get; set; } // Id tin nhắn


        public bool IsUpdateTransaction { get; set; } // cộng/trừ tiền tài khoản. Nếu phiếu thu thì cộng tiền vào tài khoản, nếu phiếu chi thì trừ tiền trong tài khoản
        /// <summary>
        /// Loại cập nhật giao dịch của khách hàng
        /// 0. Không làm gì
        /// 1. Công nợ
        /// 2. Ví
        /// </summary>
        public int CustomerTransactionUpdateType { get; set; }  
        
        public string Files { get; set; } // Danh sách file đính kèm. Lưu dạng json với format: Id và Name 

    }
}