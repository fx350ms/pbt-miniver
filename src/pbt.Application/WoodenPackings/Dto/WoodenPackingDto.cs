using Abp.Application.Services.Dto;


namespace pbt.WoodenPackings.Dto
{
    public class WoodenPackingDto : EntityDto<long>
    {
        public string WoodenPackingCode { get; set; }
        public decimal WeightTotal { get; set; }
        public decimal VolumeTotal { get; set; }
        public decimal CostTotal { get; set; }
    }
}
