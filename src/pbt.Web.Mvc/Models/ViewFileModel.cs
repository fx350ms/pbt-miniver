using pbt.FileUploads.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models
{
    public class ViewFileModel
    {
        public List<FileUploadDto> Files { get; set; } = new List<FileUploadDto>();
    }
}
