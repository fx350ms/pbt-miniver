using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Dictionary.Dto
{
    public class DictionaryDto : EntityDto<int>
    {
        public string NameVi { get; set; }
        public string NameCn { get; set; }
        public string Description { get; set; }
    }
}
