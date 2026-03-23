using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.CustomerFakes.Dto;
using pbt.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace pbt.CustomerFakes
{
    public class CustomerFakeAppService : AsyncCrudAppService<CustomerFake, CustomerFakeDto, long, PagedAndSortedResultRequestDto, CreateUpdateCustomerFakeDto, CustomerFakeDto>, ICustomerFakeAppService
    {

        public CustomerFakeAppService(IRepository<CustomerFake, long> repository)
            : base(repository)
        {

        }

        public override Task<PagedResultDto<CustomerFakeDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
        {
            try
            {
                return base.GetAllAsync(input);
            }
            catch (Exception ex)
            {

                throw;
            }

            return null;

        }

        public async Task ImportCustomerFakesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            var CustomerFakes = new List<CustomerFake>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                IWorkbook workbook;
                if (Path.GetExtension(file.FileName).Equals(".xls"))
                {
                    workbook = new HSSFWorkbook(stream); // Định dạng .xls
                }
                else if (Path.GetExtension(file.FileName).Equals(".xlsx"))
                {
                    workbook = new XSSFWorkbook(stream); // Định dạng .xlsx
                }
                else
                {
                    throw new ArgumentException("Định dạng tệp không được hỗ trợ.");
                }

                var sheet = workbook.GetSheetAt(0); // Lấy sheet đầu tiên
                for (int i = 1; i <= sheet.LastRowNum; i++) // Bỏ qua hàng tiêu đề
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var CustomerFake = new CustomerFake
                    {
                        FullName = row.GetCell(1)?.ToString(),
                        PhoneNumber = row.GetCell(3)?.ToString(),
                        Address = row.GetCell(4)?.ToString(),
                       
                    };

                    CustomerFakes.Add(CustomerFake);
                }
            }

            foreach (var CustomerFake in CustomerFakes)
            {
                await Repository.InsertAsync(CustomerFake);
            }
        }

        public async Task<byte[]> ExportCustomerFakesToExcelAsync()
        {
            // Lấy danh sách khách hàng từ database
            var CustomerFakes = await Repository.GetAllListAsync();

            // Tạo workbook và sheet
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("CustomerFakes");

            // Tạo hàng tiêu đề (header)
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Id");
            headerRow.CreateCell(1).SetCellValue("Full Name");
            headerRow.CreateCell(2).SetCellValue("Email");
            headerRow.CreateCell(3).SetCellValue("Phone Number");
            headerRow.CreateCell(4).SetCellValue("Address");
            headerRow.CreateCell(5).SetCellValue("Date of Birth");
            headerRow.CreateCell(6).SetCellValue("Gender");
            headerRow.CreateCell(7).SetCellValue("Status");

            // Thêm dữ liệu khách hàng vào sheet
            for (int i = 0; i < CustomerFakes.Count; i++)
            {
                var CustomerFake = CustomerFakes[i];
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(CustomerFake.Id);
                row.CreateCell(1).SetCellValue(CustomerFake.FullName ?? "");
                row.CreateCell(3).SetCellValue(CustomerFake.PhoneNumber ?? "");
                row.CreateCell(4).SetCellValue(CustomerFake.Address ?? "");
               
            }

            // Ghi dữ liệu ra một mảng byte
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

    }
}
