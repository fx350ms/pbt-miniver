using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace pbt.FileUploads.Dto
{
    public class FileUploadDto : FullAuditedEntityDto<long>
    {
        public string FileName { get; set; } // Tên file
        public string FilePath { get; set; } // Đường dẫn file
        public long? FileSize { get; set; } // Kích thước file
        public string FileType { get; set; } // Kiểu file
        public byte[] FileContent { get; set; } // Nội dung file
        public string Description { get; set; } // Mô tả file
        public bool IsDeleted { get; set; } // Trạng thái xóa
    }

    public class CreateAndUpdateFileUploadDto : FullAuditedEntityDto<long>
    {
        public string FileName { get; set; } // Tên file
        public string FilePath { get; set; } // Đường dẫn file
        public long? FileSize { get; set; } // Kích thước file
        public string FileType { get; set; } // Kiểu file
        public byte[] FileContent { get; set; } // Nội dung file
        public string Description { get; set; } // Mô tả file
        public bool IsDeleted { get; set; } // Trạng thái xóa

        public List<IFormFile> Attachments { get; set; }
    }
}