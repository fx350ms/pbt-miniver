using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.Orders.Dto
{
    public class CreateUpdateOrderNoteDto : EntityDto<long>
    {
        public long OrderId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        public string CreatorUserName { get; set; }
    }
}