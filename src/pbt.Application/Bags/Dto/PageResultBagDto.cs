using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Bags.Dto
{
    public class PageResultBagDto : PagedResultDto<BagDto>
    {
        public decimal TotalWeight { get; set; }
        public decimal TotalPackage { get; set; }
    }
}
