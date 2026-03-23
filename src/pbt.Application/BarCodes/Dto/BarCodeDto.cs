using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.BarCodes.Dto
{
    public class BarCodeDto : FullAuditedEntityDto<long>
    {
        /// <summary>
        /// Mã quét
        /// </summary>
        public string ScanCode { get; set; }


        /// <summary>
        /// Loại mã Bao/Kiện
        /// </summary>
        public int? CodeType { get; set; }

        /// <summary>
        /// Nội dung quét
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Mã kiện
        /// </summary>
        public string PackageCode { get; set; }

        /// <summary>
        /// Hành động (ví dụ: Đóng gói, Xuất kho, Nhập kho)
        /// </summary>
        public int? Action { get; set; }

        /// <summary>
        /// Kho quét
        /// </summary>
        public int? SourceWarehouseId { get; set; }
        /// <summary>
        /// Tên kho quét
        /// </summary>
        public string? SourceWarehouseName { get; set; }

        /// <summary>
        /// Kho đích
        /// </summary>
        public int? DestinationWarehouseId { get; set; }

        /// <summary>
        /// Đơn hàng liên quan
        /// </summary>
        public long? OrderId { get; set; }

        /// <summary>
        /// Tên người quen mã
        /// </summary>
        public string CreatorUserName { get; set; }
        public string CustomerName { get; set; }
    }
}
