using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using pbt.ApplicationUtils;
using pbt.Bags.Dto;
using pbt.Entities;
using pbt.Orders.Dto;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;
using pbt.Waybills.Dto;

namespace pbt.Packages.Dto
{
    public class EditPackageDto : FullAuditedEntityDto<int>
    {
        public string PackageNumber { get; set; }
        public string TrackingNumber { get; set; }
        public long? CustomerId { get; set; } // mã khách hàng
        public int? BagId { get; set; } // ID bao
        public int? AddressId { get; set; } // mã khách hàng
        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Tên sản phẩm tiếng Việt là bắt buộc.")]
        public string ProductNameVi { get; set; }
        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Tên sản phẩm tiếng Trung là bắt buộc.")]
        public string ProductNameCn { get; set; }

        public long OrderId { get; set; }

        public int? ShippingPartnerId { get; set; }
        public int? ShippingLineId { get; set; }
        public int? CategoryId { get; set; }
        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Số lượng là bắt buộc.")]
        public int? Quantity { get; set; }
        public int? ProductGroupTypeId { get; set; }
        public decimal? Price { get; set; }
        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Giá là bắt buộc.")]
        public decimal? PriceCN { get; set; }

        public string PriceStr => Price.HasValue ? Price.Value.ToCurrencyFormat() : "";

        public decimal? TotalFee { get; set; }
        public string TotalFeeStr => TotalFee.HasValue ? TotalFee.Value.ToCurrencyFormat() : "";

        public int? Length { get; set; } // cm
        public string LengthString => Length.HasValue ? Length.Value.ToThousandFormat() : string.Empty;
        public int? Width { get; set; } // cm
        public string WidthString => Width.HasValue ? Width.Value.ToThousandFormat() : string.Empty;
        public int? Height { get; set; } // cm
        string HeightString => Height.HasValue ? Height.Value.ToThousandFormat() : string.Empty;
        public int? Dimention
        {
            get { return Length * Width * Height; }
        }

        public decimal? Weight { get; set; } // kg
        public string WeightString => Weight.HasValue ? Weight.Value.ToString("0.##") : string.Empty;
        public decimal? Volume { get; set; } // thể tích
        public string VolumeString => Volume.HasValue ? Volume.Value.ToThousandFormat() : string.Empty;
        public string ProductLink { get; set; }
        public string Description { get; set; }
        public bool IsInsured { get; set; }
        public bool IsWoodenCrate { get; set; }

        public int? WoodenCrateType { get; set; }

        public long? SharedCrateSelectId { get; set; }
        public bool IsShockproof { get; set; }
        public bool IsDomesticShipping { get; set; }
        public DateTime? MatchTime { get; set; } // thời gian khớp

        public string MatchTimeFormat
        {
            get { return MatchTime is not null ? MatchTime?.ToString("dd/MM/yyyy HH:mm") : null; }
        } // thời gian xuất kho

        public DateTime? DeliveryTime { get; set; } // thời gian xuất kho

        public string DeliveryTimeFormat
        {
            get { return DeliveryTime is not null ? DeliveryTime?.ToString("dd/MM/yyyy HH:mm") : null; }
        } // thời gian xuất kho

        public int WarehouseStatus { get; set; } // trạng thái kho

        public string WarehouseStatusName
        {
            get
            {
                var status = (WarehouseStatus)WarehouseStatus;
                return status.GetDescription();
            }
        } // trạng thái kho

        public int ShippingStatus { get; set; } // trạng thái vận chuyên

        public string ShippingStatusName
        {
            get
            {
                var status = (PackageDeliveryStatusEnum)ShippingStatus;
                return status.GetDescription();
            }
        } // trạng thái vận chuyên

        public DateTime CreationTime { get; set; } // thời gian tạo
        public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo

        /// <summary>
        /// Kho
        /// </summary>
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Kiện lỗi
        /// </summary>
        public bool IsDefective { get; set; }
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
        
        public virtual BagDto Bag { get; set; }
        public virtual OrderDto Order { get; set; }
        public virtual WaybillDto Waybill { get; set; }

        public string OrderCode { get; set; }

        public virtual string WaybillCodeFake { get; set; }
        public long? ParentId { get; set; }

    }
}