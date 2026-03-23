using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Bags.Dto
{
    public class BagReadyForCreateDeliveryNoteDto : EntityDto<int>
    {
        public string BagCode { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public string Note { get; set; }
    }
}
