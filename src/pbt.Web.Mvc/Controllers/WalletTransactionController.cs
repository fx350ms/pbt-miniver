using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Customers;
using pbt.Transactions;
using pbt.Transactions.Dto;
using pbt.Web.Models.Transactions;
using System;
using System.Threading.Tasks;
using ClosedXML.Excel;
using pbt.ApplicationUtils;
using pbt.FileUploads;
using pbt.Packages.Dto;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Customers)]
    //  [AbpMvcAuthorize]
    public class WalletTransactionController : pbtControllerBase
    {
        private readonly ICustomerTransactionAppService _customerTransactionAppService;
        private readonly IFileUploadAppService _fileUploadAppService;

        public WalletTransactionController(
            ICustomerTransactionAppService customerTransactionAppService,
            IFileUploadAppService fileUploadAppService
        )
        {
            _customerTransactionAppService = customerTransactionAppService;
            _fileUploadAppService = fileUploadAppService;
        }

        //private readonly IWalletTransactionAppService _walletTransaction;
        //private readonly IChargingRequestAppService _chargingRequestAppService;

        //public WalletTransactionController(IWalletTransactionAppService walletTransaction,
        //    IChargingRequestAppService chargingRequestAppService)
        //{
        //    _walletTransaction = walletTransaction;
        //    _chargingRequestAppService = chargingRequestAppService;
        //}

        public async Task<IActionResult> Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> Process(ChargingRequestDto request)
        //{
        //    try
        //    {
        //        var result = _chargingRequestAppService.ProcessAsync(request);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}
        public async Task<IActionResult> Download(PagedCustomerTransactionResultRequestDto input)
{
    var result = await _customerTransactionAppService.GetCurrentCustomerTransactionAsync(input);
    var packages = result.Items;

    using (var workbook = new ClosedXML.Excel.XLWorkbook())
    {
        var worksheet = workbook.Worksheets.Add("Packages");

        var headers = new[]
        {
            "STT",
            "Thời gian",
            "Mã giao dịch",
            "Loại giao dịch",
            "Ghi chú",
            "Giá trị nạp tiền",
            "Giá trị thanh toán",
            "Giá trị rút tiền",
            "Số dư sau giao dịch",
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c9daf8");
            cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
        }

        int currentRow = 2;
        foreach (var p in packages)
        {
            worksheet.Cell(currentRow, 1).Value = currentRow - 1;
            worksheet.Cell(currentRow, 2).Value = p.TransactionDate.ToString("dd/MM/yyyy HH:mm") ?? "";
            worksheet.Cell(currentRow, 3).Value = p.ReferenceCode;

            worksheet.Cell(currentRow, 4).Value = ((TransactionTypeEnum)p.TransactionType).GetDescription();
            worksheet.Cell(currentRow, 5).Value = p.Description;

            var transactionTypes = new[]
                { TransactionTypeEnum.Deposit, TransactionTypeEnum.Payment, TransactionTypeEnum.Withdraw };
            var columns = new[] { 6, 7, 8 };

            for (int i = 0; i < transactionTypes.Length; i++)
            {
                if (p.TransactionType == (int)transactionTypes[i])
                {
                    worksheet.Cell(currentRow, columns[i]).Value = p.Amount;
                    worksheet.Cell(currentRow, columns[i]).Style.NumberFormat.Format = p.Amount % 1 == 0 ? "#,##0" : "#,##0.0";
                }
                else
                {
                    worksheet.Cell(currentRow, columns[i]).Value = "";
                }
            }

            worksheet.Cell(currentRow, 9).Value = p.BalanceAfterTransaction;
            worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = p.BalanceAfterTransaction % 1 == 0 ? "#,##0" : "#,##0.0";

            currentRow++;
        }

        var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
        range.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        range.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

        // Fix alignment for columns 6, 7, and 8
        worksheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        worksheet.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Columns().AdjustToContents();

        using (var stream = new System.IO.MemoryStream())
        {
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var fileName = $"CustomerTransaction_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
    }
}