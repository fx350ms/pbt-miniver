using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Packages.Dto
{
    public class PackageReadyForCreateDeliveryNoteDto : EntityDto<int>
    {
        public string PackageNumber { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public string Note { get; set; }
    }
}
