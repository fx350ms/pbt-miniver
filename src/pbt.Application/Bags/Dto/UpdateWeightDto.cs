using Abp.Application.Services.Dto;

namespace pbt.Bags.Dto
{
    public class UpdateWeightDto : EntityDto<int>
    {
        public int Id { get; set; }
        public bool? IsWeightCover { get; set; }
        public decimal Weight { get; set; } 
        public decimal? WeightCover { get; set; } = 0;
    }
}
