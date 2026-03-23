using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace pbt.Entities;

public class Bag : FullAuditedEntity<int>
{
    public string BagCode { get; set; }
    public int BagType { get; set; }
    public long? CustomerId { get; set; }
    public bool IsSolution { get; set; } // Dung dịch
    public bool IsWoodSealing { get; set; } // Đóng gỗ
    public bool IsFakeGoods { get; set; } // Hàng giả
    public bool IsOtherFeature { get; set; } // Khác
    public int? WarehouseCreateId { get; set; }
    public int? WarehouseDestinationId { get; set; }
    public int? CurrentWarehouseId { get; set; }
    public string Receiver { get; set; }
    [CanBeNull] public string Note { get; set; }
    public int? WarehouseStatus { get; set; }
    /// <summary>
    /// Trọng lượng thực tế của bao ( cân nặng bì + cân nặng kiện hàng trong bao)
    /// </summary>
    public decimal? Weight { get; set; } // Trọng lượng thực tế của bao ( cân nặng bì + cân nặng kiện hàng trong bao)
    public decimal? Volume { get; set; } // Thể tích bao
    public bool IsClosed { get; set; } = false;
    public int ShippingPartnerId { get; set; }
    public int ShippingType { get; set; }
    public int ShippingStatus { get; set; }

    public bool IsWeightCover { get; set; } // Có tính bì hay không?
    public decimal? WeightCover { get; set; } // Trọng lượng bao bì có được tính vào trọng lượng của kiện hàng hay không
    public decimal? WeightCoverFee { get; set; } // Phí bao bì

    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    /// <summary>
    /// Tổng cân nặng kiện hàng trong bao
    /// </summary>
    public decimal? WeightPackage { get; set; } // Tổng cân nặng kiện hàng trong bao
    public int? BagSize { get; set; }
    [CanBeNull] public string otherReason { get; set; }

    [ForeignKey("CustomerId")][CanBeNull] public virtual Customer Customer { get; set; }
    [ForeignKey("WarehouseCreateId")][CanBeNull] public virtual Warehouse WarehouseCreate { get; set; }
    [ForeignKey("WarehouseDestinationId")]
    [CanBeNull] public virtual Warehouse WarehouseDestination { get; set; }
  

    [CanBeNull] public virtual List<Package> Packages { get; set; }
    [ForeignKey("ShippingPartnerId")]
    public virtual ShippingPartner ShippingPartner { get; set; }
    public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo

    public DateTime? ExportDate { get; set; } // thời gian xuất kho
    public DateTime? ExportDateVn { get; set; } // thời gian xuất kho VN
    public DateTime? ImportDate { get; set; } // thời gian nhập kho
    public DateTime? DeliveryDate { get; set; } // thời gian giao hàng
    public DateTime? TransferWarehouseTime { get; set; } // thời gian chuyển kho

    public int? DeliveryRequestId { get; set; }
    public int? DeliveryNoteId { get; set; }
    [ForeignKey("DeliveryNoteId")][CanBeNull] public virtual DeliveryNote DeliveryNote { get; set; } // Phiếu xuất kho
    [DefaultValue(false)]
    public bool IsRepresentForDeliveryNote { get; set; } // đại diện cho phiếu xuất kho
    public decimal? ShippingFee { get; set; } // Tổng Phí vận chuyển quốc tế
    public decimal? DomesticShippingFee { get; set; } // Tổng Phí vận chuyển nội địa tại TQ
    public decimal? ShockproofFee { get; set; } // phí quấn bọt khí
    public decimal? InsuranceFee { get; set; } // phí bảo hiểm
    public decimal? WoodenPackagingFee { get; set; } // phí đóng gỗ

    /// <summary>
    /// Tổng phí vận chuyển của các kiện trong bao
    /// </summary>
    public decimal? TotalPackageFee { get; set; } // Tổng phí vận chuyển của các kiện trong bao

    /// <summary>
    /// Tổng phí vận chuyển của bao (tổng phí kiện + phí bao bì + các phí khác)
    /// </summary>
    public decimal? TotalFee { get; set; } // Tổng phí vận chuyển của bao (tổng phí kiện + phí bao bì + các phí khác)

    /// <summary>
    /// Tổng số kiện trong bao
    /// </summary>
    public int? TotalPackages { get; set; } // Tổng số kiện trong bao

    /// <summary>
    /// Tổng cân nặng của các kiện trong bao
    /// </summary>
    public decimal? TotalWeightPackage { get; set; }

    public decimal? TotalOriginShippingCost { get; set; } // Tổng phí vận chuyển gốc của bao (tổng phí kiện + phí bao bì + các phí khác)
    public decimal? TotalOriginPackageShippingCost { get; set; } // Tổng phí vận chuyển gốc của kien 

}