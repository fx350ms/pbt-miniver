#nullable enable
using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbt.Entities;
using pbt.Orders.Dto;

namespace pbt.Waybills.Dto
{
    public class CreateWaybillSimpleDto : FullAuditedEntityDto<long>
    {
        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillCode { get; set; }
    }
}
