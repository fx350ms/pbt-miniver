using Abp.Application.Services.Dto;

namespace pbt.DeliveryNotes.Dto;

public class UpdateDeliveryNoteStatusDto : EntityDto
{
    public int Id { get; set; }
    public int Status { get; set; }
}