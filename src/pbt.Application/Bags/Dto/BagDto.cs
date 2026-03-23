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
    public class BagDto : FullAuditedEntityDto<int>
    {
        public string BagCode { get; set; }
        public int BagType { get; set; }

        public string BagTypeName
        {
            get
            {
                var type = (BagTypeEnum)BagType;
                return type.GetDescription();
            }
        }


        public string CustomerName { get; set; }

        public string WarehouseCreateName { get; set; }

        public string WarehouseDestinationName { get; set; }

        public string CurrentWarehouseName { get; set; }


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
        public bool IsClosed { get; set; }
        public int? ShippingPartnerId { get; set; }
        public string ShippingPartnerName { get; set; }
        public int ShippingType { get; set; }
        public string? ShippingTypeName { get; set; }
        public int? WarehouseStatus { get; set; }

        /// <summary>
        /// Trường này là trường cân nặng thực tế của bao
        /// </summary>
        public decimal? Weight { get; set; }

        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        public int? BagSize { get; set; }

        // Trường này chỉ tính cân của kiện trong bao
        public decimal? WeightPackage { get; set; }
        // Tổng cân của các kiện
        public decimal? WeightPackages { get; set; }

        public decimal? VolumePackages
        {
            get
            {
                return Length * Width * Height;
            }
        }

        public int? TotalPackages { get; set; }

        public decimal? Volume { get; set; }
        public int? DeliveryNoteId { get; set; }
        public string DeliveryNoteCode { get; set; } // Mã phiếu xuất kho

        public DateTime CreationTime { get; set; } // thời gian tạo
        public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo
        public bool IsWeightCover { get; set; } // Có tính bì hay không?
        public decimal? WeightCover { get; set; } // Trọng lượng bao bì có được tính vào trọng lượng của kiện hàng hay không
        public decimal? WeightCoverFee { get; set; } // Phí bao bì

        public int ShippingStatus { get; set; }
        [CanBeNull] public string otherReason { get; set; }

        public DateTime? ImportDate { get; set; } // thời gian nhập kho ở VN
        public DateTime? ExportDate { get; set; } // thời gian xuất kho ở TQ
        public DateTime? TransferWarehouseTime { get; set; } // thời gian chuyển kho
        public string ShippingStatusString
        {
            get { return ((BagShippingStatus)ShippingStatus).GetDescription(); }
        }


        /// <summary>
        /// Tổng cân nặng bao gồm cả cân nặng kiện và cân nặng bao bì
        /// </summary>
        public decimal TotalAllWeight { get; set; }

        [CanBeNull] public virtual CustomerDto Customer { get; set; }
        [CanBeNull] public virtual WarehouseDto WarehouseCreate { get; set; }
        [CanBeNull] public virtual WarehouseDto WarehouseDestination { get; set; }
        [CanBeNull] public virtual ShippingPartnerDto ShippingPartner { get; set; }
        [CanBeNull] public virtual List<PackageDto> PackagesDtos { get; set; }

        /// <summary>
        /// Tổng phí vận chuyển của các kiện trong bao
        /// </summary>
        public decimal? TotalPackageFee { get; set; }

        /// <summary>
        /// Tổng phí vận chuyển của bao (tổng phí kiện + phí bao bì + các phí khác)
        /// </summary>
        public decimal? TotalFee { get; set; }

        /// <summary>
        /// Tổng số kiện trong bao
        /// </summary>
        public int? TotalPackage { get; set; }

        /// <summary>
        /// Tổng cân nặng của các kiện trong bao
        /// </summary>
        public decimal? TotalWeightPackage { get; set; }


        public decimal? TotalOriginShippingCost { get; set; } // Tổng phí vận chuyển gốc của bao (tổng phí kiện + phí bao bì + các phí khác)
        public decimal? TotalOriginPackageShippingCost { get; set; } // Tổng phí vận chuyển gốc của kien 
    }
}