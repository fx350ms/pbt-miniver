using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace pbt.Entities
{
    public class Order : FullAuditedEntity<long>
    {

        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillNumber { get; set; }


        //public long? WaybillId { get; set; }
        /// <summary>
        /// Mã khách hàng
        /// </summary>
        /// can be null
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
        /// 
        [DefaultValue(true)]
        public bool IsCustomerOrder { get; set; }

        /// <summary>
        /// Loại đơn
        /// 1. Đơn thường
        /// 2. Ký gửi
        /// </summary>
        /// 
        [DefaultValue(1)]
        public int OrderType { get; set; }


        /// <summary>
        /// Line vận chuyển: Chính ngạch, tiểu ngạch
        /// </summary>
        /// 
        [DefaultValue(0)]
        public int ShippingLine { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        [DefaultValue(0)]
        public int OrderStatus { get; set; }

        /// <summary>
        /// Trạng thái vận chuyển
        /// </summary>
        [DefaultValue(0)]
        public int ShippingStatus { get; set; }

        /// <summary>
        /// Chi phí thanh toán
        /// </summary>

        [DefaultValue(0)]
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Giá bán
        /// </summary>
        [DefaultValue(0)]
        public decimal SellingPrice { get; set; }

        /// <summary>
        /// Id Của nhân viên kinh doanh
        /// </summary>
        [DefaultValue(0)]
        public long UserSaleId { get; set; }

        // Trường UseInsurance giữ nguyên [DefaultValue(0)]
        [DefaultValue(0)]
        public bool UseInsurance { get; set; }

        // Trường UseWoodenPackaging giữ nguyên [DefaultValue(0)]
        [DefaultValue(0)]
        public bool UseWoodenPackaging { get; set; }

        // Trường UseShockproofPackaging giữ nguyên [DefaultValue(0)]
        public bool UseShockproofPackaging { get; set; }

        // Trường UseDomesticTransportation giữ nguyên [DefaultValue(0)]
        [DefaultValue(0)]
        public bool UseDomesticTransportation { get; set; }

        #region THAY ĐỔI VÀ BỔ SUNG TỪ CLASS MỚI

        /// <summary>
        /// Bảo hiểm
        /// </summary>
        // THAY ĐỔI: Bỏ [DefaultValue(0)]

        [DefaultValue(0)]
        public decimal Insurance { get; set; }

        // THAY ĐỔI: Bỏ [DefaultValue(0)]

        [DefaultValue(0)]
        public decimal PriceInsurance { get; set; }

        [DefaultValue(0)]
        public decimal OrderFee { get; set; }  // Phí đặt hàng

        // THAY ĐỔI: Từ decimal sang decimal? (Nullable) VÀ Bỏ [DefaultValue(0)]
        // Dù class mới có chú thích là "Phí đóng gỗ", trường này vẫn là thay đổi thuộc tính/kiểu dữ liệu của WoodenPackagingFee gốc
        [DefaultValue(0)]
        public decimal? WoodenPackagingFee { get; set; }  // Phí đóng gỗ

        // THAY ĐỔI: Bỏ [DefaultValue(0)]
        [DefaultValue(0)]
        public decimal BubbleWrapFee { get; set; }  // Phí quấn bọt khí

        // Trường DomesticShipping giữ nguyên [DefaultValue(0)]
        [DefaultValue(0)]
        public decimal DomesticShipping { get; set; }  // Ship nội địa (Xuất hiện 2 lần trong class mới nhưng ta giữ trường gốc và chỉ sửa đổi ở đây)

        [DefaultValue(0)]
        public decimal ECommerceShipping { get; set; }  // Vận chuyển Thương mại điện tử

        [DefaultValue(0)]
        [Description("Giá trị hàng hóa")]
        public decimal GoodsValue { get; set; }  // Giá trị hàng hóa

        [DefaultValue(0)]
        public decimal TotalCost { get; set; }  // Tổng chi phí

        [DefaultValue(0)]
        public decimal Paid { get; set; }  // Đã thanh toán

        [DefaultValue(0)]
        public decimal AmountDue { get; set; }  // Cần thanh toán

        [DefaultValue(0)]
        public int? PackageCount { get; set; } // Tổng kiện hàng

        [DefaultValue(0)]
        public int CNWarehouseId { get; set; }

        [DefaultValue(0)]
        public int VNWarehouseId { get; set; }

        [DefaultValue(0)]
        public long? AddressId { get; set; }

        [DefaultValue(0)]
        public int PaymentStatus { get; set; }

        [DefaultValue(0)]
        public decimal? UnitPrice { get; set; }

        public string Note { get; set; } // Ghi chú đơn hàng

        // BỔ SUNG CÁC TRƯỜNG MỚI (NON-TIME RELATED)

        /// <summary>
        /// Giá trị hàng hóa tiền việt
        /// </summary>
        [DefaultValue(0)]
        public decimal? Price { get; set; } // giá trị hàng hóa tiền việt

        [DefaultValue(0)]
        public decimal? PriceCN { get; set; } // giá trị hàng hóa tiền tệ

        [DefaultValue(0)]
        public decimal? TotalPrice { get; set; } // Tổng tất cả chi phí

        #region Shockproof Packaging - Quấn bọt khí (chống sốc)

        /// <summary>
        /// phí quấn bọt khí
        /// </summary>
        /// 
        [DefaultValue(0)]
        public decimal? ShockproofFee { get; set; } // phí quấn bọt khí
        #endregion

        #region Insurance - Bảo hiểm

        [DefaultValue(false)]
        public bool IsInsured { get; set; } //bảo hiểm

        [DefaultValue(0)]
        public decimal? InsuranceValue { get; set; } // giá trị bảo hiểm

        [DefaultValue(0)]
        public decimal? InsuranceFee { get; set; } // phí bảo hiểm
        #endregion

        #region Wooden Crate - Đóng gỗ

        [DefaultValue(false)]
        public bool IsWoodenCrate { get; set; } // đóng gỗ

        [DefaultValue(false)]
        public long WoodenPackingId { get; set; } // đóng gỗ chung id
        #endregion

        #region Domestic Shipping

        [DefaultValue(0)]
        public bool IsDomesticShipping { get; set; }
        [DefaultValue(0)]
        public decimal DomesticShippingFeeRMB { get; set; }
        [DefaultValue(0)]
        public decimal DomesticShippingFee { get; set; } // phí ship nội địa tiền việt
        #endregion

        /// <summary>
        /// Tổng phí vận chuyển theo cân hoặc thể tích
        /// </summary>
        /// 
        [DefaultValue(0)]
        public decimal? TotalFee { get; set; } // Tổng phí vận chuyển theo cân
        [DefaultValue(0)]
        public decimal? WeightPackage { get; set; } // Tổng cân nặng kiện hàng theo đơn
        [DefaultValue(0)]
        public decimal? Weight { get; set; } // Tổng cân nặng
        [DefaultValue(0)]
        public decimal? Volume { get; set; } // tổng thể tích

        [DefaultValue(0)]
        public decimal? WeightCover { get; set; } // Trọng lượng bao bì có được tính vào trọng lượng của kiện hàng hay không
        [DefaultValue(0)]
        public decimal? WeightCoverFee { get; set; } // Phí bao bì
        [DefaultValue(0)]
        public decimal? CostFee { get; set; } // Giá vốn 
        [DefaultValue(0)]
        public int UnitType { get; set; } // 1: KG, 2: M3
        public decimal? OriginShippingCost { get; set; }

        #endregion
        [CanBeNull] public Customer Customer { get; set; }
        /// <summary>
        /// Các trường thời gian tương ứng với trạng thái của đơn hàng
        /// </summary>

        public long? ParentId { get; set; } // Id của đơn hàng gốc nếu là đơn hàng con

        public DateTime? InTransitToChinaWarehouseTime { get; set; } // Thời gian hàng về kho TQ
        public DateTime? InTransitTime { get; set; } // Thời gian đang vận chuyển quốc tế
        public DateTime? InTransitToVietnamWarehouseTime { get; set; } // Thời gian hàng đến kho VN
        public DateTime? DeliveryTime { get; set; } // Thời gian giao hàng (theo phiếu xuất kho ra khỏi kho)
        public DateTime? DeliveredTime { get; set; } // Thời gian giao hàng thành công
        public DateTime? ComplaintTime { get; set; } // Thời gian khiếu nại
        public DateTime? RefundTime { get; set; } // Thời gian hoàn tiền
        public DateTime? CancelledTime { get; set; } // Thời gian đơn hàng bị hủy
        public DateTime? OrderCompletedTime { get; set; } // Thời gian hoàn thành đơn hàng
        public DateTime? TransferWarehouseTime { get; set; } // thời gian chuyển kho
    }
}
