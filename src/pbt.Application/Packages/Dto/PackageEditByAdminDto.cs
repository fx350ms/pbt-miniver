using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using pbt.Bags.Dto;
using pbt.Orders.Dto;
using pbt.Waybills.Dto;
using System;

namespace pbt.Packages.Dto
{
    public class PackageEditByAdminDto : EntityDto<int>
    {
        public string PackageNumber { get; set; }
        public string TrackingNumber { get; set; }
        public long? CustomerId { get; set; } // mã khách hàng
        public string ProductNameVi { get; set; }
        public string ProductNameCn { get; set; }
        public int? ShippingPartnerId { get; set; }
        public int? ShippingLineId { get; set; }

        public int? ProductGroupTypeId { get; set; }
        public decimal? Price { get; set; }
        public decimal? PriceCN { get; set; }
        public decimal? TotalFee { get; set; }
        public int? Length { get; set; } // cm
        public int? Width { get; set; } // cm
        public int? Height { get; set; } // cm
        public int? Quantity { get; set; } // số lượng
        public decimal? Weight { get; set; } // kg
        public decimal? Volume { get; set; } // thể tích
        public bool IsInsured { get; set; }
        public bool IsWoodenCrate { get; set; }
        public int? WoodenCrateType { get; set; }

        public long? SharedCrateSelectId { get; set; }
        public bool IsShockproof { get; set; }
        public bool IsDomesticShipping { get; set; }

        public DateTime? MatchTime { get; set; } // thời gian khớp
        public DateTime? DeliveryTime { get; set; } // thời gian xuất kho
        public string DeliveryTimeStr => DeliveryTime.HasValue ? DeliveryTime.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";

        public DateTime? ExportDate { get; set; } // thời gian xuất kho ở TQ
        public string ExportDateStr => ExportDate.HasValue ? ExportDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";

        public DateTime? ImportDate { get; set; } // thời gian nhập kho ở VN
        public string ImportDateStr => ImportDate.HasValue ? ImportDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "";

        public int WarehouseStatus { get; set; } // trạng thái kho
        public int ShippingStatus { get; set; } // trạng thái vận chuyên
        /// <summary>
        /// Kho
        /// </summary>
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Giá trị bảo hiểm
        /// </summary>
        public decimal? InsuranceValue { get; set; }
        public string Note { get; set; }
        public decimal? TotalPrice { get; set; } // Tổng
        public decimal? ShockproofFee { get; set; } // phí quấn bọt khí
        public decimal? InsuranceFee { get; set; } // phí bảo hiểm
        public decimal? WoodenPackagingFee { get; set; } // phí đóng gỗ
        public decimal? DomesticShippingFee { get; set; }
        public decimal? DomesticShippingFeeRMB { get; set; }
 
    }
}
