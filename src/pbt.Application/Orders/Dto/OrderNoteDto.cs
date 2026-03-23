using Abp.Application.Services.Dto;
using System;

namespace pbt.Orders.Dto
{
    public class OrderNoteDto : EntityDto<long>
    {
        public long OrderId { get; set; }
        public string Content { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime CreationTime { get; set; }
    }
}