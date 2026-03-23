using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using System;
using JetBrains.Annotations;
using pbt.Warehouses.Dto;
using pbt.Waybills.Dto;


namespace pbt.Orders.Dto
{
    public class OrderWaybillDto : FullAuditedEntityDto<long>
    {
        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillNumber { get; set; }

        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public int OrderStatus { get; set; }
        public DateTime? InTransitToChinaWarehouseTime { get; set; } // Thời gian hàng về kho TQ
        public DateTime? InTransitTime { get; set; } // Thời gian đang vận chuyển quốc tế
        public DateTime? InTransitToVietnamWarehouseTime { get; set; } // Thời gian hàng đến kho VN
        public DateTime? DeliveryTime { get; set; } // Thời gian  giao hàng

        public string TrackingNumber { get; set; }
    }
}
