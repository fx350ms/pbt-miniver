using System.Collections.Generic;
using Abp.Application.Services.Dto;
using pbt.Bags.Dto;
using pbt.Packages.Dto;

namespace pbt.DeliveryNotes.Dto
{
    public class DeliveryNotePackageAndBagItemDto : EntityDto<int>
    {
        public string PackageCode { get; set; }
        public string BagCode { get; set; }
        public int ItemType { get; set; }

    }
}
