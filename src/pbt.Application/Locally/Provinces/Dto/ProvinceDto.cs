using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Provinces.Dto
{
    public class ProvinceDto : EntityDto<int>
    {
        public string Name { get; set; }
    }
}
