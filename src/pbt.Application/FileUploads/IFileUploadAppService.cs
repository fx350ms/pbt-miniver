using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.FileUploads.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.FileUploads
{
    public interface IFileUploadAppService : IAsyncCrudAppService<FileUploadDto, long, PagedResultRequestDto, FileUploadDto, FileUploadDto>
    {
        public Task<List<FileUploadNameDto>> UploadFilesAsync([FromForm] List<IFormFile> files);

        public  Task<List<FileUploadDto>> GetByIdsAsync(List<long> ids);
    }
}