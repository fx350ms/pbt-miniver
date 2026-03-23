using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.ApplicationUtils;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Packages.Dto;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Bags.Dto
{
    public class BagStampDto : EntityDto<int>
    {
        [CanBeNull] public string BagCode { get; set; }
        public int BagType { get; set; }

        public string BagTypeName
        {
            get
            {
                var type = (BagTypeEnum)BagType;
                return type.GetDescription();
            }
        }

        public string CustomerCodeName
        {
            get { return Customer != null ? Customer.FullName + '/' + Customer.CustomerCode : null; }
        }

        public string CustomerName
        {
            get { return Customer != null ? Customer.FullName : null; }
        }

        public string WarehouseCreateName
        {
            get { return WarehouseCreate != null ? WarehouseCreate.Name : null; }
        }

        public string WarehouseDestinationName
        {
            get { return WarehouseDestination != null ? WarehouseDestination.Name : null; }
        }

        public int? CustomerId { get; set; }
        public bool IsSolution { get; set; } // Dung dịch
        public bool IsWoodSealing { get; set; } // Đóng gỗ
        public bool IsFakeGoods { get; set; } // Hàng giả
        public bool IsOtherFeature { get; set; } // Khác
        public int? WarehouseCreateId { get; set; }
        public int? WarehouseDestinationId { get; set; }
        public string Receiver { get; set; }
        [CanBeNull] public string Note { get; set; }
        public bool IsClosed { get; set; }
        public int ShippingPartnerId { get; set; }
        public string? ShippingPartnerName { get; set; }
        public int ShippingType { get; set; }
        public string? ShippingTypeName { get; set; }
        public int? WarehouseStatus { get; set; }
        public decimal? Weight { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public int? BagSize { get; set; }

        // Tổng cân của các kiện
        public decimal? WeightPackages { get; set; }
        public int? TotalPackages { get; set; }

        public decimal? Volume { get; set; }

        public decimal? VolumeStr
        {
            get
            {
                return (Length ?? 0) * (Width ?? 0) * (Height ?? 0) / 1000000;
            }
        }
        public int? DeliveryNoteId { get; set; }
        public DateTime CreationTime { get; set; } // thời gian tạo
        public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo
        public bool IsWeightCover { get; set; }
        public int ShippingStatus { get; set; }
        public string otherReason { get; set; }

        public string ShippingStatusString
        {
            get { return ((BagShippingStatus)ShippingStatus).GetDescription(); }
        }

        public CustomerDto Customer { get; set; }
        public WarehouseDto WarehouseCreate { get; set; }
        public WarehouseDto WarehouseDestination { get; set; }
        public ShippingPartnerDto ShippingPartner { get; set; }
        public List<PackageDto> PackagesDtos { get; set; }
    }
}