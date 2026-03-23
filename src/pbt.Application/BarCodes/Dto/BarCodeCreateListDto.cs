using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.BarCodes.Dto
{
    public class BarCodeCreateListDto 
    {
        /// <summary>
        /// Mã quét
        /// </summary>
        public string ScanCode { get; set; }

        /// <summary>
        /// Thời gian quét
        /// </summary>
        public DateTime ScanTime { get; set; }

        /// <summary>
        /// Loại mã (ví dụ: QR code, barcode)
        /// </summary>
        public string CodeType { get; set; }

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
        public int Action { get; set; }

        /// <summary>
        /// Kho quét
        /// </summary>
        public int SourceWarehouseId { get; set; }

        /// <summary>
        /// Kho đích
        /// </summary>
        public int DestinationWarehouseId { get; set; }

        /// <summary>
        /// Đơn hàng liên quan
        /// </summary>
        public long OrderId { get; set; }


    }
}
