using Abp.Application.Services.Dto;

namespace pbt.Transactions.Dto
{
    public class CharingSourceDto : FullAuditedEntityDto<int>
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public string Description { get; set; }
        public int SourceType { get; set; }
    }
}