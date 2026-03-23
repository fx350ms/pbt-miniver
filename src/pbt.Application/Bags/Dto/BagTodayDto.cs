using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Bags.Dto
{
    public class BagTodayDto : EntityDto<int>
    {
        public string BagCode { get; set; }
        public decimal Weight { get; set; }
        public int BagType { get; set; }
        public string Note { get; set; }
        public bool IsClosed { get; set; }
    }
}
