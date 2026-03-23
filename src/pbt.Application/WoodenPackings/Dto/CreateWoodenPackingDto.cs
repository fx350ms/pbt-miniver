using System.Collections.Generic;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.Entities;


namespace pbt.WoodenPackings.Dto
{
    public class CreateWoodenPackingDto : EntityDto<long>
    {
        public Package Package { get; set; }
        [CanBeNull] public List<string> packageNumber { get; set; }
    }
}
