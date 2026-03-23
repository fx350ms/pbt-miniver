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

namespace pbt.DeliveryNotes.Dto
{
    public class DeliveryNoteDto : EntityDto<int>
    {
        public string DeliveryNoteCode { get; set; }
        public int Status { get; set; }

        public string Receiver { get; set; }

        /// <summary>
        /// Phí vận chuyển quốc tế
        /// </summary>
        public decimal? ShippingFee { get; set; }
        /// <summary>
        /// Phí vận chuyển nội địa (tại Việt Nam)
        /// </summary>
        public decimal? DeliveryFee { get; set; }
        public decimal FinancialNegativePart { get; set; } // phần âm tài chính
        public decimal? Cod { get; set; } // cod
                                          // public int Size { get; set; } // kích thươc
        [CanBeNull] public string Note { get; set; }
        public string RecipientPhoneNumber { get; set; } // số điện thoại người nhận
        public string RecipientAddress { get; set; } // địa chỉ người nhận
        [CanBeNull] public string CreatorName { get; set; } //người tạo

        public DateTime CreationTime { get; set; } // thời gian tạo
        public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo
        public int? ExporterId { get; set; } // người xuất
        public string? ExporterName { get; set; } // ten người xuất
        public DateTime? ExportTime { get; set; } // Thời gian xuất
        public string ExportTimeFormat => ExportTime?.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo

        public decimal? Width { get; set; } // Chiều rộng
        public decimal? Height { get; set; } // Chiều cao
        public decimal? Length { get; set; } // Chiều dài

        public int? ShippingPartnerId { get; set; } // đơn vị vận chuyển

        public int? ExportWarehouse { get; set; } // kho xuất
        public string? ExportWarehouseName { get; set; } // kho xuất
        public int? TotalItem { get; set; } //Tổng số lượng hàng xuất
        public decimal? TotalWeight { get; set; } // Tổng cân nặng
        public int? DeliveryFeeReason { get; set; } // Lý do tính phí vận chuyển nội địa
        public decimal? BalanceBefore { get; set; } /// Số dư trước khi xuất kho
        public decimal? BalanceAfter { get; set; } /// Số dư sau khi xuất kho   
        public decimal? TotalVolume { get; set; } // Tổng thể tích
        public decimal? TotalFee { get; set; } // Tổng tất cả phí

        /// <summary>
        /// 
        /// </summary>
        public long? CustomerId { get; set; }

        public CustomerDto Customer { get; set; }
        public ShippingPartnerDto ShippingPartner { get; set; }
    }
}
