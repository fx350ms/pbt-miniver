using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pbt.Authorization;
using pbt.ConfigurationSettings;
using pbt.Controllers;
using pbt.Customers;
using pbt.Packages;
using pbt.Packages.Dto;
using pbt.ShippingPartners;
using pbt.ShippingRates;
using pbt.Warehouses;
using pbt.Web.Models.Packages;
using pbt.WoodenPackings;
using System;
using System.IO;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    [Authorize]
    public class PackagesController : pbtControllerBase
    {
        private readonly pbtAppSession _pbtAppSession;
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly ICustomerAppService _customerAppService;
        private readonly IPackageAppService _packageAppService;
        private readonly IProductGroupTypeAppService _productGroupTypeAppService;
        private readonly IConfigurationSettingAppService _configurationSettingAppService;
        private readonly IWoodenPackingService _woodenPackingService;
        private readonly IShippingPartnerAppService _shippingPartner;
        public PackagesController(
            IWarehouseAppService warehouseAppService,
            pbtAppSession pbtAppSession,
            IPackageAppService packageAppService,
            ICustomerAppService customerAppService,
            IProductGroupTypeAppService productGroupTypeAppService,
            IConfigurationSettingAppService configurationSettingAppService,
            IWoodenPackingService woodenPackingService,
            IShippingPartnerAppService shippingPartner
            )
        {
            _warehouseAppService = warehouseAppService;
            _pbtAppSession = pbtAppSession;
            _customerAppService = customerAppService;
            _packageAppService = packageAppService;
            _productGroupTypeAppService = productGroupTypeAppService;
            _configurationSettingAppService = configurationSettingAppService;
            _woodenPackingService = woodenPackingService;
            _shippingPartner = shippingPartner;
        }

        public async Task<IActionResult> Index()
        {
            var warehouseVNs = await _warehouseAppService.GetByCountry(2);
            //ViewBag.WarehousesVietNam = warehouseVNs;
            var model = new PackageIndexViewModel()
            {
                Customers = await _customerAppService.GetFull(),
                WarehouseVNs = warehouseVNs
            };

            return View(model);
        }

        public async Task<IActionResult> Create(string waybillCode)
        {
            var model = new CreateUpdatePackage()
            {
                // WarehousesVietNam = await _warehouseAppService.GetByCountry(2),  
                CustomerSelectListItems = await _customerAppService.GetAllCustomerWithWarehouses(),
                ProductGroupTypes = await _productGroupTypeAppService.GetAllListAsync(),
                WoodenPackingDtos = await _woodenPackingService.GetAllForSelectAsync()
            };

            string key = "ExchangeRateRMB";
            var rsString = await _configurationSettingAppService.GetValueAsync(key);
            long rs = 1;
            if (long.TryParse(rsString, out long result))
            {
                rs = result;
            }
            model.RMBRate = rs;

            if (string.IsNullOrEmpty(waybillCode)) return View(model);
            if (model.Package != null)
            {
                model.Package.TrackingNumber = waybillCode;
            }
            else
            {
                model.Package = new CreateUpdatePackageDto()
                {
                    TrackingNumber = waybillCode
                };
            }

            return View(model);
        }

        public async Task<IActionResult> Pending()
        {
            return View();
        }

        public async Task<IActionResult> Download(PagedPackageResultRequestDto input)
        {
            input.SkipCount = 0; // Đặt SkipCount về 0 để lấy tất cả các bản ghi
            input.MaxResultCount = int.MaxValue; // Đặt MaxResultCount về giá trị lớn nhất để lấy tất cả các bản ghi
            input.ExcludeCoverWeight = true;

            // Lấy danh sách packages dựa trên filter
            var result = await _packageAppService.GetAllPackagesFilterAsync(input);
            var packages = result.Items;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                // Tiêu đề các cột
                var headers = new[]
                {
                     "Khách hàng",
                    "Mã vận đơn",
                    "Số kiện",
                    "Ngày xuất kho TQ",
                    "Ngày xuất kho HN",
                    "Mã bao",
                    "Kích thước (m³)",
                    "Cân nặng (kg)",
                    "Ship NĐ",
                    "Phí gia cố",
                    "Line VC",
                    "Ghi chú",
                    "Mã kiện",
                    "Tên sản phẩm (VN)",
                    "Tên sản phẩm (CN)",
                    "Số lượng",
                    "Giá (VNĐ)",
                    "Giá (TQ)",
                    "Tổng phí",
                    "Dài (cm)",
                    "Rộng (cm)",
                    "Cao (cm)",
                    "Kho",
                    "Trạng thái kho",
                    "Trạng thái VC",
                    "Ngày tạo",
                    "Phí bảo hiểm",
                    "Phí quấn bọt khí",
                    "Phí đóng gỗ",
                    "Giá trị bảo hiểm",
                    "Tổng giá trị",
                    "Đối tác VC",
                    "Loại VC",
                    "Thời gian nhập kho VN",
                    "Thời gian khớp",
                    "Thời gian vào bao",
                    "Kiện lỗi",
                    "Đối tác VC (VN)",
                    "Phí gốc"
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
                foreach (var p in packages)
                {
                    worksheet.Cell(currentRow, 1).Value = p.CustomerName ?? ""; // Khách hàng

                    worksheet.Cell(currentRow, 2).Value = p.TrackingNumber; // Mã vận đơn
                    worksheet.Cell(currentRow, 3).Value = p.Quantity; // Số kiện
                    worksheet.Cell(currentRow, 4).Value = p.ExportDate?.ToString("dd/MM/yyyy HH:mm") ?? ""; // Ngày xuất kho TQ
                    worksheet.Cell(currentRow, 5).Value = p.DeliveryTime?.ToString("dd/MM/yyyy HH:mm") ?? ""; // Ngày xuất kho HN
                    worksheet.Cell(currentRow, 6).Value = p.BagNumber ?? ""; // Mã baoworksheet.Cell(currentRow, 5).Value = p.ProductNameCn ?? ""; // Tên sản phẩm (CN)

                    worksheet.Cell(currentRow, 7).Value = p.Volume ?? 0; // Kích thước (m³)
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = p.Volume.HasValue && p.Volume.Value % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 8).Value = p.Weight ?? 0; // Cân nặng (kg)
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = p.Weight.HasValue && p.Weight.Value % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 9).Value = p.DomesticShippingFee ?? 0; // Ship NĐ
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = p.DomesticShippingFee.HasValue && p.DomesticShippingFee.Value % 1 == 0 ? "#,##0" : "#,##0.00";
                    worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Phí gia cố
                    var shockproofFee = (p.ShockproofFee ?? 0) + (p.WoodenPackagingFee ?? 0); // Phí gia cố
                    worksheet.Cell(currentRow, 10).Value = shockproofFee; // Phí gia cố
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = shockproofFee % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 11).Value = p.ShippingLineString ?? ""; // Line VC
                    worksheet.Cell(currentRow, 12).Value = p.Note ?? ""; // Ghi chú
                    worksheet.Cell(currentRow, 13).Value = p.PackageNumber ?? ""; // Mã kiện
                    worksheet.Cell(currentRow, 14).Value = p.ProductNameVi ?? ""; // Tên sản phẩm (VN)
                    worksheet.Cell(currentRow, 15).Value = p.ProductNameCn ?? ""; // Tên sản phẩm (CN)

                    worksheet.Cell(currentRow, 16).Value = p.Quantity ?? 0; // Số lượng
                    worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = p.Quantity.HasValue && p.Quantity.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 17).Value = p.Price ?? 0; // Giá (VNĐ)
                    worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = p.Price.HasValue && p.Price.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 18).Value = p.PriceCN ?? 0; // Giá (TQ)
                    worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = p.PriceCN.HasValue && p.PriceCN.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 19).Value = p.TotalFee ?? 0; // Tổng phí
                    worksheet.Cell(currentRow, 19).Style.NumberFormat.Format = p.TotalFee.HasValue && p.TotalFee.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 19).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 20).Value = p.Length ?? 0; // Dài (cm)
                    worksheet.Cell(currentRow, 20).Style.NumberFormat.Format = p.Length.HasValue && p.Length.Value % 1 == 0 ? "#,##0" : "#,##0.0";


                    worksheet.Cell(currentRow, 21).Value = p.Width ?? 0; // Rộng (cm)
                    worksheet.Cell(currentRow, 21).Style.NumberFormat.Format = p.Width.HasValue && p.Width.Value % 1 == 0 ? "#,##0" : "#,##0.0";

                    worksheet.Cell(currentRow, 22).Value = p.Height ?? 0; // Cao (cm)
                    worksheet.Cell(currentRow, 22).Style.NumberFormat.Format = p.Height.HasValue && p.Height.Value % 1 == 0 ? "#,##0" : "#,##0.0";

                    worksheet.Cell(currentRow, 23).Value = p.WarehouseName ?? ""; // Kho
                    worksheet.Cell(currentRow, 24).Value = p.WarehouseStatusName ?? ""; // Trạng thái kho
                    worksheet.Cell(currentRow, 25).Value = p.ShippingStatusName ?? ""; // Trạng thái VC
                    worksheet.Cell(currentRow, 26).Value = p.CreationTimeFormat ?? ""; // Ngày tạo

                    worksheet.Cell(currentRow, 27).Value = p.InsuranceFee ?? 0; // Phí bảo hiểm
                    worksheet.Cell(currentRow, 27).Style.NumberFormat.Format = p.InsuranceFee.HasValue && p.InsuranceFee.Value % 1 == 0 ? "#,##0" : "#,##0.0";

                    worksheet.Cell(currentRow, 28).Value = p.ShockproofFee ?? 0; // Phí quấn bọt khí
                    worksheet.Cell(currentRow, 28).Style.NumberFormat.Format = p.ShockproofFee.HasValue && p.ShockproofFee.Value % 1 == 0 ? "#,##0" : "#,##0.0";

                    worksheet.Cell(currentRow, 29).Value = p.WoodenPackagingFee ?? 0; // Phí đóng gỗ
                    worksheet.Cell(currentRow, 29).Style.NumberFormat.Format = p.WoodenPackagingFee.HasValue && p.WoodenPackagingFee.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 30).Value = p.InsuranceValue; // Giá trị bảo hiểm
                                                                             //   worksheet.Cell(currentRow, 30).Style.NumberFormat.Format = p.InsuranceValue.HasValue && p.InsuranceValue.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 31).Value = p.TotalPrice ?? 0; // Tổng giá trị
                    worksheet.Cell(currentRow, 31).Style.NumberFormat.Format = p.TotalPrice.HasValue && p.TotalPrice.Value % 1 == 0 ? "#,##0" : "#,##0.0";

                    worksheet.Cell(currentRow, 32).Value = p.ShippingPartnerName ?? ""; // Đối tác VC
                    worksheet.Cell(currentRow, 33).Value = p.ShippingLineString ?? ""; // Loại VC
                    worksheet.Cell(currentRow, 34).Value = p.ImportDate?.ToString("dd/MM/yyyy HH:mm") ?? ""; // Thời gian nhập kho VN
                    worksheet.Cell(currentRow, 35).Value = p.MatchTimeFormat ?? ""; // Thời gian khớp
                    worksheet.Cell(currentRow, 36).Value = p.BaggingDate?.ToString("dd/MM/yyyy HH:mm") ?? ""; // Thời gian vào bao
                    worksheet.Cell(currentRow, 37).Value = p.IsDefective ? "Có" : "Không"; // Kiện lỗi
                    worksheet.Cell(currentRow, 39).Value = p.OriginShippingCost ?? 0; // Tổng phí goc
                    worksheet.Cell(currentRow, 39).Style.NumberFormat.Format = p.OriginShippingCost.HasValue && p.OriginShippingCost.Value % 1 == 0 ? "#,##0" : "#,##0.0";
                    worksheet.Cell(currentRow, 39).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
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

        /// <summary>
        /// Tải xuống danh sách gói hàng với thông tin tối thiểu
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> DownloadMin(PagedPackageResultRequestDto input)
        {
            input.SkipCount = 0; // Đặt SkipCount về 0 để lấy tất cả các bản ghi
            input.MaxResultCount = int.MaxValue; // Đặt MaxResultCount về giá trị lớn nhất để lấy tất cả các bản ghi
            input.IsExcel = true;
            input.ExcludeCoverWeight = true;

            var result = await _packageAppService.GetAllPackagesFilterAsync(input);
            var packages = result.Items;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                var headers = new[]
                {
                "Username",
                "Mã vận đơn gốc",
                "Mã vận đơn khai báo",
                "Số kiện",
                "Ngày xuất kho TQ",
                "Mã bao",
                "Cân nặng tịnh (Kg)",
                "Kích thước (m³)",
                "SHIP NĐ",
                "Đóng gỗ/Gia cố hàng",
                "Line",
                "Ghi chú",
                "Mã kiện"
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

                int currentRow = 2;
                foreach (var p in packages)
                {
                    worksheet.Cell(currentRow, 1).Value = p.CustomerName ?? "";
                    worksheet.Cell(currentRow, 2).Value = p.WaybillNumber ?? "";
                    worksheet.Cell(currentRow, 3).Value = p.TrackingNumber ?? "";
                    worksheet.Cell(currentRow, 4).Value = p.Quantity ?? 0;

                    worksheet.Cell(currentRow, 5).Value = p.ExportDate?.ToString("dd/MM/yyyy") ?? "";

                    worksheet.Cell(currentRow, 5).Value = p.BagNumber ?? "";

                    worksheet.Cell(currentRow, 7).Value = p.Weight ?? 0;
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(currentRow, 8).Value = p.Volume ?? 0;
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0.000";

                    worksheet.Cell(currentRow, 9).Value = p.DomesticShippingFee ?? 0;
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0.00";

                    // Phí gia cố = Phí quấn bọt khí + đóng gỗ
                    var securingFee = (p.ShockproofFee ?? 0) + (p.WoodenPackagingFee ?? 0);
                    worksheet.Cell(currentRow, 10).Value = securingFee;
                    worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = "#,##0.00";

                    worksheet.Cell(currentRow, 11).Value = p.ShippingLineString ?? "";
                    worksheet.Cell(currentRow, 12).Value = p.Note ?? "";
                    worksheet.Cell(currentRow, 13).Value = p.PackageNumber ?? "";

                    currentRow++;
                }

                // Style range
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Packages_Min_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Load package detail
            var dto = await _packageAppService.GetDetailAsync(id);

            // If package is in a bag (BagId not null and > 0) -> disallow editing and return message
            if (dto?.BagId != null && dto.BagId > 0)
            {
                // Return 400 with a clear message for the caller/UI to display.
                //return BadRequest("bỏ kiện ra khỏi bao rồi sửa");
                RedirectToAction("Error", "Home", new { message = "Kiện hàng đã được đóng vào bao, vui lòng bỏ kiện ra khỏi bao trước khi sửa." });
            }

            string key = "ExchangeRateRMB";
            var rsString = await _configurationSettingAppService.GetValueAsync(key);
            long rs = 1;
            if (long.TryParse(rsString, out long result))
            {
                rs = result;
            }
            var model = new UpdatePackageModel()
            {
                Dto = dto,
                Customers = await _customerAppService.GetFull(),
                ProductGroupTypes = await _productGroupTypeAppService.GetAllListAsync(),
                RMBRate = rs,
                WoodenPackingDtos = await _woodenPackingService.GetAllForSelectAsync()
            };
            return View("Edit", model);
        }


        public async Task<IActionResult> EditByAdmin(int id)
        {
            // Load package detail
            var dto = await _packageAppService.GetForAdminEditByIdAsync(id);

            string key = "ExchangeRateRMB";
            var rsString = await _configurationSettingAppService.GetValueAsync(key);
            long rs = 1;
            if (long.TryParse(rsString, out long result))
            {
                rs = result;
            }
            var model = new EditPackageByAdminModel()
            {
                Dto = dto,
                RMBRate = rs,
                Customers = await _customerAppService.GetFull(),
                ProductGroupTypes = await _productGroupTypeAppService.GetAllListAsync(),
                WoodenPackingDtos = await _woodenPackingService.GetAllForSelectAsync()
            };
            return View("EditByAdmin", model);
        }




        /// <summary>
        /// Chức năng tài chính đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Finance(int id)
        {
            // Load package detail
            var dto = await _packageAppService.GetWithFinanceAsync(id);
            string key = "ExchangeRateRMB";
            var rs = await _configurationSettingAppService.GetValueTAsync<decimal>(key);


            var model = new PackageFinanceModel()
            {
                Dto = dto,
                Customer = await _customerAppService.GetAsync(new EntityDto<long>(dto.CustomerId.Value)),
                ProductGroupType = await _productGroupTypeAppService.GetAsync(new EntityDto<int>(dto.ProductGroupTypeId.Value)),
                CreateWarehouse = await _warehouseAppService.GetAsync(dto.WarehouseCreateId.Value),
                ToWarehouse = await _warehouseAppService.GetAsync(dto.WarehouseDestinationId.Value),
                RMBRate = rs,

                //  WoodenPackingDtos = await _woodenPackingService.GetAllForSelectAsync()
            };
            if (dto.ShippingPartnerId.HasValue && dto.ShippingPartnerId > 0)
            {
                model.ShippingPartner = await _shippingPartner.GetAsync(dto.ShippingPartnerId.Value);
            }
            return View("Finance", model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _packageAppService.GetDetailAsync(id);

            var model = new PackageDetailModel()
            {
                Dto = dto
            };
            return View("Detail", model);
        }

        public async Task<IActionResult> RematchOrder(int id)
        {
            var dto = await _packageAppService.GetAsync(new EntityDto<int>(id));

            var model = new PackageRematchOrderModel()
            {
                Dto = dto
            };

            return View("RematchOrder", model);
        }
    }
}
