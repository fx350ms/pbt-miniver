using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.ApplicationUtils;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Warehouses.Dto;
using pbt.Waybills.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace pbt.Orders.Dto
{
    public class OrderDto : FullAuditedEntityDto<long>
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
        public decimal BubbleWrapFee { get; set; }  // Phí quấn bọt khí
        public decimal DomesticShipping { get; set; }  // Ship nội địa
        public decimal ECommerceShipping { get; set; }  // Vận chuyển Thương mại điện tử
        public decimal GoodsValue { get; set; }  // Giá trị hàng hóa
        public decimal TotalCost { get; set; }  // Tổng chi phí
        public decimal Paid { get; set; }  // Đã thanh toán
        public decimal AmountDue { get; set; }  // Cần thanh toán

        public int CNWarehouseId { get; set; }
        public string CNWarehouseName { get; set; }
        public int VNWarehouseId { get; set; }
        public string VNWarehouseName { get; set; }
        public int AddressId { get; set; }
        public string AddressName { get; set; }

        public int? PackageCount { get; set; }

        public int PaymentStatus { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal Dimension { get; set; }
        public decimal ServicesFee { get; set; }

      

        public long? ParentId { get; set; }

        public string Note { get; set; } // [ADDED]
        public DateTime? InTransitToChinaWarehouseTime { get; set; } // [ADDED]
        public DateTime? InTransitTime { get; set; } // [ADDED]
        public DateTime? InTransitToVietnamWarehouseTime { get; set; } // [ADDED]
        public DateTime? DeliveryTime { get; set; } // [ADDED]
        public DateTime? DeliveredTime { get; set; } // [ADDED]
        public DateTime? ComplaintTime { get; set; } // [ADDED]
        public DateTime? RefundTime { get; set; } // [ADDED]
        public DateTime? CancelledTime { get; set; } // [ADDED]
        public DateTime? OrderCompletedTime { get; set; } // [ADDED]
        public DateTime? TransferWarehouseTime { get; set; } // thời gian chuyển kho



        #region Additional Fees & Calculation Properties (Phí & Tính toán bổ sung)
        /// <summary>
        /// phí quấn bọt khí (nếu khác với BubbleWrapFee)
        /// </summary>
        public decimal? ShockproofFee { get; set; }

        /// <summary>
        /// Giá trị bảo hiểm
        /// </summary>
        public decimal? InsuranceValue { get; set; }

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        public decimal? InsuranceFee { get; set; }

        /// <summary>
        /// Id đóng gỗ chung
        /// </summary>
        public long WoodenPackingId { get; set; }

        /// <summary>
        /// Cờ sử dụng vận chuyển nội địa
        /// </summary>
        [DefaultValue(0)]
        public bool IsDomesticShipping { get; set; }

        /// <summary>
        /// Phí ship nội địa (tiền tệ Trung Quốc)
        /// </summary>
        [DefaultValue(0)]
        public decimal DomesticShippingFeeRMB { get; set; }

        public decimal DomesticShippingFee { get; set; }
        /// <summary>
        /// Tổng phí vận chuyển theo cân
        /// </summary>
        public decimal? TotalFee { get; set; }

        /// <summary>
        /// Tổng cân nặng kiện hàng theo đơn
        /// </summary>
        public decimal? WeightPackage { get; set; }

        /// <summary>
        /// Tổng cân nặng
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Tổng thể tích
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Trọng lượng bao bì
        /// </summary>
        public decimal? WeightCover { get; set; }

        /// <summary>
        /// Phí bao bì
        /// </summary>
        public decimal? WeightCoverFee { get; set; }

        /// <summary>
        /// Giá vốn
        /// </summary>
        public decimal? CostFee { get; set; }

        /// <summary>
        /// 1: KG, 2: M3
        /// </summary>
        public int UnitType { get; set; }

        /// <summary>
        /// Chi phí vận chuyển gốc
        /// </summary>
        public decimal? OriginShippingCost { get; set; }

        /// <summary>
        /// Phí đóng gỗ (Lưu ý: kiểu dữ liệu trong Entity là nullable)
        /// </summary>
        public decimal? WoodenPackagingFee { get; set; }


        // Các trường liên quan đến giá trị tiền tệ
        /// <summary>
        /// Giá trị hàng hóa tiền Việt (thiếu trong DTO)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Giá trị hàng hóa tiền tệ RMB (thiếu trong DTO)
        /// </summary>
        public decimal? PriceCN { get; set; }

        /// <summary>
        /// Tổng tất cả chi phí (thiếu trong DTO)
        /// </summary>
        public decimal? TotalPrice { get; set; }

        // Các cờ (Flags) dịch vụ gia tăng chi tiết hơn
        /// <summary>
        /// Bảo hiểm (cờ chi tiết hơn, DTO chỉ có UseInsurance)
        /// </summary>
        public bool IsInsured { get; set; }

        /// <summary>
        /// Đóng gỗ (cờ chi tiết hơn, DTO chỉ có UseWoodenPackaging)
        /// </summary>
        public bool IsWoodenCrate { get; set; }
 
        #endregion
    }
}
