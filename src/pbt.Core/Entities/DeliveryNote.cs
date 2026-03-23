using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace pbt.Entities;

public class DeliveryNote :  FullAuditedEntity<int>
{
    public string DeliveryNoteCode { get; set; }
    public int Status { get; set; }
    
    public long? CustomerId { get; set; }
    public string Receiver { get; set; }

    /// <summary>
    /// Phí vận chuyển quốc tế
    /// </summary>
    public decimal? ShippingFee { get; set; }

    /// <summary>
    /// Phí vận chuyển nội địa (tại Việt Nam)
    /// </summary>
    public decimal? DeliveryFee { get; set; }
    public decimal? FinancialNegativePart { get; set; } /// Phần âm tài chính
    public decimal? BalanceBefore { get; set; } /// Số dư trước khi xuất kho
    public decimal? BalanceAfter { get; set; } /// Số dư sau khi xuất kho   
    public decimal? Cod { get; set; }
    public int? Length { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    [CanBeNull] public string Note { get; set; }
    public string RecipientPhoneNumber { get; set; }
    public string RecipientAddress { get; set; }
    public long? ExporterId { get; set; }
    public DateTime? ExportTime { get; set; }
    public int? ExportWarehouse { get; set; }

    public int? ShippingPartnerId { get; set; }
    public int? DeliveryFeeReason { get; set; }
    public decimal? TotalWeight { get; set; } // Tổng cân nặng
    public decimal? TotalVolume { get; set; } // Tổng thể tích
    public decimal? TotalFee { get; set; } // Tổng tất cả phí
    public int? WarehouseCreateId { get; set; }
    [CanBeNull] virtual public Customer Customer { get; set; }

    [CanBeNull]
    [ForeignKey("ShippingPartnerId")]
    public virtual ShippingPartner ShippingPartner { get; set; }
}