using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.Bags.Dto;

public class BagClosedDto : EntityDto<int>
{
    [CanBeNull] public string BagCode { get; set; }
    public decimal? Weight { get; set; }
    public virtual int? TotalPackage { get; set; }
}