using Abp.Application.Services.Dto;
using System;

namespace pbt.Messages.Dto
{
    public class ReceiveMessageDto
    {
        
        public string DeviceName { get; set; } // Tên thiết bị
        public string DeviceId { get; set; } // ID thiết bị: Serial Number
        public string Content { get; set; } // Nội dung
        public string Progress { get; set; } // Tiến trình
        public string Log { get; set; } // Log
        public string Sign { get; set; } // Chữ ký
    }
     
}