namespace pbt.ShippingRates.Dto;

/// <summary>
/// Represents the result of a shipping cost calculation.
/// </summary>
public class ShippingCostResult
{
    public int ShippingRateGroupId { get; set; }
    public int ShippingRateId { get; set; }
    public decimal ShippingFee { get; set; } // Phí vận chuyển
    public decimal PricePerUnit { get; set; } // giá vận chuyển trên 1 đơn vị
    public int UnitType { get; set; } // tính theo trọng lượng hay khối lươgnj
    public decimal Rs { get; set; } // tỷ giá
    public decimal RMB { get; set; } // tỷ giá

    public decimal InsuranceValue { get; set; } // Giá trị bảo hiểm theo customer
    public decimal InsuranceFee { get; set; } // bảo hiểm
    public decimal PackagePrice { get; set; } // tổng tất cả các loại phí
    public decimal WoodenFee { get; set; } // phí đóng gỗ
    public decimal ShockproofFee { get; set; } // phí quấn bọt
    public decimal DomesticShippingFee { get; set; } // phí vận chuyển nội địa tiền VND
    public decimal DomesticShippingFeeCN { get; set; } // phí vận chuyển nội địa tiền trung
    public decimal PricePerUnitM3 { get; set; }
    public decimal PricePerUnitKG { get; set; }

    public decimal TotalServiceFee { get; set; } // tổng phí dịch vụ
    
}