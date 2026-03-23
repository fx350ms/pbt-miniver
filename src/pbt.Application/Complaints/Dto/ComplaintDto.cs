using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace pbt.Complaints.Dto
{
    public class ComplaintDto : FullAuditedEntityDto<long>
    {
        public string ComplaintCode { get; set; } // Mã khiếu nại
        public long OrderId { get; set; } // Id đơn hàng
        public string OrderCode { get; set; } // Mã đơn hàng
        public int Status { get; set; } // Trạng thái (Pending, Resolved, etc.)
        public string Resolution { get; set; } // Phương án xử lý
        public decimal RefundAmount { get; set; } // Tiền hoàn
        public string Reason { get; set; } // Lý do
        public string Description { get; set; } // Đề nghị hoàn
        public decimal RefundAmountExpect { get; set; } // Tiền hoàn mong muốn


        public long AssignToUserId { get; set; } // Nhân viên xử lý
        public string AssignToUserName { get; set; } // Nhân viên xử lý

        public List<IFormFile> Images { get; set; }
        public string CreationTimeString => CreationTime.ToString("dd/MM/yyyy HH:mm");
    }
}
