using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using pbt.ApplicationUtils;
using pbt.Bags.Dto;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Orders.Dto;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;
using pbt.Waybills.Dto;

namespace pbt.Packages.Dto
{
    public class PackageFinanceDto : FullAuditedEntityDto<int>
    {
        public string ProductNameVi { get; set; }

        public string ProductNameCn { get; set; }

        public string TrackingNumber { get; set; }
        public long OrderId { get; set; }
        public string PackageNumber { get; set; }

        public string BagNumber { get; set; }

        public int? ShippingPartnerId { get; set; }
        public int? ShippingLineId { get; set; }

        public string ShippingLineString => ((LineShipping)ShippingLineId).GetDescription();

        public int? ProductGroupTypeId { get; set; }

        public int? CategoryId { get; set; }

        #region Thông tin kiện

        public int? Quantity { get; set; }

        public int? Length { get; set; } // cm
        public string LengthStr => Length.HasValue ? Length.Value.ToCustomNumberFormat() : "0";
        public int? Width { get; set; } // cm
        public string WidthStr => Width.HasValue ? Width.Value.ToCustomNumberFormat() : "0";
        public int? Height { get; set; }
        public string HeightStr => Height.HasValue ? Height.Value.ToCustomNumberFormat() : "0";
        public decimal? Volume => (decimal?)(Length * Width * Height) / 1000000;
        public string VolumeStr => Volume.HasValue ? Volume.Value.ToCustomNumberFormat() : "0";
        public decimal? Weight { get; set; } // kg
        public string WeightStr => Weight.HasValue ? Weight.Value.ToCustomNumberFormat() : "0";


        /// <summary>
        /// Kho hiện tại
        /// </summary>
        public int? WarehouseId { get; set; }
        public int WarehouseStatus { get; set; } // trạng thái kho
        public string WarehouseStatusName => ((WarehouseStatus)WarehouseStatus).GetDescription(); // trạng thái kho
        public int ShippingStatus { get; set; } // trạng thái vận chuyên
        public string ShippingStatusName => ((PackageDeliveryStatusEnum)ShippingStatus).GetDescription(); // trạng thái vận chuyên

        public string Note { get; set; }

        public int? BagId { get; set; }

        public string ShippingPartnerName { get; set; }

        public string OrderCode { get; set; }

        public virtual string WaybillCodeFake { get; set; }
        public long? ParentId { get; set; }

        public long? costPerKg { get; set; } // giá trên kg
        public long? CustomerId { get; set; }

        public string DeliveryNoteCode { get; set; } // mã phiếu xuất kho

        public bool? IsQuickBagging { get; set; }

        public decimal? OriginShippingCost { get; set; } // 1: KG, 2: M3

        public string WarehouseName { get; set; }


        /// <summary>
        /// Đại diện cho bao để hiển thị cân nặng bì của bao
        /// </summary>
        [DefaultValue(false)]
        public bool IsRepresentForWeightCover { get; set; }

        public int? WarehouseCreateId { get; set; } // Kho tạo
        public string WarehouseCreateName { get; set; } // Tên kho tạo

        public int? WarehouseDestinationId { get; set; } // Kho đích
        public string WarehouseDestinationName { get; set; } // Tên kho đích
        #endregion


        #region Thông tin tài chính


        public decimal? Price { get; set; }
        public bool IsPriceUpdate { get; set; } = false;
        public string WeightUpdateReason { get; set; }
        public decimal? PriceCN { get; set; }

        public string PriceStr => Price.HasValue ? Price.Value.ToCustomNumberFormat() : "0";

        public decimal? TotalFee { get; set; }
        public string TotalFeeStr => TotalFee.HasValue ? TotalFee.Value.ToCustomNumberFormat() : "0";

        public string TotalServiceFeeStr
        {
            get
            {
                decimal total = 0m;
                if (WoodenPackagingFee.HasValue) total += WoodenPackagingFee.Value;
                if (ShockproofFee.HasValue) total += ShockproofFee.Value;
                if (DomesticShippingFee.HasValue) total += DomesticShippingFee.Value;
                if (InsuranceFee.HasValue) total += InsuranceFee.Value;
                return total.ToCustomNumberFormat();
            }
        }

        public decimal RMB { get; set; } // Giá trị RMB

        public bool IsInsured { get; set; } 
        public bool IsWoodenCrate { get; set; } 
        public long WoodenPackingId { get; set; }   // nhóm đóng gỗ chung id
        public bool IsShockproof { get; set; } 
        public bool IsDomesticShipping { get; set; } 

        /// <summary>
        /// Giá vận chuyển nội địa 
        /// </summary>
        public decimal? DomesticShippingFee { get; set; }
        public string DomesticShippingFeeStr => DomesticShippingFee.HasValue ? DomesticShippingFee.Value.ToCustomNumberFormat() : "0";

        public decimal? DomesticShippingFeeRMB { get; set; }
        public string DomesticShippingFeeRMBStr => DomesticShippingFeeRMB.HasValue ? DomesticShippingFeeRMB.Value.ToCustomNumberFormat() : "0";

        /// <summary>
        /// Giá trị bảo hiểm
        /// </summary>
        public decimal InsuranceValue { get; set; }
        public string InsuranceValueStr => InsuranceValue.ToCustomNumberFormat();

        /// <summary>
        /// Tổng giá trị hàng hóa, bao gồm cả phí vận chuyển, bảo hiểm, đóng gói, v.v.
        /// </summary>
        public decimal? TotalPrice { get; set; } // Tổng tất cả chi phí
        public string TotalPriceStr => TotalPrice.HasValue ? TotalPrice.Value.ToCustomNumberFormat() : "0";
        public decimal? ShockproofFee { get; set; } // phí quấn bọt khí

        public string ShockproofFeeStr => ShockproofFee.HasValue ? ShockproofFee.Value.ToCustomNumberFormat() : "0";

        public decimal? InsuranceFee { get; set; } // phí bảo hiểm
        public string InsuranceFeeStr => InsuranceFee.HasValue ? InsuranceFee.Value.ToCustomNumberFormat() : "0";
        public decimal? WoodenPackagingFee { get; set; } // phí đóng gỗ
        public string WoodenPackagingFeeStr => WoodenPackagingFee.HasValue ? WoodenPackagingFee.Value.ToCustomNumberFormat() : "0";


        public decimal? CostFee { get; set; } // Giá vốn 
        public int UnitType { get; set; } // 1: KG, 2: M3

        #endregion


        #region timeline


        public DateTime? MatchTime { get; set; } // thời gian khớp
        public DateTime? DeliveryTime { get; set; } // thời gian xuất kho
        public DateTime? ExportDate { get; set; } // thời gian xuất kho ở TQ
        public DateTime? ImportDate { get; set; } // thời gian nhập kho ở VN
        public DateTime? BaggingDate { get; set; } // Thời gian được đưa vào bao
        public string DeliveryTimeFormat
        {
            get { return DeliveryTime is not null ? DeliveryTime?.ToString("dd/MM/yyyy HH:mm") : null; }
        } // thời gian xuất kho

        public DateTime CreationTime { get; set; } // thời gian tạo

        public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo
        public string MatchTimeFormat
        {
            get { return MatchTime is not null ? MatchTime?.ToString("dd/MM/yyyy HH:mm") : null; }
        } // thời gian xuất kho

        #endregion
    }
}