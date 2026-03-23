using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.ApplicationUtils;
using pbt.Customers.Dto;
using pbt.Packages.Dto;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.DeliveryNotes.Dto
{
    public class DeliveryNoteExportViewDto : EntityDto<int>
    {
        public string DeliveryNoteCode { get; set; }    
        public string Receiver { get; set; }
        public long CustomerId { get; set; }
        /// <summary>
        /// Phí vận chuyển nội địa (tại Việt Nam)
        /// </summary>
        public decimal DeliveryFee { get; set; }
        public DateTime? ExportTime { get; set; } // Thời gian xuất
        public int? ShippingPartnerId { get; set; } // đơn vị vận chuyển việt nam
        public string ShippingPartnerName { get; set; } // đơn vị vận chuyển việt nam
        public int DeliveryFeeReason { get; set; } // Lý do tính phí vận chuyển nội địa
        public List<DeliveryNoteExportItemViewDto> Items { get; set; }
        public decimal TotalFee { get; set; }

    }

    public class DeliveryNoteExportItemViewDto
    {

        public int Id { get; set; }
        /// <summary>
        /// Mã bao hàng (Alias: ItemCode)
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// Loại đối tượng: 2 - Bag (Alias: ItemType)
        /// </summary>
        public int ItemType { get; set; } = 2;

        /// <summary>
        /// Có phải hàng đã được xử lý (Solution) không? (ISNULL(b.IsSolution, 0))
        /// </summary>
        public bool IsSolution { get; set; }

        /// <summary>
        /// Có phải hàng đã được niêm phong gỗ không? (ISNULL(b.IsWoodSealing, 0))
        /// </summary>
        public bool IsWoodSealing { get; set; }

        /// <summary>
        /// Có phải hàng giả không? (ISNULL(b.IsFakeGoods, 0))
        /// </summary>
        public bool IsFakeGoods { get; set; }

        /// <summary>
        /// Có tính năng khác không? (ISNULL(b.IsOtherFeature, 0))
        /// </summary>
        public bool IsOtherFeature { get; set; }

        public string OtherReason { get; set; }

        /// <summary>
        /// ID đối tác vận chuyển
        /// </summary>
        public long? ShippingPartnerId { get; set; }

        /// <summary>
        /// Tên đối tác vận chuyển
        /// </summary>
        public string ShippingPartnerName { get; set; }

        /// <summary>
        /// Line vận chuyển (Alias: ShippingLine)
        /// </summary>
        public int ShippingLine { get; set; }

        /// <summary>
        /// Cân nặng (gốc)
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Thể tích
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Cân nặng bì
        /// </summary>
        public decimal WeightCover { get; set; }

        /// <summary>
        /// Phí cân nặng bì
        /// </summary>
        public decimal WeightCoverFee { get; set; }

        /// <summary>
        /// Trạng thái vận chuyển
        /// </summary>
        public int ShippingStatus { get; set; }

        /// <summary>
        /// Tổng tất cả cân nặng: Kiện + bì (Alias: TotalAllWeight)
        /// </summary>
        public decimal TotalAllWeight { get; set; }

        /// <summary>
        /// Tổng số lượng kiện trong bao
        /// </summary>
        public int TotalPackages { get; set; }

        /// <summary>
        /// Tổng tất cả các phí của kiện ( vận chuyển, dịch vụ) + phí bì
        /// </summary>
        public decimal TotalFee { get; set; }

        /// <summary>
        /// Cân nặng kiện (Tổng cân nặng các kiện trong bao)
        /// </summary>
        public decimal TotalWeightPackage { get; set; }

        /// <summary>
        /// Phí vận chuyển
        /// </summary>
        public decimal ShippingFee { get; set; }

        /// <summary>
        /// Phí vận chuyển nội địa TQ
        /// </summary>
        public decimal DomesticShippingFee { get; set; }

        /// <summary>
        /// Phí chống sốc
        /// </summary>
        public decimal ShockproofFee { get; set; }

        /// <summary>
        /// Phí bảo hiểm
        /// </summary>
        public decimal InsuranceFee { get; set; }

        /// <summary>
        /// Phí đóng gỗ
        /// </summary>
        public decimal WoodenPackagingFee { get; set; }

        /// <summary>
        /// Ngày nhập VN
        /// </summary>
        public DateTime? ImportDate { get; set; }

        /// <summary>
        /// Thời gian tạo (CreationTime)
        /// </summary>
        public DateTime CreationTime { get; set; }

    }
}
