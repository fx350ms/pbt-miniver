using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using pbt.Controllers;
using pbt.FileUploads;
using pbt.FileUploads.Dto;
using pbt.FundAccounts;
using pbt.Transactions;
using pbt.Transactions.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using pbt.Web.Models;
using pbt.Packages.Dto;
using pbt.ApplicationUtils;
using System;
using pbt.Customers;
using pbt.Web.Models.Transactions;

namespace pbt.Web.Controllers
{
    public class TransactionController : pbtControllerBase
    {
        private readonly ITransactionAppService _transactionAppService;
        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly IFileUploadAppService _fileUploadAppService;
        private readonly ICustomerAppService _customerAppService;
        public TransactionController(ITransactionAppService transactionAppService,
            IFundAccountAppService fundAccountAppService,
            ICustomerAppService customerAppService,
            IFileUploadAppService fileUploadAppService
            )
        {
            _transactionAppService = transactionAppService;
            _fundAccountAppService = fundAccountAppService;
            _fileUploadAppService = fileUploadAppService;
            _customerAppService = customerAppService;
        }

        public async Task<IActionResult> Index()
        {
            var activeFundAccounts = await _fundAccountAppService.GetFundAccountsByCurrentUserAsync();
            ViewBag.ActiveFundAccounts = activeFundAccounts;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            // Lấy thông tin chi tiết giao dịch từ service
            var transaction = await _transactionAppService.GetAsync(new EntityDto<long>(id));

            if (transaction == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy giao dịch
            }

            var fundAcount = await _fundAccountAppService.GetAsync(new EntityDto<int>( transaction.FundAccountId));   

            // Map dữ liệu sang ViewModel
            var model = new TransactionDetailViewModel
            {
                Transaction = transaction,
                FundAccount = fundAcount,   
                //Id = transaction.Id,
                ////FundAccountName = transaction.FundAccountName,
                ////FundAccountBalance = transaction.FundAccountBalance,
                //Currency = transaction.Currency,
                //TransactionType = transaction.TransactionType,
                //TransactionDirection = transaction.TransactionDirection,
                //Amount = transaction.Amount,
                //RefCode = transaction.RefCode,
                //OrderId = transaction.OrderId,
                ////RecipientPayerName = transaction.RecipientPayerName,
                ////TotalDebt = transaction.TotalDebt,
                ////MaxDebt = transaction.MaxDebt,
                //CurrentAmount = transaction.Amount,
                //TransactionContent = transaction.TransactionContent,
                //Notes = transaction.Notes,
                //Files = transaction.Files?.Select(f => new FileDto
                //{
                //    Id = f.Id,
                //    FileName = f.FileName
                //}).ToList()
            };

            // Trả về view Details với model
            return View("Details", model);
        }

        public async Task<ActionResult> CreateReceipt()
        {
            var activeFundAccounts = await _fundAccountAppService.GetFundAccountsByCurrentUserAsync();
            ViewBag.ActiveFundAccounts = activeFundAccounts;
            return View();
        }

        public async Task<ActionResult> CreatePayment()
        {
            var activeFundAccounts = await _fundAccountAppService.GetFundAccountsByCurrentUserAsync();
            ViewBag.ActiveFundAccounts = activeFundAccounts;
            return View();
        }


        public async Task<ActionResult> Create()
        {
            var activeFundAccounts = await _fundAccountAppService.GetFundAccountsByCurrentUserAsync();
            ViewBag.ActiveFundAccounts = activeFundAccounts;

            
            return View();
        }

        // public async Task<PagedResultDto<TransactionDto>> GetAllDataAsync(PagedTransactionResultRequestDto input)


        public async Task<IActionResult> ExportExcel(PagedTransactionResultRequestDto input)
        {
            // Lấy danh sách giao dịch dựa trên filter
            input.MaxResultCount = int.MaxValue;

            var dataPaged = await _transactionAppService.GetAllDataAsync(input);
            var transactions = dataPaged.Items;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Giao dịch");

                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT",
                    "Thời gian",
                    "Mã giao dịch",
                    "Mã tham chiếu",
                    "Nội dung giao dịch",
                    "Số tiền trước giao dịch",
                    "Số tiền thu",
                    "Số tiền chi",
                    "Số tiền sau giao dịch",
                    "Đơn vị tiền tệ"
                };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                }

                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 2;
                int index = 1;
                foreach (var transaction in transactions)
                {
                    worksheet.Cell(currentRow, 1).Value = index++; // STT
                    worksheet.Cell(currentRow, 2).Value = transaction.CreationTime.ToString("dd/MM/yyyy HH:mm"); // Thời gian
                    worksheet.Cell(currentRow, 3).Value = transaction.TransactionId; // Mã giao dịch
                    worksheet.Cell(currentRow, 4).Value = transaction.RefCode; // Mã tham chiếu
                    worksheet.Cell(currentRow, 5).Value = transaction.TransactionContent; // Nội dung giao dịch

                    worksheet.Cell(currentRow, 6).Value = transaction.TotalAmount - transaction.Amount; // Số tiền trước giao dịch
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 7).Value = transaction.TransactionDirection == (int)TransactionDirectionEnum.Receipt ? transaction.Amount : 0; // Số tiền thu
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 7).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 8).Value = transaction.TransactionDirection == (int)TransactionDirectionEnum.Expense ? transaction.Amount : 0; // Số tiền chi
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 9).Value = transaction.TotalAmount; // Số tiền sau giao dịch
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 10).Value = transaction.Currency; // Số tiền sau giao dịch
                    currentRow++;
                }

                // Định dạng bảng
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                range.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                range.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

                // Tự động điều chỉnh kích thước cột
                worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Transactions_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult> Files(long id)
        {
            var model = new ViewFileModel()
            {

            };
            var transaction = await _transactionAppService.GetAsync(new EntityDto<long>(id));
            if (!string.IsNullOrEmpty(transaction.Files))
            {
                var files = JsonConvert.DeserializeObject<List<FileUploadNameDto>>(transaction.Files);

                var fileUploadDtos = await _fileUploadAppService.GetByIdsAsync(files.Select(x => x.Id).ToList());

                model.Files = fileUploadDtos;
            }
            return PartialView("_Files", model);
        }
    }
}