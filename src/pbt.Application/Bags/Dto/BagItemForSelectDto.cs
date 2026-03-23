using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Bags.Dto
{
    public class BagItemForSelectDto : EntityDto<int>
    {
        public string BagCode { get; set; }
        public int BagType { get; set; }
        public int ShippingStatus { get; set; }
    }
}
