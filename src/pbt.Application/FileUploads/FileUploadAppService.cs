using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.Entities;
using pbt.FileUploads.Dto;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace pbt.FileUploads
{
    public class FileUploadAppService : AsyncCrudAppService<FileUpload, FileUploadDto, long, PagedResultRequestDto, FileUploadDto, FileUploadDto>, IFileUploadAppService
    {
        private readonly IRepository<FileUpload, long> _fileUploadRepository;

        public FileUploadAppService(IRepository<FileUpload, long> repository)
            : base(repository)
        {
            _fileUploadRepository = repository;
        }

        public async Task<List<FileUploadDto>> GetByIdsAsync(List<long> ids)
        {
            var query = await _fileUploadRepository.GetAllAsync();
            query = query.Where(x => ids.Contains(x.Id));
            var fileUploads = await query.ToListAsync();

            return ObjectMapper.Map<List<FileUploadDto>>(fileUploads);

        }


        public async Task<List<FileUploadNameDto>> UploadFilesAsync([FromForm] List<IFormFile> files)
        {
            var fileIds = new List<FileUploadNameDto>();

            foreach (var file in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    try
                    {
                        await file.CopyToAsync(memoryStream); // Đọc dữ liệu từ file
                        var fileContent = memoryStream.ToArray(); // Chuyển đổi dữ liệu thành mảng byte
                        var fileUpload = new FileUploadDto
                        {
                            FileName = file.FileName,
                            FileType = file.ContentType,    // Loại file (image/jpeg, image/png)
                            FileContent = fileContent, // Dữ liệu nhị phân của hình ảnh
                            FileSize = file.Length,
                        };
                        var fileUploaded = await base.CreateAsync(fileUpload); // Lưu vào database
                        fileIds.Add(new FileUploadNameDto()
                        {
                            Id = fileUploaded.Id,
                            FileName = fileUploaded.FileName,
                        });

                    }
                    catch (System.Exception ex)
                    {
                    }
                }
            }
            return fileIds;
        }
    }
}