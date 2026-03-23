#nullable enable
using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using pbt.Entities;
using pbt.Orders.Dto;

namespace pbt.Waybills.Dto
{
    public class WaybillDto : FullAuditedEntityDto<long>
    {
        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillCode { get; set; }

        /// <summary>
        /// ID đơn hàng (FK đến Order)
        /// </summary>
        public long? OrderId { get; set; }


        /// <summary>
        /// Mã đơn hàng 
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// Kho nhận (Warehouse)
        /// </summary>
        public int? ReceivingWarehouseId { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Thời gian nhận
        /// </summary>
        public DateTime? ReceivingTime { get; set; }

        /// <summary>
        /// Trạng thái vận đơn
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }

        public OrderDto? Order { get; set; }
        public long? ParentId { get; set; }
        [CanBeNull] public virtual Waybill ParentDto { get; set; }

      //  public long? CustomerId { get; set; }
    }
}
