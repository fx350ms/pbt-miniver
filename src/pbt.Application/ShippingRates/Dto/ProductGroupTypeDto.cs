using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ShippingRates.Dto
{
    public class ProductGroupTypeDto : EntityDto<int>
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Note { get; set; }
    }
}
