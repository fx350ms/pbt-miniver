using System.Collections.Generic;
using Abp.Application.Services.Dto;
using pbt.Bags.Dto;
using pbt.Packages.Dto;

namespace pbt.Export.Dto
{
    public class ExportItemsDto : EntityDto<int>
    {
        public List<BagDto> Bags { get; set; }
        public List<PackageDeliveryRequestDto> Packages { get; set; }
        
    }
}
