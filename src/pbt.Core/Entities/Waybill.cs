using Abp.Domain.Entities.Auditing;
using pbt.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace pbt.Entities
{
    public class Waybill : FullAuditedEntity<long> // Sử dụng FullAuditedEntity để ghi nhận các thông tin tạo, cập nhật, xóa
    {
        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillCode { get; set; }

        /// <summary>
        /// ID đơn hàng (FK đến Order)
        /// </summary>
        //public long? OrderId { get; set; }


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
        
      //  public long? CustomerId { get; set; }

        //[ForeignKey("OrderId")]
        //[CanBeNull]
        //public virtual Order Order { get; set; }
        
        //public long? ParentId { get; set; }
        //[CanBeNull] public virtual Waybill Parent { get; set; }

    }
}
