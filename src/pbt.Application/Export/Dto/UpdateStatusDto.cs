using Abp.Application.Services.Dto;

namespace pbt.Export.Dto;

public class UpdateStatusDto : EntityDto
{
    public int Id { get; set; }
    public int Status { get; set; }
}