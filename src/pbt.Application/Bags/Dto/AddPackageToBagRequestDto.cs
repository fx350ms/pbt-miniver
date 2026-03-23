using System;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.Bags.Dto
{
    //custom PagedResultRequestDto
    public class AddPackageToBagRequestDto
    {
        public string PackageCode { get; set; }
        public int PackageId { get; set; }
        public int BagId { get; set; }
    }
}
