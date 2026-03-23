using Abp.Application.Services.Dto;
using System;

namespace pbt.Messages.Dto
{
    public class ReceiveMessageReponseDto
    {
        public int Code { get; set; } // Mã kết quả xử lý. 1: Thành công, 0: Thất bại
        public string Message { get; set; } // Thông điệp
        public long? MessageId { get; set; } // ID của tin nhắn đã xử lý
    }
}