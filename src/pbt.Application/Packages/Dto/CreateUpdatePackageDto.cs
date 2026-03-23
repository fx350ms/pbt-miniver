using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Abp.Runtime.Validation;
using pbt.Entities;
using pbt.Warehouses.Dto;

namespace pbt.Packages.Dto
{
    public class ListCreateUpdatePackageInput : EntityDto<int>
    {
        public List<CreateUpdatePackageDto> Packages { get; set; }
        public int? NumberPackage { get; set; }
        public string? parentTrackingNumber { get; set; }
        public long? CustomerId { get; set; }
        public int ShippingLineId { get; set; }
    }

    public class CreateMultiPackage
    {
        public CreateUpdatePackageDto OriginPackage { get; set; }
        [CanBeNull] public List<CreateUpdatePackageDto> FakePackage { get; set; }
    }

    public class CreateUpdatePackageDto : ICustomValidate
    {
        [CanBeNull]
        public string ProductNameVi { get; set; }
        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Tên sản phẩm tiếng Trung là bắt buộc.")]
        public string ProductNameCn { get; set; }
        [CanBeNull] public string TrackingNumber { get; set; }
        // [Required(ErrorMessage = "Mã đơn hàng là bắt buộc.")]
        // public int? ShippingPartnerId { get; set; }
        [Required(ErrorMessage = "Tuyến vận chuyển là bắt buộc.")]
        public int? ShippingLineId { get; set; }
        public int? ProductGroupTypeId { get; set; }
        public int? CategoryId { get; set; }
        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Số lượng là bắt buộc.")]
        public int? Quantity { get; set; }

        [RequiredIfShippingLineId("ShippingLineId", [2], ErrorMessage = "Giá sản phẩm là bắt buộc.")]

        public decimal? Price { get; set; }

        public int? Height { get; set; } // cm
        public int? Length { get; set; } // cm
        public int? Width { get; set; } // cm
        [Required(ErrorMessage = "Cân nặng là bắt buộc.")] 
        public decimal Weight { get; set; } // kg
        public decimal Volume { get; set; } // thể tích
        [CanBeNull] public string ProductLink { get; set; }
        [CanBeNull] public string Description { get; set; }
        public bool IsInsured { get; set; }
        public decimal? InsuranceValue { get; set; }
        public bool IsWoodenCrate { get; set; }
        
        public int? WoodenCrateType { get; set; }
        public List<string>? WoodenPacking { get; set; }
        public bool IsShockproof { get; set; }
        public bool IsDomesticShipping { get; set; }
        [DefaultValue(0)]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal DomesticShippingFee { get; set; }
        public DateTime MatchTime { get; set; } // thời gian khớp
        public DateTime DeliveryTime { get; set; } // thời gian xuất kho
        public int WarehouseStatus { get; set; } // trạng thái kho
        public int ShippingStatus { get; set; } // trạng thái vận chuyên
        public DateTime CreationTime { get; set; } // thời gian tạo
        public int? WarehouseId { get; set; } // Kho
        public long? Parrent { get; set; } // package id origin
        public long? costPerKg { get; set; }  // giá trên kg
        public long? CustomerId { get; set; }

        [CanBeNull] public string Base64Image { get; set; }

        // [CanBeNull] public List<CreateUpdatePackageDto> children { get; set; }
        public void AddValidationErrors(CustomValidationContext context)
        {
        }
    }
}