using Abp.Domain.Entities.Auditing;
using pbt.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    public class BarCode : FullAuditedEntity<long> // Sử dụng FullAuditedEntity để ghi nhận các thông tin tạo, cập nhật, xóa
    {
        /// <summary>
        /// Mã quét
        /// </summary>
        public string ScanCode { get; set; }


        /// <summary>
        /// Loại mã: Mã vận đơn/mã kiện
        /// </summary>
        public int? CodeType { get; set; }

        /// <summary>
        /// Nội dung quét: Quét mã tạo kiện, quét mã xuất kho, đóng bao
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
