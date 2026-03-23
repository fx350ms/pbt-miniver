using Abp.Domain.Entities;
using System;

namespace pbt.Entities
{
    public class ComplaintImage : Entity<long>
    {
        public long ComplaintId { get; set; } // FK đến Complaint
        public byte[] ImageData { get; set; } // Dữ liệu nhị phân của hình ảnh
        public string ContentType { get; set; } // Loại file (image/png, image/jpeg)
        public string FileName { get; set; } // Tên file
        public string Description { get; set; } // Mô tả ảnh
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Ngày tạo
    }
}
