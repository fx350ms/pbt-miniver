using Abp.Domain.Entities.Auditing;

namespace pbt.Entities
{
    public class FileUpload : AuditedEntity<long>
    {
        public string FileName { get; set; } // Tên file
        public string FilePath { get; set; } // Đường dẫn file
        public long? FileSize { get; set; } // Kích thước file
        public string FileType { get; set; } // Kiểu file
        public byte[] FileContent { get; set; } // Nội dung file
        public string Description { get; set; } // Mô tả file
        public bool IsDeleted { get; set; } = false; // Trạng thái xóa
    }
}
