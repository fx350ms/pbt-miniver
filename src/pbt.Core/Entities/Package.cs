using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace pbt.Entities;

public class Package : FullAuditedEntity<int>
{
    
    public string ProductNameVi { get; set; }
    
    public string ProductNameCn { get; set; }

    public string TrackingNumber { get; set; }

    // đơn hàng id
    public long? OrderId { get; set; }

    // mã vận đơn id
    public long? WaybillId { get; set; }
    public string PackageNumber { get; set; }
    public string BagNumber { get; set; }
    public int? ShippingPartnerId { get; set; }
    public int? ShippingLineId { get; set; }
    public int? ProductGroupTypeId { get; set; }
    public int? CategoryId { get; set; }
    /// <summary>
    /// Số lượng
    /// </summary>
    public int? Quantity { get; set; } // số lượng

    /// <summary>
    /// Giá trị hàng hóa tiền việt
    /// </summary>
    public decimal? Price { get; set; } // giá trị hàng hóa tiền việt


    public bool IsPriceUpdate { get; set; } = false;

    /// <summary>
    /// Lý do thay đổi cân nặng
    /// </summary>
    public string WeightUpdateReason { get; set; }

    /// <summary>
    /// Giá trị hàng hóa tiền tệ (RMB)
    /// </summary>
    public decimal? PriceCN { get; set; } // giá trị hàng hóa tiền tệ

    /// <summary>
    /// Tổng giá trị hàng hóa, bao gồm cả phí vận chuyển, bảo hiểm, đóng gói, v.v.
    /// </summary>
    public decimal? TotalPrice { get; set; } // Tổng tất cả chi phí

    /// <summary>
    /// phí quấn bọt khí
    /// </summary>
    public decimal? ShockproofFee { get; set; } // phí quấn bọt khí
    /// <summary>
    ///  phí bảo hiểm
    /// </summary>
    public decimal? InsuranceFee { get; set; } // phí bảo hiểm

    /// <summary>
    /// Phí đóng gỗ
    /// </summary>
    public decimal? WoodenPackagingFee { get; set; } // phí đóng gỗ

    /// <summary>
    /// Tổng phí vận chuyển theo cân hoặc thể tích
    /// </summary>
    public decimal? TotalFee { get; set; } // Tổng phí vận chuyển theo cân
    public int? Length { get; set; } // cm
    public int? Width { get; set; } // cm
    public int? Height { get; set; } // cm
    public decimal? Weight { get; set; } // kg
    public string ProductLink { get; set; }
    public string Description { get; set; }
    [DefaultValue(false)]
    public bool IsInsured { get; set; } //bảo hiểm
    [DefaultValue(false)]
    public bool IsWoodenCrate { get; set; } // đóng gỗ
    
    public long? WoodenPackingId { get; set; } // đóng gỗ chung id
    [DefaultValue(false)]
    public bool IsShockproof { get; set; } // quấn bọt khí
    
    public bool? IsDomesticShipping { get; set; }
    [DefaultValue(0)]
    public decimal DomesticShippingFeeRMB { get; set; }
    [DefaultValue(0)]
    public decimal DomesticShippingFee { get; set; } // phí ship nội địa tiền việt
    public DateTime? MatchTime { get; set; } // thời gian khớp
    public DateTime? ExportDate { get; set; } // thời gian xuất kho ở TQ
    public DateTime? ImportDate { get; set; } // thời gian nhập kho ở VN
    public DateTime? DeliveryTime { get; set; } // thời gian vận chuyển, xuất kho ở VN
    public DateTime? BaggingDate { get; set; } // Thời gian được đưa vào bao

    /// <summary>
    /// Kho hiện tại của kiện hàng
    /// </summary>
    public int? WarehouseId { get; set; } // Kho hiện tại
    [DefaultValue(0)]
    public int WarehouseStatus { get; set; } // trạng thái kho
    [DefaultValue(0)]
    public int ShippingStatus { get; set; } // trạng thái vận chuyên
    public int? BagId { get; set; }
    public int? LastBagId { get; set; } // bao cuối cùng kiện nằm trong đó
    
    [DefaultValue(false)]
    public bool IsDefective { get; set; } // kiện lỗi 
    public decimal? InsuranceValue { get; set; } // giá trị bảo hiểm
    public int? DeliveryRequestId { get; set; }
    public int? DeliveryNoteId { get; set; } // phiếu xuất kho
    [ForeignKey("DeliveryNoteId")][CanBeNull] public virtual DeliveryNote DeliveryNote { get; set; }

    [DefaultValue(false)]
    public bool IsRepresentForDeliveryNote { get; set; } // kiện đại diện cho phiếu xuất kho

    public string Note { get; set; }

    [ForeignKey("BagId")]
    public virtual Bag Bag { get; set; }

    public long? ParentId { get; set; } // không dùng
    public long? costPerKg { get; set; }  // không dùng
    public long? CustomerId { get; set; }
    public long? CustomerFakeId { get; set; }
    public decimal? UnitPrice { get; set; }  // giá vận chuyển trên 1 kg
    [CanBeNull] public virtual Customer Customer { get; set; }
    [ForeignKey("OrderId")][CanBeNull] public virtual Order Order { get; set; }
    
    [ForeignKey("ShippingPartnerId")][CanBeNull] public virtual ShippingPartner ShippingPartner { get; set; }
    [ForeignKey("WarehouseId")][CanBeNull] public virtual Warehouse Warehouse { get; set; }
    
    //bool IsQuickBagging set default = true
    [DefaultValue(true)]
    public bool IsQuickBagging { get; set; }

    public DateTime? LostTime { get; set; } // Thời gian kiện bị thất lạc
    public DateTime? InWarehouseTime { get; set; } // Thời gian kiện về kho
    public DateTime? WaitingForShippingTime { get; set; } // Thời gian chờ vận chuyển
    public DateTime? ShippingTime { get; set; } // Thời gian đang vận chuyển
    public DateTime? WaitingForDeliveryTime { get; set; } // Thời gian chờ giao
    public DateTime? DeliveryRequestTime { get; set; } // Thời gian yêu cầu giao
    public DateTime? DeliveryInProgressTime { get; set; } // Thời gian đang giao
    public DateTime? ReceivedTime { get; set; } // Thời gian kiện được nhận
    public DateTime? CompletedTime { get; set; } // Thời gian hoàn thành
    public DateTime? WaitingForReturnTime { get; set; } // Thời gian chờ trả hàng
    public DateTime? ReturningTime { get; set; } // Thời gian đang trả hàng
    public DateTime? ReturnedTime { get; set; } // Thời gian đã trả hàng
    public DateTime? CustomerNotClaimingTime { get; set; } // Thời gian khách không nhận
    public DateTime? WaitingForClearanceTime { get; set; } // Thời gian chờ thanh lý
    public DateTime? ClearanceTime { get; set; } // Thời gian đã thanh lý
    public DateTime? TransferWarehouseTime { get; set; } // Thời gian chuyển kho
    public DateTime? LastUnbagTime { get; set; } // Thời gian bỏ bao lần cuối

    [DefaultValue(false)]
    public bool IsRepresentForWeightCover { get; set; }
    
    public int? WarehouseCreateId { get; set; } // Kho tạo
    public int? WarehouseDestinationId { get; set; } // Kho đích

    /// <summary>
    /// Giá vốn được nhập hoặc tính theo đối tác vận chuyển
    /// </summary>
    public decimal? CostFee { get; set; } // Giá vốn 
    public int UnitType { get; set; } // 1: KG, 2: M3
    public decimal? OriginShippingCost { get; set; }  

    /// <summary>
    /// Mã vận đơn
    /// </summary>
    public string WaybillNumber { get; set; }
}