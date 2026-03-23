using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Orders.Dto;
using pbt.Warehouses;
using pbt.Web.Models.Orders;
using pbt.Web.Models.Packages;
using pbt.Web.Models.Warehouses;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using pbt.Authorization;
using pbt.Authorization.Users;
using pbt.CustomerAddresss;
using pbt.Customers;
using DocumentFormat.OpenXml.Spreadsheet;
using pbt.Web.Models.Operate;
using pbt.Users;
using pbt.BarCodes.Dto;
using ClosedXML.Excel;
using System.IO;
using pbt.BarCodes;
using System;
using pbt.ApplicationUtils;
using Abp.Authorization;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    //  [AbpMvcAuthorize]
    public class OperateController : pbtControllerBase
    {
        private readonly pbtAppSession _pbtAppSession;
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly ICustomerAppService _customerAppService;
        private readonly UserManager _userManager;
        private readonly IUserAppService _userAppService;
        private readonly IBarCodeAppService _barCodeService;

        public OperateController(
            IWarehouseAppService warehouseAppService,
            pbtAppSession pbtAppSession,
            ICustomerAddressAppService customerAddressService,
            ICustomerAppService customerAppService,
            UserManager userManager,
            IUserAppService userAppService,
            IBarCodeAppService barCodeService
            )
        {
            _warehouseAppService = warehouseAppService;
            _pbtAppSession = pbtAppSession;
            _customerAppService = customerAppService;
            _userManager = userManager;
            _userAppService = userAppService;
            _barCodeService = barCodeService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ScanCodeHistoryViewModel
            {
                WarehouseUser = await _userAppService.GetWarehouseUsersAsync(),
            };
            return View(model);
        }

        public async Task<IActionResult> Download(PagedBarCodeResultRequestDto input)
        {
            // Lấy dữ liệu dựa theo filter

            input.MaxResultCount = int.MaxValue;
            input.SkipCount = 0;
            var result = await _barCodeService.GetDataAsync(input);
            var items = result.Items;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Danh sách quét mã");

                // Tiêu đề cột
                var headers = new[] { "STT", "Quét mã", "Thời gian quét", "Loại mã", "Hành động", "Kho quét", "Người quét" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Ghi dữ liệu
                int row = 2;
                int index = 1;
                foreach (var item in items)
                {
                    worksheet.Cell(row, 1).Value = index++; // STT
                    worksheet.Cell(row, 2).Value = item.ScanCode ?? ""; // Quét mã
                    worksheet.Cell(row, 3).Value = item.CreationTime.ToString("dd/MM/yyyy HH:mm"); // Thời gian quét

                    // Loại mã
                    string codeTypeStr = ((CodeType)item.CodeType).GetDescription();
                    worksheet.Cell(row, 4).Value = codeTypeStr;

                    // Hành động
                    string actionStr = ((OperateActionType)item.Action).GetDescription();
                    worksheet.Cell(row, 5).Value = actionStr;

                    // Kho quét
                    worksheet.Cell(row, 6).Value = item.SourceWarehouseName;

                    // Người quét
                    worksheet.Cell(row, 7).Value = item.CreatorUserName ?? "";

                    row++;
                }

                // Định dạng bảng
                var dataRange = worksheet.Range(1, 1, row - 1, headers.Length);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"DanhSachQuetMa_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [AbpAuthorize]
        public async Task<IActionResult> Create()
        {

            var warehouseId = _pbtAppSession.WarehouseId;
            if (warehouseId == null)
            {
                return RedirectToAction("Error", "Home", new { Message = L("YourAccountIsNotAssignToWarehouse") });
            }
            var warehouse = await _warehouseAppService.GetAsync(warehouseId.Value);
            ViewBag.warehouseType = warehouse.CountryId;
            return View();
        }

        public async Task<IActionResult> History()
        {
            var model = new ScanCodeHistoryViewModel
            {
                WarehouseUser = await _userAppService.GetWarehouseUsersAsync(),
            };
            return View(model);
        }

    }
}
