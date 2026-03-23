using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    /// <summary>
    /// Bảng giá gốc của đối tác vận chuyển
    /// </summary>
    public class ShippingCostGroup : Entity<long>
    {
        /// <summary>
        /// Tên bảng giá vốn
        /// </summary>
        public string Name { get; set; } // Tên bảng giá  
        /// <summary>
        /// Ghi chú
        /// </summary>
        public string Note { get; set; }     // Ví dụ: "Không axit ăn mòn", "400-799kg/m3"  

        /// <summary>
        /// Đối tác vận chuyển
        /// </summary>
        public int ShippingPartnerId { get; set; } // Đối tác vận chuyển
        public bool IsActived { get; set; } // Trạng thái kích hoạt bảng giá
        public DateTime? FromDate { get; set; } // Ngày bắt đầu áp dụng
        public DateTime? ToDate { get; set; } // Ngày kết thúc áp dụng
    }

    public class ShippingCostBase : Entity<long>
    {
        /// <summary>
        /// 
        /// </summary>
        public long ShippingCostGroupId { get; set; }

        /// <summary>
        /// Line vận chuyển
        /// </summary>
        public int ShippingTypeId { get; set; } // Chính ngạch, tiểu ngạch, TMĐT  
        /// <summary>
        /// Kho nguồn
        /// </summary>
        public int WarehouseFromId { get; set; } // Kho nguồn

        /// <summary>
        /// Kho đích
        /// </summary>
        public int WarehouseToId { get; set; } // Kho đích
        
        public virtual ICollection<ShippingCostTier> Tiers { get; set; } // Liên kết với bảng ShippingRateTier  
    }

    public class ShippingCostTier : Entity<long>
    {
        public int ProductTypeId { get; set; }
        public decimal? FromValue { get; set; }       // Ví dụ: 0
        public decimal? ToValue { get; set; }         // Ví dụ: 20
        public string Unit { get; set; }             // đơn vị tính: "kg" hoặc "m3"
        public decimal PricePerUnit { get; set; }    // Ví dụ: 35.000đ / kg
        public long? ShippingCostBaseId { get; set; }

        [ForeignKey("ShippingCostBaseId")]
        public virtual ShippingCostBase ShippingCostBase { get; set; } // Liên kết với bảng ShippingCost
    }

}
