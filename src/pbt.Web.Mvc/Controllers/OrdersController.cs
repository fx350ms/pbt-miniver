using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using AspNetCoreGeneratedDocument;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Complaints;
using pbt.ConfigurationSettings;
using pbt.Controllers;
using pbt.CustomerAddresss;
using pbt.CustomerAddresss.Dto;
using pbt.Customers;
using pbt.Customers.Dto;
using pbt.OrderHistories;
using pbt.OrderLogs;
using pbt.OrderNumbers;
using pbt.Orders;
using pbt.Orders.Dto;
using pbt.Packages;
using pbt.Packages.Dto;
using pbt.Warehouses;
using pbt.Web.Models.DeliveryNote;
using pbt.Web.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Customers)]
    [AbpMvcAuthorize]
    public class OrdersController : pbtControllerBase
    {
        private readonly IOrderAppService _orderService;
        private readonly IWarehouseAppService _warehouseService;
        private readonly ICustomerAddressAppService _customerAddressService;
        private readonly IPackageAppService _packageAppService;
        private readonly IComplaintAppService _complaintAppService;
        private readonly IOrderHistoryAppService _orderHistoryAppService;
        private readonly IOrderLogAppService _orderLogAppService;
        private readonly ICustomerAppService _customerService;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IOrderNoteAppService _orderNoteAppService;
        //     private readonly IWaybillAppService _waybillAppService;
        private readonly string[] _roles;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IConfigurationSettingAppService _configurationSettingAppService;

        private readonly pbtAppSession _pbtAppSession;

        public OrdersController(IOrderAppService orderService,
                IWarehouseAppService warehouseService,
                ICustomerAddressAppService customerAddressService,
                ICustomerAppService customerService,
                IPackageAppService packageAppService,
                IComplaintAppService complaintAppService,
                IOrderHistoryAppService orderHistoryAppService,
                IOrderLogAppService orderLogAppService,
                IIdentityCodeAppService identityCodeAppService,
                IHttpContextAccessor httpContextAccessor,
                IConfigurationSettingAppService configurationSettingAppService,
                IOrderNoteAppService orderNoteAppService,
             //     IWaybillAppService waybillAppService,

             pbtAppSession pbtAppSession
            )
        {
            _orderService = orderService;
            _warehouseService = warehouseService;
            _customerAddressService = customerAddressService;
            _pbtAppSession = pbtAppSession;
            _customerService = customerService;
            _packageAppService = packageAppService;
            _complaintAppService = complaintAppService;
            _orderHistoryAppService = orderHistoryAppService;
            _orderLogAppService = orderLogAppService;
            _identityCodeAppService = identityCodeAppService;
            // _waybillAppService = waybillAppService;
            _roles = httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
            _configurationSettingAppService = configurationSettingAppService;
            _orderNoteAppService = orderNoteAppService;
        }

        private async Task<int> GetMaxDayQuery()
        {
            var maxDayStr = await _configurationSettingAppService.GetValueAsync("MaxDayQuery");
            if (!int.TryParse(maxDayStr, out var _maxDayQuery))
            {
                _maxDayQuery = 3; // giá trị mặc định nếu không lấy được từ cấu hình
            }
            return _maxDayQuery;
        }

        public async Task<IActionResult> Index(int status = -1)
        {

            var model = new OrderListIndexModel()
            {
                Customers = await _customerService.GetCustomersByCurrentUserForSelectOrderViewAsync(""),
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetPackagesByOrder(long orderId)
        {
            var packages = await _packageAppService.GetPackagesByOrderId(orderId);
            return PartialView("_OrderPackages", packages);
        }

        [HttpGet]
        public async Task<IActionResult> GetPackagesTrackingByOrder(long orderId)
        {
            var packages = await _packageAppService.GetPackagesByOrderId(orderId);
            return PartialView("_OrderPackagesTracking", packages);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderNotes(long orderId)
        {
            var notes = await _orderNoteAppService.GetAllByOrderIdAsync(orderId);
            return PartialView("_orderNotes", notes);
        }

        public async Task<IActionResult> DownloadPackage(PagedMyOrderRequestDto input)
        {
            input.SkipCount = 0; // Đặt SkipCount về 0 để lấy tất cả các bản ghi
            input.MaxResultCount = int.MaxValue; // Đặt MaxResultCount về giá trị lớn nhất để lấy tất cả các bản ghi
            var maxDayQuery = await GetMaxDayQuery();

            #region giới hạn thời gian

            //// Kiểm tra khoảng cách giữa ngày bắt đầu và ngày kết thúc
            //if (input.EndDate.HasValue && input.StartDate.HasValue)
            //{
            //    var dateDifference = (input.EndDate.Value - input.StartDate.Value).TotalDays;
            //    if (dateDifference > maxDayQuery)
            //    {
            //        // lấy ngày kết thúc làm mốc 
            //        input.CreateEndDateStr = input.EndDate.Value.AddDays(-maxDayQuery).ToString("yyyy-MM-dd");
            //    }
            //}
            //else if (!input.EndDate.HasValue && input.StartDate.HasValue)
            //{
            //    // Nếu không có ngày kết thúc, lấy ngày bắt đầu + 3 ngày
            //    input.EndDateStr = input.StartDate.Value.AddDays(3).ToString("yyyy-MM-dd");
            //}
            //else if (input.EndDate.HasValue && !input.StartDate.HasValue)
            //{
            //    // Nếu có ngày kết thúc nhưng không có ngày bắt đầu, lấy ngày kết thúc - 3 ngày
            //    input.StartDateStr = input.EndDate.Value.AddDays(-3).ToString("yyyy-MM-dd");
            //}
            //else
            //{
            //    // Nếu không có cả ngày bắt đầu và ngày kết thúc, lấy ngày hiện tại - 3 ngày làm ngày bắt đầu
            //    input.EndDateStr = DateTime.Now.ToString("yyyy-MM-dd");
            //    input.StartDateStr = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            //}

            //// Xử lý khoảng cách giữa StartImportDate và EndImportDate
            //if (input.EndImportDate.HasValue && input.StartImportDate.HasValue)
            //{
            //    var dateDifference = (input.EndImportDate.Value - input.StartImportDate.Value).TotalDays;
            //    if (dateDifference > maxDayQuery)
            //    {
            //        // Nếu khoảng cách lớn hơn maxDayQuery, lấy EndImportDate làm mốc
            //        input.ImportStartDateStr = input.EndImportDate.Value.AddDays(-maxDayQuery).ToString("yyyy-MM-dd");
            //    }
            //}
            //else if (!input.EndImportDate.HasValue && input.StartImportDate.HasValue)
            //{
            //    // Nếu không có EndImportDate, lấy StartImportDate + 3 ngày
            //    input.ImportEndDateStr = input.StartImportDate.Value.AddDays(3).ToString("yyyy-MM-dd");
            //}
            //else if (input.EndImportDate.HasValue && !input.StartImportDate.HasValue)
            //{
            //    // Nếu có EndImportDate nhưng không có StartImportDate, lấy EndImportDate - 3 ngày
            //    input.ImportStartDateStr = input.EndImportDate.Value.AddDays(-3).ToString("yyyy-MM-dd");
            //}
            //else
            //{
            //    // Nếu không có cả StartImportDate và EndImportDate, lấy ngày hiện tại - 3 ngày làm StartImportDate
            //    input.ImportStartDateStr = DateTime.Now.ToString("yyyy-MM-dd");
            //    input.ImportStartDateStr = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            //}

            //// Chuyển đổi StartImportDate và EndImportDate thành chuỗi định dạng "yyyy-MM-dd"
            //input.ImportStartDateStr = input.StartImportDate?.ToString("yyyy-MM-dd");
            //input.ImportEndDateStr = input.EndImportDate?.ToString("yyyy-MM-dd");

            //// Xử lý khoảng cách giữa StartExportDate và EndExportDate
            //if (input.EndExportDate.HasValue && input.StartExportDate.HasValue)
            //{
            //    var dateDifference = (input.EndExportDate.Value - input.StartExportDate.Value).TotalDays;
            //    if (dateDifference > maxDayQuery)
            //    {
            //        // Nếu khoảng cách lớn hơn maxDayQuery, lấy EndExportDate làm mốc
            //        input.ExportEndDateStr = input.EndExportDate.Value.AddDays(-maxDayQuery).ToString("yyyy-MM-dd");
            //    }
            //}
            //else if (!input.EndExportDate.HasValue && input.StartExportDate.HasValue)
            //{
            //    // Nếu không có EndExportDate, lấy StartExportDate + 3 ngày
            //    input.ExportEndDateStr = input.StartExportDate.Value.AddDays(3).ToString("yyyy-MM-dd");
            //}
            //else if (input.EndExportDate.HasValue && !input.StartExportDate.HasValue)
            //{
            //    // Nếu có EndExportDate nhưng không có StartExportDate, lấy EndExportDate - 3 ngày
            //    input.ExportEndDateStr = input.EndExportDate.Value.AddDays(-3).ToString("yyyy-MM-dd");
            //}
            //else
            //{
            //    // Nếu không có cả StartExportDate và EndExportDate, lấy ngày hiện tại - 3 ngày làm StartExportDate
            //    input.ExportEndDateStr = DateTime.Now.ToString("yyyy-MM-dd");
            //    input.ExportStartDateStr = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
            //}

            //// Chuyển đổi StartExportDate và EndExportDate thành chuỗi định dạng "yyyy-MM-dd"
            //input.ExportStartDateStr = input.StartExportDate?.ToString("yyyy-MM-dd");
            //input.ExportEndDateStr = input.EndExportDate?.ToString("yyyy-MM-dd");

            #endregion
            
            // kiểm tra input date đều null thì set StartCreateDate và EndCreateDate là 3 ngày gần nhất
            //if (input.StartCreateDate == null 
            //    && input.EndCreateDate == null 
            //    && input.StartExportDate == null 
            //    && input.EndExportDate == null 
            //    && input.StartDeliveryDate == null 
            //    && input.EndDeliveryDate == null)
            //{
            //    input.CreateStartDateStr = DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy");
            //}

            // chỉ lấy danh sách trong vòng 7 ngày

            var packages = await _orderService.GetAllPackageByMyOrders(input);

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT",
                    "Khách hàng",
                    "Mã vận đơn gốc",
                    "Mã tra cứu",
                    "Mã bao",
                    "Mã kiện",
                    "Cân nặng bì (kg)",
                    "Line VC",
                    "Trạng thái",
                    "Cân nặng (kg)",
                    "Thể tích",
                    "Giá cước",
                    "Phí đóng gỗ",
                    "Phí quấn bọt khí",
                    "Phí ship nội địa",
                    "Bảo hiểm",
                    "Tổng chi phí",
                    "Ngày tạo",
                    "Ngày xuất kho TQ",
                    "Ngày nhập kho VN",

                };

                // Ghi tiêu đề vào hàng đầu tiên
                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < 16; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                }

                // Merge các cột ngày và giờ
                worksheet.Range(1, 18, 1, 19).Merge();
                worksheet.Range(1, 20, 1, 21).Merge();
                worksheet.Range(1, 22, 1, 23).Merge();


                worksheet.Cell(1, 17).Value = headers[16];
                worksheet.Cell(1, 17).Style.Font.Bold = true;
                worksheet.Cell(1, 17).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c9daf8");
                worksheet.Cell(1, 17).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 17).Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

                worksheet.Cell(1, 19).Value = headers[17];
                worksheet.Cell(1, 19).Style.Font.Bold = true;
                worksheet.Cell(1, 19).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c9daf8");
                worksheet.Cell(1, 19).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 19).Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;


                worksheet.Cell(1, 21).Value = headers[18];
                worksheet.Cell(1, 21).Style.Font.Bold = true;
                worksheet.Cell(1, 21).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c9daf8");
                worksheet.Cell(1, 21).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 21).Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;


                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 2;
                int index = 1;
                foreach (var p in packages)
                {
                    worksheet.Cell(currentRow, 1).Value = index++;
                    worksheet.Cell(currentRow, 2).Value = p.CustomerName; // Khách hàng
                    worksheet.Cell(currentRow, 3).Value = p.WaybillNumber; // Mã vận đơn
                    worksheet.Cell(currentRow, 4).Value = p.TrackingNumber; // Mã vận đơn
                    worksheet.Cell(currentRow, 5).Value = p.BagNumber; // Mã bao
                    worksheet.Cell(currentRow, 6).Value = p.PackageNumber; // Mã kiện
                    worksheet.Cell(currentRow, 7).Value = p.WeightCover;
                    worksheet.Cell(currentRow, 8).Value = ((LineShipping)p.ShippingLineId).GetDescription();
                    worksheet.Cell(currentRow, 9).Value = ((PackageDeliveryStatusEnum)p.ShippingStatus).GetDescription();

                    worksheet.Cell(currentRow, 10).Value = p.Weight;
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = p.Weight % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 11).Value = p.Volume;
                    worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = p.Volume % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Phí vận chuyển
                    worksheet.Cell(currentRow, 12).Value = p.TotalFee;
                    worksheet.Cell(currentRow, 12).Style.NumberFormat.Format = p.TotalFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    // Phí đóng gỗ
                    worksheet.Cell(currentRow, 13).Value = p.WoodenPackagingFee;
                    worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = p.WoodenPackagingFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Phí chống sốc
                    worksheet.Cell(currentRow, 14).Value = p.ShockproofFee;
                    worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = p.ShockproofFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Phí ship nội địa
                    worksheet.Cell(currentRow, 15).Value = p.DomesticShippingFee;
                    worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = p.DomesticShippingFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    // Bảo hiểm
                    worksheet.Cell(currentRow, 16).Value = p.InsuranceFee;
                    worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = p.InsuranceFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Tổng phí
                    worksheet.Cell(currentRow, 17).Value = p.TotalPrice;
                    worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = p.TotalPrice % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 18).Value = p.CreationTime.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(currentRow, 19).Value = p.CreationTime.ToString("HH:mm") ?? "";
                    worksheet.Cell(currentRow, 19).Style.NumberFormat.Format = "HH:mm";

                    worksheet.Cell(currentRow, 20).Value = p.ExportDate?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(currentRow, 20).Style.NumberFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(currentRow, 21).Value = p.ExportDate?.ToString("HH:mm") ?? "";
                    worksheet.Cell(currentRow, 21).Style.NumberFormat.Format = "HH:mm";

                    worksheet.Cell(currentRow, 22).Value = p.ImportDate?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(currentRow, 22).Style.NumberFormat.Format = "dd/MM/yyyy";
                    worksheet.Cell(currentRow, 23).Value = p.ImportDate?.ToString("HH:mm") ?? "";
                    worksheet.Cell(currentRow, 23).Style.NumberFormat.Format = "HH:mm";

                    currentRow++;
                }

                // Định dạng bảng
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length + 3);
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
                    var fileName = $"Packages_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> ExportMyOrder(PagedMyOrderRequestDto input)
        {
            input.SkipCount = 0; // Đặt SkipCount về 0 để lấy tất cả các bản ghi
            input.MaxResultCount = int.MaxValue; // Đặt MaxResultCount về giá trị lớn nhất để lấy tất cả các bản ghi
            input.IsExportExcel = true;
            
            // kiểm tra input date đều null thì set StartCreateDate và EndCreateDate là 3 ngày gần nhất
            if (input.StartCreateDate == null
                && input.EndCreateDate == null
                && input.StartExportDate == null
                && input.EndExportDate == null
                && input.StartDeliveryDate == null
                && input.EndDeliveryDate == null)
            {
                input.StartCreateDate = DateTime.Now.AddDays(-3);
            }

            var result = await _orderService.GetAllMyOrders(input);
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT",
                    "Khách hàng",
                    "Mã vận đơn",
                    "Mã bao",
                    "Cân nặng bì (kg)",
                    "Line VC",
                    "Trạng thái",
                    "Cân nặng (kg)",
                    "Thể tích",
                    "Giá cước",
                    "Phí đóng gỗ",
                    "Phí quấn bọt khí",
                    "Phí ship nội địa",
                    "Bảo hiểm",
                    "Tổng chi phí",
                    "Ngày tạo",
                    "",
                    "Ngày xuất kho TQ",
                    "",
                    "Ngày nhập kho VN",
                    "",
                };
                worksheet.Column("C").Width = 50;
                worksheet.Column("D").Width = 70;
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
                foreach (var p in result.Items)
                {
                    worksheet.Cell(currentRow, 1).Value = index++; // Khách hàng
                    worksheet.Cell(currentRow, 2).Value = p.CustomerName; // Khách hàng
                    worksheet.Cell(currentRow, 3).Value = p.WaybillNumber; // Mã vận đơn
                    worksheet.Cell(currentRow, 4).Value = p.BagNumbers; // Mã bao
                    
                    worksheet.Cell(currentRow, 5).Value = p.WeightCover;
                    worksheet.Cell(currentRow, 6).Value = ((LineShipping)p.ShippingLine).GetDescription();
                    worksheet.Cell(currentRow, 7).Value = ((OrderStatus)p.ShippingStatus).GetDescription();

                    worksheet.Cell(currentRow, 8).Value = p.Weight ?? 0;
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = p.TotalWeight % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 9).Value = p.Volume ?? 0;
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = p.Dimension % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // giá vận chuyển
                    worksheet.Cell(currentRow, 10).Value = p.TotalFee ?? 0;
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = p.UnitPrice % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    //// Phí đóng gỗ
                    worksheet.Cell(currentRow, 11).Value = p.WoodenPackagingFee ?? 0;
                    worksheet.Cell(currentRow, 11).Style.NumberFormat.Format = p.WoodenPackagingFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Phí chống sốc
                    worksheet.Cell(currentRow, 12).Value = p.ShockproofFee ?? 0;
                    worksheet.Cell(currentRow, 12).Style.NumberFormat.Format = p.BubbleWrapFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Phí ship nội địa
                    worksheet.Cell(currentRow, 13).Value = p.DomesticShipping;
                    worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = p.DomesticShipping % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Bảo hiểm
                    worksheet.Cell(currentRow, 14).Value = p.InsuranceFee ?? 0;
                    worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = p.Insurance % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Tổng chi phí
                    worksheet.Cell(currentRow, 15).Value = p.TotalPrice;
                    worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = p.TotalCost % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Ngày tạo
                    worksheet.Cell(currentRow, 16).Value = p.CreationTime.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(currentRow, 17).Value = p.CreationTime.ToString("HH:mm") ?? "";
                    // Ngày xuất kho TQ
                    worksheet.Cell(currentRow, 18).Value = p.InTransitTime?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(currentRow, 19).Value = p.InTransitTime?.ToString("HH:mm") ?? "";
                    // Ngày nhập kho VN
                    worksheet.Cell(currentRow, 20).Value = p.InTransitToVietnamWarehouseTime?.ToString("dd/MM/yyy") ?? "";
                    worksheet.Cell(currentRow, 21).Value = p.InTransitToVietnamWarehouseTime?.ToString("HH:mm") ?? "";

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
                    var fileName = $"Packages_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> Create(string waybillNumber = "")
        {
            //var customerId = _pbtAppSession.CustomerId;
            //   var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("DH");
            var model = new CreateOrderModel()
            {
                Dto = new OrderDto()
                {
                    WaybillNumber = waybillNumber,
                },
                Customers = await _customerService.GetCustomersByCurrentUserAsync(),
                // WaybillCode = waybillCode
            };
            return View(model);
        }

        public async Task<IActionResult> Import()
        {
            return View();
        }

        public async Task<IActionResult> CreateMyOrder(string waybillCode = "")
        {
            var customerId = _pbtAppSession.CustomerId;
            var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("DH");
            var model = new CreateMyOrderModel()
            {
                Dto = new CreateMyOrderDto()
                {
                    CustomerId = customerId.Value,
                    OrderNumber = identityCode.Code,
                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
                WaybillCode = waybillCode
            };
            return View(model);
        }


        public async Task<IActionResult> CreateCustomerOrder(string waybillCode = "")
        {

            var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("DH");

            var model = new CreateCustomerOrderModel()
            {
                Dto = new CreateUpdateOrderDto()
                {
                    OrderNumber = identityCode.Code,
                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                WaybillCode = waybillCode,
            };
            return View(model);
        }

        public async Task<IActionResult> CreateOrderBySale()
        {
            var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("DH");
            var model = new CreateCustomerOrderModel()
            {
                Dto = new CreateUpdateOrderDto()
                {
                    OrderNumber = identityCode.Code,
                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                //  Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
                Customers = await _customerService.GetBySale(AbpSession.UserId.Value) //!customerId.HasValue ? null : await _customerService.GetChildren(customerId.Value)

            };
            return View(model);
        }


        public async Task<IActionResult> GetAddressByCustomerId(long customerId)
        {
            var data = await _customerAddressService.GetByCustomerId(customerId);
            return PartialView("_AddressByCustomerId", data);
        }

        public async Task<IActionResult> Detail(long id)
        {
            var dto = await _orderService.GetAsync(new EntityDto<long>(id));
            //  var waybill = await _waybillAppService.GetByCode(dto.WaybillNumber);
            //OrderDto parrentOrderDto = await _orderService.GetByWaybillAsync(dto.WaybillNumber);
            if (dto != null)
            {
                OrderDto parentOrderDto = await _orderService.GetAsync(new EntityDto<long>(dto.ParentId.HasValue ? dto.ParentId ?? 0 : dto.Id));

                CustomerAddressDto customerAddress = null;
                if (dto.AddressId > 0)
                {
                    customerAddress = await _customerAddressService.GetAsync(new EntityDto<long>(dto.AddressId));
                }

                // var packages = await _packageAppService.GetPackagesByOrderIds(childOrderIds);
                var packages = await _packageAppService.GetPackagesByOrderId(id);
                var complaints = await _complaintAppService.GetByOrderId(id);
                var histories = await _orderHistoryAppService.GetByOrderId(id);
                var logs = await _orderLogAppService.GetByOrderId(id);
                // lấy OrderStatus là OrderStatus nhỏ nhất của packages.order
                var orderStatus = (int)OrderStatus.OrderCompleted;
                if (packages != null && packages.Count > 0)
                {
                    foreach (var package in packages)
                    {
                        var order = _orderService.GetAsync(new EntityDto<long>(package.OrderId)).Result;
                        if (order.OrderStatus < orderStatus) orderStatus = order.OrderStatus;
                    }
                }
                else
                {
                    orderStatus = (int)OrderStatus.New;
                }
                dto.OrderStatus = orderStatus;

                var model = new OrderDetailModel()
                {
                    Dto = dto,
                    CustomerAddress = customerAddress,
                    Packages = packages,
                    Complaints = complaints,
                    Histories = histories,
                    Logs = logs,
                    // PackageDto = packageDto,
                    ParentOrder = parentOrderDto,
                };
                return View(model);
            }
            return View(null);

        }

        public async Task<IActionResult> Edit(long id)
        {
            var dto = await _orderService.GetAsync(new EntityDto<long>(id));
            CustomerDto customerDto = null;

            //get current customer login
            var currentCustomerId = _pbtAppSession.CustomerId;

            // get current permission


            if (dto.CustomerId != null && dto.CustomerId > 0)
            {
                customerDto = await _customerService.GetAsync(new EntityDto<long>(dto.CustomerId.Value));
            }

            dto.IsCustomerOrder = dto.CustomerId == currentCustomerId;
            var customerAddress = await _customerAddressService.GetAsync(new EntityDto<long>(dto.AddressId));
            var model = new EditOrderModel()
            {
                Dto = dto,
                CustomerAddress = customerAddress,
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                CustomerName = customerDto?.Username
            };
            return View(model);
        }

        public async Task<IActionResult> GetPackageList(long id)
        {
            var dto = await _orderService.GetAsync(new EntityDto<long>(id));
            if (dto != null)
            {
                var packages = await _packageAppService.GetListWithBagInfoByOrder(id);

                return PartialView("_PackageList", packages);
            }
            return PartialView("_PackageList", null);

        }
    }
}
