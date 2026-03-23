using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.BarCodes.Dto
{
    public class PagedBarCodeOnCreateViewRequestDto : PagedResultRequestDto
    {
        public bool OnlyMyCode { get; set; }
    }
}
