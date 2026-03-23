using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.ComplaintImages;
using pbt.ComplaintImages.Dto;
using pbt.Complaints.Dto;
using pbt.Entities;
using pbt.Orders.Dto;
using pbt.Users.Dto;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Linq.Extensions;
using Abp.Collections.Extensions;
using Abp.AutoMapper;

namespace pbt.Complaints
{
    public class ComplaintAppService : AsyncCrudAppService<Complaint, ComplaintDto, long, PagedComplaintResultRequestDto, ComplaintDto, ComplaintDto>, IComplaintAppService
    {
        public IComplaintImageAppService _complaintImageService;
        private pbtAppSession _pbtAppSession;
        public ComplaintAppService(IRepository<Complaint, long> repository,
            IComplaintImageAppService complaintImageService,
            pbtAppSession pbtAppSession
            )
            : base(repository)
        {
            _complaintImageService = complaintImageService;
            _pbtAppSession = pbtAppSession;
        }


        public async override Task<PagedResultDto<ComplaintDto>> GetAllAsync(PagedComplaintResultRequestDto input)
        {
            //   var currentUserId = AbpSession.UserId;
            var query = base.CreateFilteredQuery(input);

            //     query = query.Where(x => x.CreatorUserId == currentUserId);
            query = query.Where(x => input.Status == -1 || x.Status == input.Status);
            //query = query.Where(x => !string.IsNullOrEmpty(input.Keyword) || (x.OrderCode.ToUpper().Contains(input.Keyword.ToUpper()) || x.Id.ToString() == input.Keyword));
            query = query.Where(x => input.StartDate == null || x.CreationTime >= input.StartDate);
            query = query.Where(x => input.EndDate == null || x.CreationTime <= input.EndDate);

            var count = query.Count();

            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            //   var data = query.ToList()
            return new PagedResultDto<ComplaintDto>()
            {
                Items = query.ToList().MapTo<List<ComplaintDto>>(),
                TotalCount = count
            };
            //return base.GetAllAsync(input);
        }

        public async Task<ComplaintDto> CreateWithImagesAsync([FromForm] ComplaintDto input)
        {

            input.Status = (int)ComplaintStatus.Pending;

            var complaint = await base.CreateAsync(input);

            // Lưu hình ảnh
            if (input.Images != null && input.Images.Any())
            {
                foreach (var file in input.Images)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream); // Đọc dữ liệu từ file

                        var image = new ComplaintImageDto
                        {
                            ComplaintId = complaint.Id,
                            ImageData = memoryStream.ToArray(), // Dữ liệu nhị phân của hình ảnh
                            ContentType = file.ContentType,    // Loại file (image/jpeg, image/png)
                            FileName = file.FileName           // Tên file
                        };
                        try
                        {
                            await _complaintImageService.CreateAsync(image);
                        }
                        catch (System.Exception ex)
                        {

                        }
                        // Lưu hình ảnh vào database

                    }
                }
            }

            return complaint;
        }

        public async Task<List<ComplaintDto>> GetByOrderId(long orderId)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.OrderId == orderId);
            var data = query.ToList();
            return data.MapTo<List<ComplaintDto>>();
        }
    }
}
