using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Waybills.Dto
{
    public class CreateWaybillListDto : FullAuditedEntityDto<long>
    {
        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillCodes { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }

       
    }
}
