using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.CompaintReasons.Dto
{
    public class ComplaintReasonDto : EntityDto<int>
    {
        public string Name { get; set; } // Tên lý do khiếu nại
        public string Description { get; set; } // Mô tả chi tiết
    }
}
