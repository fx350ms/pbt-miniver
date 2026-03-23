using Abp.Application.Services.Dto;
using System;

namespace pbt.Messages.Dto
{
    public class MessageDto : EntityDto<long>
    {
        public string DeviceName { get; set; } // Tên thiết bị
        public string DeviceId { get; set; } // ID thiết bị: Serial Number
        public string Content { get; set; } // Nội dung
        public int Status { get; set; } // Trạng thái
        public string Progress { get; set; } // Tiến trình
        public string Log { get; set; } // Log
        public int? MessageType { get; set; } // Loại tin nhắn
        public bool? IsCorrectSyntax { get; set; }
        public DateTime? CreatedDate { get; set; } // Ngày tạo
        public DateTime? LastUpdatedDate { get; set; } // Ngày cập nhật

        public string Note { get; set; }
        public long? TransactionId { get; set; }

    }
     
}