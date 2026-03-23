using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.Controllers;
using pbt.CustomerAddresss;
using pbt.Orders;
using pbt.Orders.Dto;
using pbt.Warehouses;
using pbt.Web.Models.Orders;
using System.Threading.Tasks;
using pbt.Customers;
using Abp.Application.Services.Dto;
using pbt.Packages;
using pbt.Complaints;
using pbt.OrderHistories;
using pbt.OrderLogs;
using Abp.AspNetCore.Mvc.Authorization;
using pbt.ApplicationUtils;
using pbt.Web.Models.SaleAdmin;
using pbt.CustomerAddresss.Dto;
using pbt.OrderNumbers;
using pbt.ShippingPartners;
using pbt.Bags.Dto;
using pbt.Bags;
using ClosedXML.Excel;
using System.IO;
using System;
using pbt.Packages.Dto;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Customers)]
    [AbpMvcAuthorize]
    public class SaleAdminController : pbtControllerBase
    {
        private readonly IOrderAppService _orderService;
        private readonly IWarehouseAppService _warehouseService;
        private readonly ICustomerAddressAppService _customerAddressService;
        private readonly IPackageAppService _packageAppService;
        private readonly IComplaintAppService _complaintAppService;
        private readonly IOrderHistoryAppService _orderHistoryAppService;
        private readonly IOrderLogAppService _orderLogAppService;
        private readonly ICustomerAppService _customerService;
        private readonly pbtAppSession _pbtAppSession;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IShippingPartnerAppService _shippingPartnerAppService;
        private readonly IBagAppService _bagService;


        public SaleAdminController(IOrderAppService orderService,
                IWarehouseAppService warehouseService,
                ICustomerAddressAppService customerAddressService,
                ICustomerAppService customerService,
                IPackageAppService packageAppService,
                IComplaintAppService complaintAppService,

                IOrderHistoryAppService orderHistoryAppService,
                IOrderLogAppService orderLogAppService,
                IIdentityCodeAppService identityCodeAppService,
                IShippingPartnerAppService shippingPartnerAppService,
                IBagAppService bagService,

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
            _shippingPartnerAppService = shippingPartnerAppService;
            _bagService = bagService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SaleAdminIndexModel()
            {
                ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0)
            };
            return View(model);
        }

        public async Task<IActionResult> BagWithPartner()
        {
            var model = new SaleAdminIndexModel()
            {
                ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0)
            };
            return View(model);
        }

        public async Task<IActionResult> BagWithPartnerDownload(PagedBagResultRequestDto input)
        {
            input.MaxResultCount = int.MaxValue;
            input.SkipCount = 0; // Đặt SkipCount về 0 để lấy tất cả dữ liệu
            input.ExcludeCoverWeight = false;
            // kiểm tra input date đều null thì set StartCreateDate và EndCreateDate là 3 ngày gần nhất
            if (input.StartCreateDate == null
                && input.EndCreateDate == null
                && input.StartExportDate == null
                && input.EndExportDate == null
                && input.StartImportDate == null
                && input.EndImportDate == null
                && input.StartExportDateVN == null
                && input.EndExportDateVN == null
               )
            {
                input.StartCreateDate = DateTime.Now.AddDays(-3);
            }

            var result = await _bagService.GetBagsForPartnerAsync(input);


            var bags = result.Items;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Bag Export");

                // Tiêu đề cột
                // Đảm bảo bạn đã using ClosedXML.Excel;

                // --- 1. Sửa Tiêu đề Cột (Headers) ---
                var headers = new[]
                {
                    // Ngày xuất kho TQ (Tách thành 2 cột)
                    "Ngày Xuất TQ",
                    "Giờ Xuất TQ",
    
                    // Ngày nhập kho VN (Tách thành 2 cột)
                    "Ngày Nhập VN",
                    "Giờ Nhập VN",
    
                    // Ngày xuất kho VN (Tách thành 2 cột)
                    "Ngày Xuất VN",
                    "Giờ Xuất VN",

                    "Người nhận",
                    "Mã bao",
                    "Cân nặng (kg)",
                    "Kích thước (m³)",
                    "Số kiện",
                    "Đối tác vận chuyển",
                    "Đặc điểm",
                    "Ghi chú"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // --- 2. Ghi Dữ liệu (Điều chỉnh Index Cột) ---
                int currentRow = 2;
                foreach (var bag in bags)
                {
                    // *** Các cột Ngày/Giờ (Tách và Định dạng) ***

                    // Ngày xuất kho TQ (Cột 1: Ngày, Cột 2: Giờ)
                    if (bag.ExportDateCN.HasValue)
                    {
                        // Cột 1: Ngày
                        worksheet.Cell(currentRow, 1).Value = bag.ExportDateCN.Value;
                        worksheet.Cell(currentRow, 1).Style.NumberFormat.Format = "dd/MM/yyyy";

                        // Cột 2: Giờ
                        worksheet.Cell(currentRow, 2).Value = bag.ExportDateCN.Value;
                        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "HH:mm:ss";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 1).Value = "";
                        worksheet.Cell(currentRow, 2).Value = "";
                    }

                    // Ngày nhập kho VN (Cột 3: Ngày, Cột 4: Giờ)
                    if (bag.ImportDateHN.HasValue)
                    {
                        // Cột 3: Ngày
                        worksheet.Cell(currentRow, 3).Value = bag.ImportDateHN.Value;
                        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "dd/MM/yyyy";

                        // Cột 4: Giờ
                        worksheet.Cell(currentRow, 4).Value = bag.ImportDateHN.Value;
                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "HH:mm:ss";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 3).Value = "";
                        worksheet.Cell(currentRow, 4).Value = "";
                    }

                    // Ngày xuất kho VN (Cột 5: Ngày, Cột 6: Giờ)
                    if (bag.ExportDateVN.HasValue)
                    {
                        // Cột 5: Ngày
                        worksheet.Cell(currentRow, 5).Value = bag.ExportDateVN.Value;
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "dd/MM/yyyy";

                        // Cột 6: Giờ
                        worksheet.Cell(currentRow, 6).Value = bag.ExportDateVN.Value;
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "HH:mm:ss";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 5).Value = "";
                        worksheet.Cell(currentRow, 6).Value = "";
                    }

                    // *** Các cột khác (Đã điều chỉnh index cột) ***

                    // Cột 7: Người nhận
                    worksheet.Cell(currentRow, 7).Value = bag.Receiver ?? "";

                    // Cột 8: Mã bao
                    worksheet.Cell(currentRow, 8).Value = bag.BagCode ?? "";

                    // Cột 9: Cân nặng (gán số + định dạng)
                    worksheet.Cell(currentRow, 9).Value = bag.Weight ?? 0;
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0.00";

                    // Cột 10: Kích thước (gán số + định dạng)
                    worksheet.Cell(currentRow, 10).Value = bag.Volume ?? 0;
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.000";

                    // Cột 11: Số kiện (gán số)
                    worksheet.Cell(currentRow, 11).Value = bag.TotalPackages ?? 0;

                    // Cột 12: Đối tác vận chuyển
                    worksheet.Cell(currentRow, 12).Value = bag.ShippingPartnerName ?? "";

                    // Cột 13: Đặc điểm
                    worksheet.Cell(currentRow, 13).Value = bag.Characteristic ?? "";

                    // Cột 14: Ghi chú
                    worksheet.Cell(currentRow, 14).Value = bag.Note ?? "";

                    currentRow++;
                }



                // Căn lề và định dạng bảng
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                // Xuất file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"BagExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }


        public async Task<IActionResult> ImportExport()
        {
            var model = new SaleAdminImportExportModel()
            {
                ShippingPartnersIntern = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0),
                ShippingPartnersDomestic = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(1),
                // Customers = await _customerService.GetCustomersByCurrentUserAsync()
            };
            return View(model);
        }

        public async Task<IActionResult> ImportExportDownload(ImportExportWithBagRequestDto input)
        {
            input.MaxResultCount = int.MaxValue;
            input.SkipCount = 0; // Đặt SkipCount về 0 để lấy tất cả dữ liệu
            input.ExcludeCoverWeight = false;
            // kiểm tra input date đều null thì set StartCreateDate và EndCreateDate là 3 ngày gần nhất
            if (
                input.StartExportDate == null
                && input.EndExportDate == null
                && input.StartImportDate == null
                && input.EndImportDate == null
                && input.StartExportVNDate == null
                && input.EndExportVNDate == null
                && input.StartCreateDate == null
                && input.EndCreateDate == null
            )
            {
                input.StartCreateDate = DateTime.Now.AddDays(-3);
            }

            var data = await _packageAppService.GetPackageImportExportWithBagAsync(input);
            var items = data.Items;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ImportExport");

                // Header
                var headers = new[]
                {
                    "Ngày xuất kho TQ",         // 1
                    "Người nhận",               // 2
                    "Mã bao/kiện",              // 3
                    "Tổng cân nặng kiện (kg)",  // 4
                    "Thể tích (m³)",            // 5
                    "Số kiện",                  // 6
                    "Đối tác vận chuyển",       // 7
                    "Đặc tính",                 // 8
                    "Ngày nhập kho VN",         // 9
                    "Giá cước",                 // 10 - Shipping fee
                    "Phí ship TQ",              // 11
                    "Phí gia cố",               // 12
                    "Phí ship VN",              // 13
                    "Tổng phí",                 // 14
                    "Giá gốc",                  // 15
                    "Ship VN chịu",             // 16
                    "Đối tác xuất",             // 17
                    "Ngày xuất kho VN"          // 18
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                int row = 2;
                foreach (var item in items)
                {
                    worksheet.Cell(row, 1).Value = item.ExportDate?.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 2).Value = item.CustomerName;
                    worksheet.Cell(row, 3).Value = item.BagType == (int)BagTypeEnum.SeparateBag ? item.BagNumber : item.PackageCode;  //string.IsNullOrWhiteSpace(item.PackageCode) ? item.BagNumber : item.PackageCode;
                    worksheet.Cell(row, 4).Value = item.Weight ?? 0;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(row, 5).Value = item.Dimension;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.000";

                    worksheet.Cell(row, 6).Value = item.PackageCount;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

                    worksheet.Cell(row, 7).Value = item.ShippingPartner ?? "";
                    worksheet.Cell(row, 8).Value = item.Characteristic ?? "";
                    worksheet.Cell(row, 9).Value = item.ImportDate?.ToString("dd/MM/yyyy");

                    worksheet.Cell(row, 10).Value = item.UnitPrice;
                    worksheet.Cell(row, 11).Value = item.ShippingFeeCN;
                    worksheet.Cell(row, 12).Value = item.SecuringCost;
                    worksheet.Cell(row, 13).Value = item.ShippingFeeVN;
                    worksheet.Cell(row, 14).Value = item.TotalFee;
                    worksheet.Cell(row, 15).Value = item.OriginShippingCost;
                    worksheet.Cell(row, 16).Value = item.ShippingFeeAbsorbedByWarehouse;

                    for (int col = 10; col <= 16; col++)
                    {
                        worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }

                    worksheet.Cell(row, 17).Value = item.ShippingPartnerVN;
                    worksheet.Cell(row, 18).Value = item.ExportDateVN?.ToString("dd/MM/yyyy");

                    row++;
                }

                // Format bảng
                var range = worksheet.Range(1, 1, row - 1, headers.Length);
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Columns().AdjustToContents();

                // Trả file về
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"ImportExport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }

        public async Task<IActionResult> BagImportExport()
        {
            var model = new SaleAdminImportExportModel()
            {
                ShippingPartnersIntern = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0),
                ShippingPartnersDomestic = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(1),
                // Customers = await _customerService.GetCustomersByCurrentUserAsync()
            };
            return View(model);
        }


        public async Task<IActionResult> MyOrder()
        {
            var model = new SaleAdminOrderModel() { };
            return View(model);
        }

        public async Task<IActionResult> CustomerDetail(long id)
        {
            var customer = await _customerService.GetAsync(new EntityDto<long>(id));
            CustomerAddressDto address = null;
            if (customer != null && customer.AddressId.HasValue)
            {
                address = await _customerAddressService.GetAsync(new EntityDto<long>(customer.AddressId.Value));
            }
            var model = new CustomerDetailModel()
            {
                Id = id,
                Customer = customer,
                Address = address
            };
            return View(model);
        }


        public async Task<IActionResult> CreateMyOrder()
        {
            var customerId = _pbtAppSession.CustomerId;
            var model = new CreateMyOrderModel()
            {
                Dto = new CreateMyOrderDto()
                {
                    CustomerId = customerId.Value,
                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
            };
            return View(model);
        }


        public async Task<IActionResult> CreateCustomerOrder()
        {
            var customerId = _pbtAppSession.CustomerId;
            var model = new CreateCustomerOrderModel()
            {
                Dto = new CreateUpdateOrderDto()
                {

                },
                WarehousesVietNam = await _warehouseService.GetByCountry((int)WarehouseType.Vn),
                WarehousesChina = await _warehouseService.GetByCountry((int)WarehouseType.Cn),
                //   Addresses = !customerId.HasValue ? null : await _customerAddressService.GetByCustomerId(customerId.Value),
                Customers = !customerId.HasValue ? null : await _customerService.GetChildren(customerId.Value)

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

        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _orderService.GetAsync(new EntityDto<long>(id));
            var customerAddress = await _customerAddressService.GetAsync(new EntityDto<long>(dto.AddressId));
            var packages = await _packageAppService.GetByOrder(id);
            var complaints = await _complaintAppService.GetByOrderId(id);
            var histories = await _orderHistoryAppService.GetByOrderId(id);
            var logs = await _orderLogAppService.GetByOrderId(id);
            var model = new OrderDetailModel()
            {
                Dto = dto,
                CustomerAddress = customerAddress,
                Packages = packages,
                Complaints = complaints,
                Histories = histories,
                Logs = logs
            };
            return View(model);
        }
    }
}
