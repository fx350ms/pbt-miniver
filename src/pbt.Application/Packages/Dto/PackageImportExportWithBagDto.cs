using Abp.Application.Services.Dto;
using pbt.Entities;
using System;


namespace pbt.Packages.Dto
{
    public class PackageImportExportWithBagDto : FullAuditedEntityDto<int>
    {

        /// <summary>
        /// NGày xuất kho TQ
        /// </summary>
        public DateTime? ExportDate { get; set; }

        /// <summary>
        /// Người nhận
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Mã vận đơn, Mã bao 
        /// </summary>
        public string WaybillCode { get; set; }
        public string PackageCode { get; set; }
        public string BagNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BagId { get; set; }
        public int PackageId { get; set; }

        /// <summary>
        /// Cân nặng bao(kg)
        /// </summary>
        public decimal? Weight { get; set; } // kg

        /// <summary>
        /// Kích thước(M3)
        /// </summary>
        public decimal Dimension { get; set; }

        /// <summary>
        /// Tổng số kiện
        /// </summary>
        public int PackageCount { get; set; }

        /// <summary>
        /// Đối tác vận chuyển QT
        /// </summary>
        public string ShippingPartner { get; set; } = string.Empty;

        /// <summary>
        /// Đặc tính
        /// </summary>
        public string Characteristic { get; set; } // Đặc tính

        /// <summary>
        /// Ngày nhập kho HN
        /// </summary>
        public DateTime? ImportDate { get; set; }


        /// <summary>
        /// Giá cước
        /// </summary>
        public decimal ShippingFeeIntern { get; set; } // Phí vận chuyển Quốc tế

        /// <summary>
        /// Ship nội địa TQ
        /// </summary>
        public decimal ShippingFeeCN { get; set; } // Phí vận chuyển Trung Quốc

        /// <summary>
        /// Phí gia cố hàng
        /// </summary>
        public decimal SecuringCost { get; set; } // Phí bảo hiểm / chống sốc

        /// <summary>
        /// Ship nội địa VN
        /// </summary>
        public decimal ShippingFeeVN { get; set; } // Phí ship nội địaVN

        /// <summary>
        /// Thành tiền
        /// </summary>
        public decimal TotalFee { get; set; } // Tổng phí

        /// <summary>
        /// Phí ship nội địa VN kho chịu
        /// </summary>
        public decimal ShippingFeeAbsorbedByWarehouse { get; set; } // Phí ship nội địa VN kho chịu

        /// <summary>
        /// Đối tác xuất
        /// </summary>
        public string ShippingPartnerVN { get; set; } // Đối tác vận chuyển VN

        /// <summary>
        /// Ngày xuất kho VN
        /// </summary>
        public DateTime? ExportDateVN { get; set; } // Ngày xuất kho VN


        public decimal? WoodenPackagingFee { get; set; } // phí đóng gỗ
        public int? ShippingLineId { get; set; }
        public string Note { get; set; }


        public int BagType { get; set; }

        public int? DeliveryNoteId { get; set; } // Phiếu xuất kho
        public DeliveryNote DeliveryNote { get; set; } // Phiếu xuất kho    

        public bool IsRepresentForDeliveryNote { get; set; } // Kiện đại diện cho phiếu xuất kho
        public decimal? UnitPrice { get; set; }  // giá vận chuyển trên 1 kg
        
        public decimal? OriginShippingCost { get; set; } // 1: KG, 2: M3

    }
}