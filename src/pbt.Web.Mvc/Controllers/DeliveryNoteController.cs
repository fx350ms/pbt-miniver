using Abp.Application.Services.Dto;
using Abp.Extensions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags;
using pbt.Bags.Dto;
using pbt.Commons.Dto;
using pbt.Controllers;
using pbt.CustomerAddresss;
using pbt.Customers;
using pbt.DeliveryNotes;
using pbt.DeliveryNotes.Dto;
using pbt.DeliveryRequests;
using pbt.Packages;
using pbt.ShippingPartners;
using pbt.Warehouses;
using pbt.Web.Models.DeliveryNote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    //  [AbpMvcAuthorize]
    [Authorize]
    public class DeliveryNoteController : pbtControllerBase
    {
        private readonly pbtAppSession _pbtAppSession;
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly IDeliveryNoteAppService _deliveryNoteAppService;
        private readonly ICustomerAppService _customerAppService;
        private readonly ICustomerAddressAppService _customerAddressAppService;
        private readonly IDeliveryRequestAppService _deliveryRequestAppService;
        private readonly IShippingPartnerAppService _shippingPartnerAppService;
        private readonly IBagAppService _bagAppService;
        private readonly IPackageAppService _packageAppService;

        public DeliveryNoteController(
            IWarehouseAppService warehouseAppService,
            pbtAppSession pbtAppSession,
            ICustomerAddressAppService customerAddressService,
            IDeliveryNoteAppService exportAppService,
            ICustomerAppService customerAppService,
            ICustomerAddressAppService customerAddressAppService,
            IDeliveryRequestAppService deliveryRequestAppService,
            IShippingPartnerAppService shippingPartnerAppService,
            IBagAppService bagAppService,
            IPackageAppService packageAppService
            )
        {
            _warehouseAppService = warehouseAppService;
            _pbtAppSession = pbtAppSession;
            _deliveryNoteAppService = exportAppService;
            _customerAppService = customerAppService;
            _customerAddressAppService = customerAddressService;
            _deliveryRequestAppService = deliveryRequestAppService;
            _shippingPartnerAppService = shippingPartnerAppService;
            _bagAppService = bagAppService;
            _packageAppService = packageAppService;
        }

        public async Task<IActionResult> Index(string filter = "")
        {
            var model = new DeliveryNoteIndexView()
            {
                Status = filter == "pending"  ? 0 : -1,
                Customers = await _customerAppService.GetAllForSelectAsync(),
                ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync((int)ShippingPartnerType.Vietnam)
            };

            return View(model);
        }
        public async Task<IActionResult> MyDeliveryNote()
        {

            var customers = await _customerAppService.GetCustomersByCurrentUserAsync();
            var model = new MyDeliveryNoteIndexViewModel()
            {
                
                Customers = customers.Select( u => new OptionItemDto()
                {
                    id = u.Id.ToString(),
                    text = u.Username
                }).ToList()
            };

            return View(model);
        }


        public async Task<IActionResult> Create(long? customerId, int? deliveryRequestId)
        {
            if (_pbtAppSession.UserId == null) return null;
            var _address = "";
            if (deliveryRequestId != null)
            {
                var deliveryRequest = await _deliveryRequestAppService.GetAsync(new EntityDto<int>((int)deliveryRequestId));
                var address = await _customerAddressAppService.GetAsync(new EntityDto<long>(deliveryRequest.AddressId));
                _address = address.FullAddress;
            }

            var customers = await _customerAppService.GetCustomerListWithFinancialAsync();

            var model = new CreateUpdateDeliveryNote()
            {
                CustomerDto = customerId == null ? null : await _customerAppService.GetAsync(new EntityDto<long>((long)customerId)),
                Customers = customers,
                ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync((int)ShippingPartnerType.Vietnam),
                Address = _address
            };
            return View(model);
        }


        public async Task<IActionResult> CreateQuickDeliveryNote(long customerId, string bagIds, string packageIds)
        {

            var customerDto = await _customerAppService.GetByCustomerIdAsync(customerId);
            var model = new CreateQuickDeliveryNote()
            {
                CustomerDto = customerDto,
                CustomerId = customerDto.Id,
                ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync((int)ShippingPartnerType.Vietnam),
                Items = new List<CreateQuickDeliveryNoteItem>()
            };
            var _bagIds = string.IsNullOrEmpty(bagIds) ? new List<int>() : bagIds.Split(',').Select(int.Parse).ToList();

            if (_bagIds.Count > 0)
            {
                var bags = await _bagAppService.GetByIdsAsync(_bagIds);

                foreach (var item in bags)
                {
                    model.Items.Add(new CreateQuickDeliveryNoteItem()
                    {
                        Id = item.Id,
                        Code = item.BagCode,
                        Type = 1, // Bao
                        TotalWeight = item.Weight ?? 0,
                        Note = item.Note
                    });
                    model.TotalWeight += (item.Weight ?? 0);
                }

            }
            var _packageIds = string.IsNullOrEmpty(packageIds) ? new List<int>() : packageIds.Split(',').Select(int.Parse).ToList();
            if (_packageIds.Count > 0)
            {
                var packages = await _packageAppService.GetByIdsAsync(_packageIds);
               
                if (packages != null && packages.Count > 0)
                {
                    foreach (var item in packages)
                    {
                        model.Items.Add(new CreateQuickDeliveryNoteItem()
                        {
                            Id = item.Id,
                            Code = item.PackageNumber,
                            Type = 2, // Kiện
                            TotalWeight = item.Weight ?? 0,
                            Note = item.Note
                        });
                        model.TotalWeight += (item.Weight ?? 0);
                    }
                }
            }

            return View(model);
        }


        public async Task<IActionResult> ExportExcel(int id)
        {
            // Lấy thông tin phiếu xuất và danh sách bao/kiện
            var deliveryNote = await _deliveryNoteAppService.GetAsync(new EntityDto<int>(id));
            var data = await _deliveryNoteAppService.GetItemByDeliveryNote(id);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(deliveryNote.DeliveryNoteCode);

                // Thêm thông tin phiếu xuất
                worksheet.Cell(1, 1).Value = "Thông tin phiếu xuất";
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                worksheet.Range(1, 1, 1, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(2, 1).Value = "Mã phiếu xuất:";
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 2).Value = deliveryNote.DeliveryNoteCode;
                worksheet.Cell(3, 1).Value = "Ngày xuất:";
                worksheet.Cell(3, 2).Value = deliveryNote.ExportTime?.ToString("dd/MM/yyyy HH:mm:ss");

                worksheet.Cell(4, 1).Value = "Tên khách hàng:";
                worksheet.Cell(4, 2).Value = deliveryNote.Receiver;

                worksheet.Cell(5, 1).Value = "Địa chỉ giao:";
                worksheet.Cell(5, 2).Value = deliveryNote.RecipientAddress;

                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT", "Mã bao/kiện", "Loại", "Cân nặng (kg)", "Tổng số kiện", "Ghi chú"
                };

                // Ghi tiêu đề vào hàng đầu tiên (bắt đầu từ dòng 7)
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(7, i + 1);
                    cell.Value = headers[i];

                    // In đậm và căn giữa tiêu đề
                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // Tô màu nền cho tiêu đề
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 8;
                int index = 1;

                // Ghi dữ liệu bao
                foreach (var bag in data.Bags)
                {
                    worksheet.Cell(currentRow, 1).Value = index++; // STT
                    worksheet.Cell(currentRow, 2).Value = bag.BagCode; // Mã bao
                    worksheet.Cell(currentRow, 3).Value = "Bao"; // Loại
                    worksheet.Cell(currentRow, 4).Value = bag.Weight; // Cân nặng
                    worksheet.Cell(currentRow, 5).Value = bag.TotalPackages; // Tổng số kiện
                    worksheet.Cell(currentRow, 6).Value = bag.Note; // Ghi chú

                    currentRow++;
                }

                // Ghi dữ liệu kiện
                foreach (var package in data.Packages)
                {
                    worksheet.Cell(currentRow, 1).Value = index++; // STT
                    worksheet.Cell(currentRow, 2).Value = package.PackageNumber; // Mã kiện
                    worksheet.Cell(currentRow, 3).Value = "Kiện"; // Loại
                    worksheet.Cell(currentRow, 4).Value = package.Weight; // Cân nặng
                    worksheet.Cell(currentRow, 5).Value = "-"; // Tổng số kiện (không áp dụng cho kiện)
                    worksheet.Cell(currentRow, 6).Value = package.Note; // Ghi chú

                    currentRow++;
                }

                // Định dạng bảng
                var range = worksheet.Range(7, 1, currentRow - 1, headers.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Tự động điều chỉnh kích thước cột
                worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"DeliveryNote_{id}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> ExportExcelFull(int id)
        {
            // Lấy thông tin phiếu xuất và danh sách bao/kiện
            var deliveryNote = await _deliveryNoteAppService.GetAsync(new EntityDto<int>(id));
            var packages = await _packageAppService.GetPackagesByDeliveryNoteIdAsync(id);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(deliveryNote.DeliveryNoteCode);

                // Thêm thông tin phiếu xuất
                worksheet.Cell(1, 1).Value = "Thông tin phiếu xuất";
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                worksheet.Range(1, 1, 1, 6).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(2, 1).Value = "Mã phiếu xuất:";
                worksheet.Cell(2, 1).Style.Font.Bold = true;
                worksheet.Cell(2, 2).Value = deliveryNote.DeliveryNoteCode;
                worksheet.Cell(3, 1).Value = "Ngày xuất:";
                worksheet.Cell(3, 2).Value = deliveryNote.ExportTime?.ToString("dd/MM/yyyy HH:mm:ss");

                worksheet.Cell(4, 1).Value = "Tên khách hàng:";
                worksheet.Cell(4, 2).Value = deliveryNote.Receiver;

                worksheet.Cell(5, 1).Value = "Địa chỉ giao:";
                worksheet.Cell(5, 2).Value = deliveryNote.RecipientAddress;

                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT", "Mã bao", "Mã kiện", "Mã vận đơn gốc","Mã vận đơn khai báo",  "Cân nặng (kg)",  "Ghi chú"
                };

                // Ghi tiêu đề vào hàng đầu tiên (bắt đầu từ dòng 7)
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(7, i + 1);
                    cell.Value = headers[i];

                    // In đậm và căn giữa tiêu đề
                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // Tô màu nền cho tiêu đề
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                }

                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 8;
                int index = 1;


                // Ghi dữ liệu kiện
                foreach (var package in packages)
                {
                    worksheet.Cell(currentRow, 1).Value = index++; // STT
                    worksheet.Cell(currentRow, 2).Value = package.BagNumber; // Mã kiện
                    worksheet.Cell(currentRow, 3).Value = package.PackageNumber; // Mã kiện
                    worksheet.Cell(currentRow, 4).Value = package.WaybillNumber; // Mã tracking
                    worksheet.Cell(currentRow, 5).Value = package.TrackingNumber; // Mã tracking
                    worksheet.Cell(currentRow, 6).Value = package.Weight; // Cân nặng
                    worksheet.Cell(currentRow, 7).Value = package.Note; // Ghi chú

                    currentRow++;
                }

                // Định dạng bảng
                var range = worksheet.Range(7, 1, currentRow - 1, headers.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Tự động điều chỉnh kích thước cột
                worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"DeliveryNote_{id}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> ExportExcelWithFinanceFull(int id)
        {
            var deliveryNote = await _deliveryNoteAppService.GetAsync(new EntityDto<int>(id));
            var packages = await _packageAppService.GetPackagesByDeliveryNoteIdAsync(id);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(deliveryNote.DeliveryNoteCode);
                int currentRow = 1;
                // Thêm thông tin phiếu xuất
                worksheet.Cell(currentRow, 1).Value = "PHIẾU XUẤT KHO";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 20;

                worksheet.Range(currentRow, 1, 3, 11).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Cell(currentRow, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                currentRow = 4;
                worksheet.Cell(currentRow, 1).Value = "Mã phiếu xuất:";
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                // worksheet.Cell(2, 1).Style.Font.Bold = true;

                worksheet.Cell(currentRow, 4).Value = deliveryNote.DeliveryNoteCode;
                worksheet.Range(currentRow, 4, currentRow, 11).Merge();
                worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Ngày xuất:";
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                worksheet.Cell(currentRow, 4).Value = deliveryNote.ExportTime?.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Range(currentRow, 4, currentRow, 11).Merge();


                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Tên khách hàng:";
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                worksheet.Range(currentRow, 4, currentRow, 11).Merge();
                worksheet.Cell(currentRow, 4).Value = deliveryNote.Receiver;

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Địa chỉ giao:";
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                worksheet.Range(currentRow, 4, currentRow, 11).Merge();
                worksheet.Cell(currentRow, 4).Value = deliveryNote.RecipientAddress;

                currentRow++;
                worksheet.Range(currentRow, 1, currentRow +1, 11).Merge();
                worksheet.Cell(currentRow, 1).Value = "DANH SÁCH CHI TIẾT";
                worksheet.Cell(currentRow, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.OliveDrab;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                currentRow++;
                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT", "Mã bao", "Mã kiện", "Mã vận đơn gốc","Mã vận đơn khai báo",  "Cân nặng (kg)","Ship nội địa", "Phí gia cố", "Giá cước", "Thành tiền",  "Ghi chú"
                };

                worksheet.Columns("A").Width = 8; // STT
                worksheet.Columns("B").Width = 18;
                worksheet.Columns("C").Width = 18;
                worksheet.Columns("D").Width = 25;
                worksheet.Columns("E").Width = 25;
                worksheet.Columns("F:I").Width = 15;
                worksheet.Columns("J:K").Width = 25;

                currentRow++;
                // Ghi tiêu đề vào hàng đầu tiên (bắt đầu từ dòng 7)
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(currentRow, i + 1);
                    cell.Value = headers[i];

                    // In đậm và căn giữa tiêu đề
                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // Tô màu nền cho tiêu đề
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;

                }

                


                // Ghi dữ liệu vào các hàng tiếp theo

                int index = 1;
                var totalShippingFee = 0m;
                currentRow++;
                // Ghi dữ liệu kiện
                foreach (var package in packages)
                {
                  
                    worksheet.Cell(currentRow, 1).Value = index++; // STT
                    
                    worksheet.Cell(currentRow, 2).Value = package.BagNumber; // Mã kiện
                    worksheet.Cell(currentRow, 3).Value = package.PackageNumber; // Mã kiện
                    worksheet.Cell(currentRow, 4).Value = package.WaybillNumber; // Mã tracking
                    worksheet.Cell(currentRow, 5).Value = package.TrackingNumber; // Mã tracking
                    worksheet.Cell(currentRow, 6).Value = package.Weight; // Cân nặng
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(currentRow, 7).Value = package.DomesticShippingFee; // Cân nặng
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(currentRow, 8).Value = (package.ShockproofFee ?? 0) + (package.WoodenPackagingFee ?? 0); // Cân nặng
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(currentRow, 9).Value = package.UnitPrice ?? 0; // Cân nặng/ 1kg hoặc 1M3
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(currentRow, 10).Value = package.TotalPrice; // Cân nặn
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 10).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 10).Style.Font.FontSize = 12;
                    worksheet.Cell(currentRow, 10).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                    worksheet.Cell(currentRow, 11).Value = package.Note; // Ghi chú
                    totalShippingFee += (package.TotalPrice ?? 0);

                    if (package.IsRepresentForWeightCover)
                    {
                        worksheet.Row(currentRow).Style.Fill.BackgroundColor = XLColor.LightCyan;
                        worksheet.Cell(currentRow, 11).Value = "BÌ";
                    }

                    currentRow++;
                }
                worksheet.Cell(currentRow, 1).Value = "Tổng tiền cước";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
                worksheet.Range(currentRow, 1, currentRow, 9).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 10).Value = totalShippingFee;//deliveryNote.ShippingFee;
                worksheet.Cell(currentRow, 10).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 10).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 10).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Phí giao hàng";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
                worksheet.Range(currentRow, 1, currentRow, 9).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                var deliveryFee = deliveryNote.DeliveryFeeReason == (int)DeliveryFeeType.WithoutFee ? 0 : (deliveryNote.DeliveryFee ?? 0);

                worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 10).Value = deliveryFee;
                worksheet.Cell(currentRow, 10).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 10).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 10).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Tổng tiền phiếu xuất";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
                worksheet.Range(currentRow, 1, currentRow, 9).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 10).Value = deliveryFee + totalShippingFee;
                worksheet.Cell(currentRow, 10).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 10).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 10).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Phần âm tài chính trước";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
                worksheet.Range(currentRow, 1, currentRow, 9).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 10).Value = deliveryNote.BalanceBefore;
                worksheet.Cell(currentRow, 10).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 10).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 10).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "Phần âm tài chính sau";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 12;
                worksheet.Range(currentRow, 1, currentRow, 9).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(currentRow, 10).Value = (deliveryNote.BalanceAfter ?? 0);
                worksheet.Cell(currentRow, 10).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 10).Style.Font.FontSize = 12;
                worksheet.Cell(currentRow, 10).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                // Định dạng bảng
                var range = worksheet.Range(1, 1, currentRow, headers.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Tự động điều chỉnh kích thước cột
             //   worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"DeliveryNote_{id}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _deliveryNoteAppService.GetWithCreatorInfoAsync(new EntityDto<int>(id));

            var model = new DeliveryNoteDetailModel()
            {
                Dto = dto
            };
            return View("Detail", model);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> LoadDeliveryNoteItem(long customerId)
        {

            var dto = await _deliveryNoteAppService.GetOrCreateByCustomerIdAsync(customerId);

            // Lấy dữ liệu cần thiết cho partial view
            var model = new DeliveryNoteItemViewModel
            {
                CustomerId = customerId,
                // Thêm các dữ liệu khác nếu cần
                DeliveryNote = dto,
                ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(1),
            };

            return PartialView("_DeliveryNoteItem", model);
        }


        public async Task<IActionResult> LoadExportDelivery(DeliveryNoteExportViewInputDto input)
        {
            var data = await _deliveryNoteAppService.GetDeliveryNotesByExportView(input);
            return View("_DeliveryNoteExportViewTable", data);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadExportDelivery(DeliveryNoteExportViewInputDto input)
        {
            var data = await _deliveryNoteAppService.GetDeliveryNotesByExportView(input);

            // Tạo workbook Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("DeliveryNotes");

                // Tiêu đề các cột
                var headers = new[]
                {
                "Mã phiếu xuất", "Ngày xuất", "Người nhận", "Phí giao hàng", "Đối tác vận chuyển",
                "Mã bao/kiện", "Đối tác vận chuyển (kiện)", "Đặc tính", "Tổng cân nặng", "Thể tích",
                "Tổng số kiện", "Ngày nhập VN", "Phí vận chuyển nội địa", "Phí đóng gỗ + chống sốc", "Tổng phí"
            };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 2;
                foreach (var deliveryNote in data)
                {
                    var rowCount = deliveryNote.Items.Count;

                    // Merge các ô cho dữ liệu phiếu xuất (chỉ merge ở dòng đầu tiên của mỗi phiếu xuất)
                    if (rowCount > 1)
                    {
                        worksheet.Range(currentRow, 1, currentRow + rowCount - 1, 1).Merge(); // Mã phiếu xuất
                        worksheet.Range(currentRow, 2, currentRow + rowCount - 1, 2).Merge(); // Ngày xuất
                        worksheet.Range(currentRow, 3, currentRow + rowCount - 1, 3).Merge(); // Người nhận
                        worksheet.Range(currentRow, 4, currentRow + rowCount - 1, 4).Merge(); // Phí giao hàng
                        worksheet.Range(currentRow, 5, currentRow + rowCount - 1, 5).Merge(); // Đối tác vận chuyển
                        worksheet.Range(currentRow, 15, currentRow + rowCount - 1, 15).Merge(); // Tổng phí
                    }

                    for (int i = 0; i < rowCount; i++)
                    {
                        var item = deliveryNote.Items[i];

                        // Ghi dữ liệu phiếu xuất (chỉ ghi ở dòng đầu tiên của mỗi phiếu xuất)
                        if (i == 0)
                        {
                            worksheet.Cell(currentRow, 1).Value = deliveryNote.DeliveryNoteCode;
                            worksheet.Cell(currentRow, 2).Value = deliveryNote.ExportTime?.ToString("dd/MM/yyyy HH:mm");
                            worksheet.Cell(currentRow, 3).Value = deliveryNote.Receiver;
                            worksheet.Cell(currentRow, 4).Value = deliveryNote.DeliveryFee.ToString("N0");
                            worksheet.Cell(currentRow, 5).Value = deliveryNote.ShippingPartnerName;
                        }

                        // Ghi dữ liệu chi tiết kiện
                        worksheet.Cell(currentRow, 6).Value = item.ItemCode;
                        worksheet.Cell(currentRow, 7).Value = item.ShippingPartnerName;
                        worksheet.Cell(currentRow, 8).Value = item.IsSolution ? "Dung dịch" :
                            item.IsWoodSealing ? "Đóng gỗ" :
                            item.IsFakeGoods ? "Hàng giả" :
                            item.IsOtherFeature ? item.OtherReason : "";
                        worksheet.Cell(currentRow, 9).Value = item.TotalAllWeight.ToString("N2");
                        worksheet.Cell(currentRow, 10).Value = item.Volume.ToString("N2");
                        worksheet.Cell(currentRow, 11).Value = item.TotalPackages;
                        worksheet.Cell(currentRow, 12).Value = item.ImportDate?.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell(currentRow, 13).Value = item.DomesticShippingFee.ToString("N0");
                        worksheet.Cell(currentRow, 14).Value = (item.WoodenPackagingFee + item.ShockproofFee).ToString("N0");

                        // Ghi tổng phí (chỉ ghi ở dòng đầu tiên của mỗi phiếu xuất)
                        if (i == 0)
                        {
                            worksheet.Cell(currentRow, 15).Value = deliveryNote.TotalFee.ToString("N0");
                        }

                        currentRow++;
                    }
                }

                // Định dạng bảng
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Tự động điều chỉnh kích thước cột
                worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"DeliveryNotes_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
    }
}
