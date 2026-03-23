using System;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.Export.Dto
{
    //custom PagedResultRequestDto
    public class PagedExportResultRequestDto : PagedResultRequestDto
    {
        [CanBeNull] public string BagCode { get; set; }
        public int? WarehouseCreate { get; set; }
        public int? WarehouseDestination { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? FilterType { get; set; }
        public bool IsClosed { get; set; }
        public bool pendingBag { get; set; }
    }
}
