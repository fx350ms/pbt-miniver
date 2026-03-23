using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using System;
using JetBrains.Annotations;
using pbt.Customers.Dto;
using pbt.Waybills.Dto;

namespace pbt.DeliveryRequests.Dto
{
    public class ItemInHouseVnDto : FullAuditedEntityDto<long>
    {
        public string OrderNumber { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillNumber { get; set; }

        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public long? CustomerId { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Loại đơn hàng sở hữu:
        /// True: Đơn của khách
        /// False: Đơn của mình
        /// </summary>
        public bool IsCustomerOrder { get; set; }

        /// <summary>
        /// Loại đơn
        /// 1. Đơn thường
        /// 2. Ký gửi
        /// </summary>
        public int OrderType { get; set; }


        /// <summary>
        /// Line vận chuyển: Chính ngạch, tiểu ngạch
        /// </summary>
        public int ShippingLine { get; set; }
        public string ShippingLineString
        {
            get
            {
                return ((pbt.ApplicationUtils.CustomerLine)ShippingLine).GetDescription();
            }
        }

        public string ShippingLineShortString
        {
            get
            {
                return ((pbt.ApplicationUtils.CustomerLine)ShippingLine).GetDescription();
            }
        }

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// Trạng thái vận chuyển
        /// </summary>
        public int ShippingStatus { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SellingPrice { get; set; }

        /// <summary>
        /// Id Của nhân viên kinh doanh
        /// </summary>
        public long UserSaleId { get; set; }


        public string OrderDateString => CreationTime.ToString("dd-MM-yyyy HH:mm");


        public string StatusString
        {
            get
            {
                return ((pbt.ApplicationUtils.OrderStatus)OrderStatus).GetDescription();
            }
        }

        public bool UseInsurance { get; set; }
        public bool UseWoodenPackaging { get; set; }
        public bool UseShockproofPackaging { get; set; }
        public bool UseDomesticTransportation { get; set; }
        public decimal Insurance { get; set; }
        public decimal PriceInsurance { get; set; }
        public decimal OrderFee { get; set; }  // Phí đặt hàng
        public decimal WoodenPackagingFee { get; set; }  // Phí đóng gỗ
        public decimal BubbleWrapFee { get; set; }  // Phí quấn bọt khí
        public decimal DomesticShipping { get; set; }  // Ship nội địa
        public decimal ECommerceShipping { get; set; }  // Vận chuyển Thương mại điện tử
        public decimal GoodsValue { get; set; }  // Giá trị hàng hóa
        public decimal TotalCost { get; set; }  // Tổng chi phí
        public decimal Paid { get; set; }  // Đã thanh toán
        public decimal AmountDue { get; set; }  // Cần thanh toán

        public int CNWarehouseId { get; set; }
        [CanBeNull] public string CNWarehouseName { get; set; }
        public int VNWarehouseId { get; set; }
        public string VNWarehouseName { get; set; }
        public int AddressId { get; set; }
        public string AddressName { get; set; }



        public int PaymentStatus { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal Dimension { get; set; }
        public decimal ServicesFee { get; set; }

        [CanBeNull] public virtual WaybillDto Waybill { get; set; }
        [CanBeNull] public virtual CustomerDto Customer { get; set; }
        
        public long? ParentId { get; set; }

        public string Note { get; set; } // [ADDED]
        public DateTime? InTransitToChinaWarehouseTime { get; set; } // [ADDED]
        public DateTime? InTransitTime { get; set; } // [ADDED]
        public DateTime? InTransitToVietnamWarehouseTime { get; set; } // [ADDED]
        public DateTime? DeliveryTime { get; set; } // [ADDED]
        public DateTime? ComplaintTime { get; set; } // [ADDED]
        public DateTime? RefundTime { get; set; } // [ADDED]
        public DateTime? CancelledTime { get; set; } // [ADDED]
        public DateTime? OrderCompletedTime { get; set; } // [ADDED]
        
        public string ItemCode { get; set; }
        public int? PackageCount { get; set; }
        public string CodeType{ get; set; }
        
    }
}
