using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Authorization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags;
using pbt.Bags.Dto;
using pbt.Configuration;
using pbt.ConfigurationSettings;
using pbt.Controllers;
using pbt.Core;
using pbt.CustomerAddresss;
using pbt.Customers;
using pbt.Customers.Dto;
using pbt.Packages;
using pbt.Packages.Dto;
using pbt.ShippingPartners;
using pbt.Warehouses;
using pbt.Web.Models.Bags;
using pbt.Web.Models.Packages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    // [AbpMvcAuthorize]
    [Authorize]
    public class BagsController : pbtControllerBase
    {
        private readonly pbtAppSession _pbtAppSession;
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly IShippingPartnerAppService _shippingPartnerAppService;
        private readonly IBagAppService _bagAppService;
        private readonly IPackageAppService _packageAppService;
        private readonly ICustomerAppService _customerAppService;
        private IConfigurationSettingAppService _configurationSettingAppService;

        public BagsController(
            IWarehouseAppService warehouseAppService,
            pbtAppSession pbtAppSession,
            ICustomerAddressAppService customerAddressService,
            IBagAppService bagAppService,
            IPackageAppService packageAppService,
            IShippingPartnerAppService shippingPartnerAppService,
            ICustomerAppService customerAppService,
            IConfigurationSettingAppService configurationSettingAppService
            )
        {
            _warehouseAppService = warehouseAppService;
            _pbtAppSession = pbtAppSession;
            _bagAppService = bagAppService;
            _packageAppService = packageAppService;
            _shippingPartnerAppService = shippingPartnerAppService;
            _customerAppService = customerAppService;
            _configurationSettingAppService = configurationSettingAppService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new BagIndexViewModel()
            {
                WarehousesCN = await _warehouseAppService.GetByCountry((int)WarehouseType.Cn),
                WarehousesVN = await _warehouseAppService.GetByCountry((int)WarehouseType.Vn),
              //  ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0)
            };

            int permissionChecker = GetPermissionChecker();
            if( permissionChecker ==  1 || permissionChecker == 2 || permissionChecker == 3 || permissionChecker == 5 || permissionChecker == 6)
            {
                model.ShippingPartners = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0);
            }    

            return View(model);
        }

        public async Task<IActionResult> Create(string packagesId)
        {
            List<PackageDetailDto> packageDetailDtos = null;
            var sameCustomer = true;
            long customerId = 0;
            int warehouseVnId = 0;
            int warehouseCnId = 0;
            string receiver = "";
            var shippingType = 0;
            if (!string.IsNullOrEmpty(packagesId))
            {
                string[] packageIds = packagesId.Split(",");
                if (packageIds.Length > 0)
                {
                    packageDetailDtos = await _bagAppService.GetListDetailAsync(packagesId);
                }

                // kiểm tra xem kiện hàng có cùng customer id hay không
                if (packageDetailDtos != null && packageDetailDtos.Count > 0)
                {
                    customerId = packageDetailDtos[0].CustomerId.Value;
                    shippingType = packageDetailDtos[0].ShippingLineId.Value;
                    warehouseVnId = packageDetailDtos[0].WarehouseDestinationId ?? 0;
                    warehouseCnId = packageDetailDtos[0].WarehouseCreateId ?? 0;

                    foreach (var detailDto in packageDetailDtos)
                    {
                        if (customerId != detailDto.CustomerId)
                        {
                            sameCustomer = false;
                            shippingType = 0;
                            receiver = "";
                            break;
                        }
                    }
                }
            }

            if (sameCustomer && customerId > 0)
            {
                var customer = await _customerAppService.GetAsync(new EntityDto<long>(customerId));
                receiver = customer.Username;
            }

            var model = new BagCreateViewModel()
            {
                // WarehousesVietNam = await _warehouseAppService.GetByCountry((int)WarehouseType.Vn),
                PackageDetailDtos = packageDetailDtos,
                customerId = customerId,
                SameCustomer = sameCustomer,
                WarehouseVnId = warehouseVnId,
                WarehouseCnId = warehouseCnId,
                Receiver = receiver,
                ShippingType = shippingType
            };
            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var dto = await _bagAppService.GetAsync(new EntityDto<int>(id));
            if (dto.WarehouseCreateId != null)
            {
                dto.WarehouseCreate = await _warehouseAppService.GetAsync(new EntityDto<int>(dto.WarehouseCreateId ?? 0));
            }
            if (dto.WarehouseDestinationId != null)
            {
                dto.WarehouseDestination = await _warehouseAppService.GetAsync(new EntityDto<int>(dto.WarehouseDestinationId ?? 0));
            }
            if (dto.ShippingPartnerId.HasValue && dto.ShippingPartnerId > 0)
            {
                dto.ShippingPartner = await _shippingPartnerAppService.GetAsync(new EntityDto<int>(dto.ShippingPartnerId.Value));
            }
            dto.ShippingTypeName = ((ShippingType)dto.ShippingType).GetDescription();
            // lấy tổng cân nặng các kiện hàng trong bao
           
            var model = new BagDetailModel()
            {
                Dto = dto
            };
            return View("Detail", model);
        }

        public async Task<IActionResult> Pending()
        {
            return View("Pending");
        }


        [HttpGet]
        public async Task<IActionResult> PrintStamp(int id, bool isExport = true)
        {
            if (id <= 0) return BadRequest("Invalid bag id");

            var dto = await _bagAppService.GetForStampAsync(id, isExport);
            if (dto == null) return NotFound();

            var model = new PrintBagStampModel
            {
                Bag = dto,
                IsExport = isExport
            };

            return View("~/Views/Bags/PrintStamp.cshtml", model);
        }

        public async Task<IActionResult> Bagging(int id)
        {
            var dto = await _bagAppService.GetAsync(new EntityDto<int>(id));
            if (dto.WarehouseCreateId != null)
            {
                dto.WarehouseCreate = await _warehouseAppService.GetAsync(new EntityDto<int>(dto.WarehouseCreateId ?? 0));
            }
            if (dto.WarehouseDestinationId != null)
            {
                dto.WarehouseDestination = await _warehouseAppService.GetAsync(new EntityDto<int>(dto.WarehouseDestinationId ?? 0));
            }
            if (dto.ShippingPartnerId.HasValue && dto.ShippingPartnerId > 0)
            {
                dto.ShippingPartner = await _shippingPartnerAppService.GetAsync(new EntityDto<int>(dto.ShippingPartnerId.Value));
            }
            dto.ShippingTypeName = ((ShippingType)dto.ShippingType).GetDescription();
            if (dto.CustomerId > 0)
                dto.Customer = await _customerAppService.GetAsync(new EntityDto<long>(dto.CustomerId ?? 0));
            dto.WeightPackages = dto.Weight;
            var model = new BagDetailModel()
            {
                Dto = dto
            };
            model.destinationWarehouses = await _warehouseAppService.GetByCountry((int)WarehouseType.Vn);
            model.ShippingPartnerDtos = await _shippingPartnerAppService.GetAllShippingPartnersByLocationAsync(0);
            var bagCoverWeight = await _configurationSettingAppService.GetValueAsync("bag_cover");
            model.BagCoverWeight = decimal.Parse(bagCoverWeight, CultureInfo.InvariantCulture);
            return View("Bagging", model);
        }



        public async Task<IActionResult> Download(BagDownloadFileRequestDto input)
        {
            // Lấy danh sách bao dựa trên input
            input.MaxResultCount = int.MaxValue; // Đặt số lượng tối đa để tải xuống tất cả dữ liệu
            input.SkipCount = 0; // Bỏ qua không có bản ghi nào

            int permissionChecker = GetPermissionChecker();
            var showShippingPartner = false;
            if (permissionChecker == 1 || permissionChecker == 2 || permissionChecker == 3 || permissionChecker == 5 || permissionChecker == 6)
            {
                showShippingPartner = true;
            }

            var bags = await _bagAppService.GetDataForDownload(input);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Bags");

                // Tiêu đề các cột
                var headers = new[]
                {
                    "Mã bao", "Người nhận", "SĐT người nhận", "Loại bao", "Khách hàng/Mã KH", "Thời gian tạo",
                    "Kho tạo", "Kho đích", "Kho hiện tại", "Trạng thái kho hiện tại", "Thời gian xuất kho nguồn",
                    "Thời gian nhập kho đích", "Cân nặng bao (kg)", "Kích thước bao", "D", "R", "C",
                    "Tổng số kiện", "Tổng cân nặng kiện (kg)", "Đặc tính", "Đối tác vận chuyển", "Nhân viên vận chuyển", "Dịch vụ",
                    "Ghi chú"
                 };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];

                    // In đậm và tô màu nền cho header
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 2;
                foreach (var bag in bags.Items)
                {
                    // Các trường không có trong DTO sẽ để trống
                    var receiverPhone = ""; // Không có trong DTO
                    var currentWarehouse = ""; // Không có trong DTO
                    var currentWarehouseStatus = ""; // Không có trong DTO
                    var deliveryStaff = ""; // Không có trong DTO

                    worksheet.Cell(currentRow, 1).Value = bag.BagCode; // Mã bao
                    worksheet.Cell(currentRow, 2).Value = bag.Receiver ?? ""; // Người nhận
                    worksheet.Cell(currentRow, 3).Value = receiverPhone; // SĐT người nhận
                    worksheet.Cell(currentRow, 4).Value = bag.BagTypeName ?? ""; // Loại bao
                    worksheet.Cell(currentRow, 5).Value = bag.BagType == (int)BagTypeEnum.SeparateBag ? (bag.CustomerName ?? "") : ""; // Khách hàng/Mã KH
                    worksheet.Cell(currentRow, 6).Value = bag.CreationTimeFormat; // Thời gian tạo
                    worksheet.Cell(currentRow, 7).Value = bag.WarehouseCreateName; // Kho tạo
                    worksheet.Cell(currentRow, 8).Value = bag.WarehouseDestinationName; // Kho đích

                    worksheet.Cell(currentRow, 9).Value = (bag.ShippingStatus == 3 || bag.ShippingStatus == 5 || bag.ShippingStatus == 6) ? (bag.WarehouseDestinationName) : (bag.WarehouseCreateName); // Kho hiện tại
                                                                                                                                                                                                        //   worksheet.Cell(currentRow, 9).Value = currentWarehouse; // Kho hiện tại 
                    worksheet.Cell(currentRow, 10).Value = ((BagShippingStatus)bag.ShippingStatus).GetDescription();//currentWarehouseStatus; // Trạng thái kho hiện tại
                    worksheet.Cell(currentRow, 11).Value = bag.ExportDate?.ToString("dd/MM/yyyy HH:mm") ?? ""; // Thời gian xuất kho nguồn
                    worksheet.Cell(currentRow, 12).Value = bag.ImportDate?.ToString("dd/MM/yyyy HH:mm") ?? ""; // Thời gian nhập kho đích
                                                                                                               //   worksheet.Cell(currentRow, 13).Value = bag.Weight?.ToExcelNumberFormat(); // Cân nặng bao (kg)
                    worksheet.Cell(currentRow, 13).SetValue(bag.Weight);

                    if ((bag.Weight ?? 0) % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = "#,##0"; // Cân nặng bao (kg)
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = "#,##0.#0"; // Cân nặng bao (kg)
                    }
                    var bagM3 = (bag.Length.HasValue && bag.Width.HasValue && bag.Height.HasValue)
                        ? ((bag.Length.Value * bag.Width.Value * bag.Height.Value) / 1000000M)
                        : 0; // Kích thước bao

                    if (bagM3 % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = "#,##0"; // thể tích bao (kg)
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = "#,##0.00"; // thể tích bao (kg)
                    }

                    worksheet.Cell(currentRow, 14).SetValue(bagM3);
                    worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


                    worksheet.Cell(currentRow, 15).SetValue(bag.Length);
                    if (bag.Length.HasValue && bag.Length.Value % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0"; // Cân nặng bao (kg)
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0.0"; // Cân nặng bao (kg)
                    }

                    worksheet.Cell(currentRow, 15).SetValue(bag.Length);
                    if (bag.Length.HasValue && bag.Length.Value % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0.0";
                    }

                    worksheet.Cell(currentRow, 16).SetValue(bag.Width);
                    if (bag.Width.HasValue && bag.Width.Value % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0.0";
                    }

                    worksheet.Cell(currentRow, 17).SetValue(bag.Height);
                    if (bag.Height.HasValue && bag.Height.Value % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0.0";
                    }

                    worksheet.Cell(currentRow, 18).SetValue(bag.TotalPackages);
                    worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 19).SetValue(bag.WeightPackages.Value); // Tổng cân nặng kiện (kg)

                    if (bag.WeightPackages.Value % 1 == 0)
                    {
                        worksheet.Cell(currentRow, 19).Style.NumberFormat.Format = "#,##0"; // Cân nặng bao (kg)
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 19).Style.NumberFormat.Format = "#,##0.#0"; // Cân nặng bao (kg)
                    }

                    worksheet.Cell(currentRow, 20).Value = bag.IsSolution ? "Dung dịch" : bag.IsWoodSealing ? "Đóng gỗ" : bag.IsFakeGoods ? "Hàng fake" : bag.IsOtherFeature ? bag.otherReason : ""; // Đặc tính
                    worksheet.Cell(currentRow, 21).Value = showShippingPartner ? (bag.ShippingPartnerName ?? "") : ""; // Đối tác vận chuyển
                    worksheet.Cell(currentRow, 22).Value = deliveryStaff; // Nhân viên vận chuyển
                    worksheet.Cell(currentRow, 23).Value = bag.ShippingTypeName ?? ""; // Dịch vụ
                    worksheet.Cell(currentRow, 24).Value = bag.Note ?? ""; // Ghi chú
                    currentRow++;
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
                    var fileName = $"Bags_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }


            }
        }

        public async Task<IActionResult> DownloadPackage(BagDownloadFileRequestDto input)
        {
            // Lấy danh sách bao dựa trên input
            input.MaxResultCount = int.MaxValue; // Đặt số lượng tối đa để tải xuống tất cả dữ liệu
            input.SkipCount = 0; // Bỏ qua không có bản ghi nào
            
        

            var packages = await _packageAppService.GetPackageDownloadByBag(input);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                // Tiêu đề các cột
                var headers = new[]
                {
                    "STT",
                    "MÃ KIỆN",
                    "MÃ BAO",
                    "KHO HIỆN TẠI",
                    "KHO ĐÍCH",
                    "MÃ ĐƠN",
                    "KHÁCH HÀNG",
                    "CÂN NẶNG TỊNH(Kg)",
                    "ĐƠN VỊ DỊCH VỤ",
                    "TRẠNG THÁI",
                    "THỂ TÍCH",
                    "MÃ VẬN ĐƠN GỐC"
                };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];

                    // In đậm và tô màu nền cho header
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 2;
                int index = 1;
                foreach (var package in packages)
                {
                    worksheet.Cell(currentRow, 1).Value = index++; // STT
                    worksheet.Cell(currentRow, 2).Value = package.PackageNumber ?? ""; // MÃ KIỆN
                    worksheet.Cell(currentRow, 3).Value = package.BagNumber ?? ""; // MÃ BAO
                    worksheet.Cell(currentRow, 4).Value = package.CurrentWarehouseName;
                    worksheet.Cell(currentRow, 5).Value = package.TargetWarehouseName;
                  

                  //  worksheet.Cell(currentRow, 5).Value = "HN"; 
                    worksheet.Cell(currentRow, 6).Value = package.TrackingNumber ?? ""; // MÃ ĐƠN
                    worksheet.Cell(currentRow, 7).Value = package.CustomerName ?? ""; // KHÁCH HÀNG
                    worksheet.Cell(currentRow, 8).Value = package.Weight; // CÂN NẶNG TỊNH(Kg)
                    worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "0.00";
                    worksheet.Cell(currentRow, 9).Value = "pbt" ?? ""; // ĐƠN VỊ DỊCH VỤ
                    worksheet.Cell(currentRow, 10).Value = package.ShippingStatusName ?? ""; // TRẠNG THÁI
                    worksheet.Cell(currentRow, 11).Value = package.Volume; // M3
                    worksheet.Cell(currentRow, 12).Value = package.WaybillNumber;
                    currentRow++;
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
                    var fileName = $"Packages_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }

        }
        public async Task<IActionResult> DownloadManifest(BagDownloadFileRequestDto input)
        {
            // Lấy dữ liệu từ bảng Package
            input.MaxResultCount = int.MaxValue;
            input.IsExcel = true;
            //// kiểm tra input date đều null thì set StartCreateDate và EndCreateDate là 3 ngày gần nhất
            //if (input.StartCreateDate == null
            //    && input.EndCreateDate == null
            //    && input.StartExportDate == null
            //    && input.EndExportDate == null
            //    && input.StartImportDate == null
            //    && input.EndImportDate == null
            //   )
            //{
            //    input.StartCreateDate = DateTime.Now.AddDays(-3);
            //}
            
            var packages = await _bagAppService.GetDataManifestForDownload(input);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                // Replace the headers array in DownloadManifest with the following:
                var headers = new[]
                {
                    "ExpBcNo",
                    "订单编号",
                    "总运单号",
                    "快递单号",
                    "发货人",
                    "发货人详细地址",
                    "发货人电话",
                    "收件人",
                    "收件人联系电话",
                    "收件人国别代码",
                    "收件人详细地址",
                    "内件名称",
                    "订单商品总数量",
                    "币制代码",
                    "订单商品总价",
                    "订单总毛重(KG)",
                    "订单总净重(KG)",
                    "商家备案名称",
                    "商家备案号",
                    "商品名称",
                    "品牌",
                    "商品规格",
                    "商品货号",
                    "商品单价",
                    "商品数量",
                    "manifest.product_total_value",
                    "HS编码",
                    "商品序号",
                    "商品计量单位代码",
                    "法定第一计量单位代码",
                    "manifest.legal_second_unit_code",
                    "第一法定单位商品数量",
                    "第二法定单位商品数量",
                    "关区代码",
                    "Package Number",
                    "Order ID",
                    "Bag Code",
                    "Link",
                    "Code Item",
                    "Invoice Code",
                    "Weight",
                    "Id Order Item",
                    "Original Short",
                    "Agency Name",
                    "Properties",
                    "Services",
                    "Status Checking",
                    "最后一里路运单号",
                    "HsCode",
                    "Is Fake Package"
                };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];

                    // In đậm và tô màu nền cho header
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                var fakeCompanyName = await _configurationSettingAppService.GetValueAsync("fake_company");
                var fakeAddress = await _configurationSettingAppService.GetValueAsync("fake_address");
                var fakePhone = await _configurationSettingAppService.GetValueAsync("fake_phone");
                var noneCN = "无";
                // Ghi dữ liệu vào các hàng tiếp theo
                int currentRow = 2;
                foreach (var package in packages.Items)
                {
                    // Khai báo các trường không có trong PackageDto
                    var ExpBcNo = ""; // ExpBcNo

                    var LegalSecondUnitCode = ""; // manifest.legal_second_unit_code
                    var IdOrderItem = ""; // Id Order Item
                    var CodeItem = ""; // Code Item
                    var TranslatedShort = ""; // Translated Short
                    var TiengTrungAI = ""; // Tiếng Trung AI
                    var TiengVietAI = ""; // Tiếng Việt AI


                    // Ghi dữ liệu vào các cột tương ứng
                    worksheet.Cell(currentRow, 1).Value = currentRow; // ExpBcNo
                    worksheet.Cell(currentRow, 2).Value = package.PackageNumber ?? ""; // 订单编号
                    worksheet.Cell(currentRow, 3).Value = ""; // 总运单号
                    worksheet.Cell(currentRow, 4).Value = package.TrackingNumber; // 快递单号
                    worksheet.Cell(currentRow, 5).Value = fakeCompanyName; // 发货人
                    worksheet.Cell(currentRow, 6).Value = fakeAddress; // 发货人详细地址
                    worksheet.Cell(currentRow, 7).Value = fakePhone; // 发货人电话
                    worksheet.Cell(currentRow, 8).Value = package.CustomerFakeName; // 收件人
                    worksheet.Cell(currentRow, 9).Value = package.CustomerFakePhone; // 收件人联系电话
                    worksheet.Cell(currentRow, 10).Value = ""; // 收件人国别代码, Mã quốc gia
                    worksheet.Cell(currentRow, 11).Value = package.CustomerFakeAddress; // 收件人详细地址
                    worksheet.Cell(currentRow, 12).Value = package.ProductNameCn ?? ""; // 内件名称
                                                                                        // var productTotalValueCell = worksheet.Cell(currentRow, 26);

                    var quantityCell = worksheet.Cell(currentRow, 13);
                    quantityCell.SetValue(package.Quantity.HasValue ? package.Quantity.Value : 0);
                    quantityCell.Style.NumberFormat.Format = "#,##0";
                    quantityCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


                    ///   var totalPrice = (package.PriceCN.HasValue ? package.PriceCN.Value : 0) * (package.Quantity.HasValue ? package.Quantity.Value : 0);
                    ///   
                    if (package.PriceCN.HasValue)
                    {
                        worksheet.Cell(currentRow, 15).SetValue(package.PriceCN);
                        worksheet.Cell(currentRow, 26).SetValue(package.PriceCN);
                        if (package.PriceCN.Value % 1 == 0)
                        {
                            worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(currentRow, 26).Style.NumberFormat.Format = "#,##0"; // manifest.product_total_value
                        }

                        else
                        {
                            worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0.0"; //  
                            worksheet.Cell(currentRow, 26).Style.NumberFormat.Format = "#,##0.0"; // manifest.product_total_value
                        }
                        worksheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(currentRow, 26).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        if (package.Quantity.HasValue)
                        {
                            var pricePerProduct = package.PriceCN.Value / package.Quantity.Value;
                            worksheet.Cell(currentRow, 24).SetValue(pricePerProduct);
                            if (pricePerProduct % 1 == 0)
                            {
                                worksheet.Cell(currentRow, 24).Style.NumberFormat.Format = "#,##0"; // manifest.product_total_value
                            }
                            else
                            {
                                worksheet.Cell(currentRow, 24).Style.NumberFormat.Format = "#,##0.0"; // manifest.product_total_value
                            }
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 24).SetValue(0);
                            worksheet.Cell(currentRow, 24).Style.NumberFormat.Format = "#,##0"; // manifest.product_total_value
                        }

                    }

                    // Tổng giá trị sản phẩm

                    if (package.Weight.HasValue)
                    {
                        worksheet.Cell(currentRow, 16).SetValue(package.Weight.Value);
                        worksheet.Cell(currentRow, 17).SetValue(package.Weight.Value);
                        if (package.Weight.Value % 1 == 0)
                        {
                            worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0.#0"; // manifest.order_total_gross_weight
                            worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0.#0"; // manifest.order_total_net_weight
                        }

                    }
                    else
                    {
                        worksheet.Cell(currentRow, 16).SetValue(0);
                        worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 17).SetValue(0);
                        worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "#,##0";
                    }

                    worksheet.Cell(currentRow, 18).Value = ""; // 商家备案名称
                    worksheet.Cell(currentRow, 19).Value = ""; // 商家备案号
                    worksheet.Cell(currentRow, 20).Value = package.ProductNameCn ?? ""; // 商品名称
                    worksheet.Cell(currentRow, 21).Value = noneCN; // 品牌
                    worksheet.Cell(currentRow, 22).Value = noneCN; // 商品规格
                    worksheet.Cell(currentRow, 23).Value = ""; // 商品货号

                    // Cột 25: ProductQuantity
                    var productQuantityCell = worksheet.Cell(currentRow, 25);
                    if (package.Quantity.HasValue)
                    {
                        productQuantityCell.SetValue(package.Quantity.Value);
                        if (package.Quantity.Value % 1 == 0)
                            productQuantityCell.Style.NumberFormat.Format = "#,##0";
                        else
                            productQuantityCell.Style.NumberFormat.Format = "#,##0.0";
                    }
                    else
                    {
                        productQuantityCell.SetValue(0);
                        productQuantityCell.Style.NumberFormat.Format = "#,##0";
                    }
                    productQuantityCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;



                    worksheet.Cell(currentRow, 27).Value = ""; // HS编码
                    worksheet.Cell(currentRow, 28).Value = ""; // 商品序号
                    worksheet.Cell(currentRow, 29).Value = ""; // 商品计量单位代码
                    worksheet.Cell(currentRow, 30).Value = ""; // 法定第一计量单位代码
                    worksheet.Cell(currentRow, 31).Value = ""; // manifest.legal_second_unit_code
                    worksheet.Cell(currentRow, 32).Value = ""; // 第一法定单位商品数量
                    worksheet.Cell(currentRow, 33).Value = ""; // 第二法定单位商品数量
                    worksheet.Cell(currentRow, 34).Value = ""; // 关区代码
                    worksheet.Cell(currentRow, 35).Value = package.PackageNumber ?? ""; // Package Number
                    worksheet.Cell(currentRow, 36).Value = package.OrderId; // Order ID
                    worksheet.Cell(currentRow, 37).Value = package.BagNumber ?? ""; // Bag Code
                    worksheet.Cell(currentRow, 38).Value = ""; // Link

                    var codeItemCell = worksheet.Cell(currentRow, 39);
                    codeItemCell.Value = CodeItem; // Code Item
                    codeItemCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(currentRow, 40).Value = package.PackageNumber ?? "";  
                    if (package.Weight.HasValue)
                    {
                        worksheet.Cell(currentRow, 41).SetValue(package.Weight.Value);
                        if (package.Weight.Value % 1 == 0)
                            worksheet.Cell(currentRow, 41).Style.NumberFormat.Format = "#,##0";
                        else
                            worksheet.Cell(currentRow, 41).Style.NumberFormat.Format = "#,##0.#0";
                    }
                    else
                    {
                        worksheet.Cell(currentRow, 41).SetValue(0);
                        worksheet.Cell(currentRow, 41).Style.NumberFormat.Format = "#,##0";
                    }
                    worksheet.Cell(currentRow, 41).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;


                    var idOrderItemCell = worksheet.Cell(currentRow, 42);
                    idOrderItemCell.Value = IdOrderItem; // Id Order Item
                    idOrderItemCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 43).Value = ""; // Original Short
                    worksheet.Cell(currentRow, 44).Value = "pbtt"; // Agency Name
                    worksheet.Cell(currentRow, 45).Value = ""; // Properties
                    worksheet.Cell(currentRow, 46).Value = "TMĐT"; // Services
                    worksheet.Cell(currentRow, 47).Value = "checked"; // Status Checking
                    worksheet.Cell(currentRow, 48).Value = ""; // 最后一里路运单号
                    worksheet.Cell(currentRow, 49).Value = ""; // HsCode
                    worksheet.Cell(currentRow, 50).Value = "No"; // Is Fake Package

                    currentRow++;
                }

                // Định dạng bảng
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Tự động điều chỉnh kích thước cột
                worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Packages_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> DownloadManifestEn(BagDownloadFileRequestDto input)
        {
            // Lấy dữ liệu từ bảng Package
            input.MaxResultCount = int.MaxValue;
            input.IsExcel = true;
            var packages = await _bagAppService.GetDataManifestForDownload(input);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Packages");

                // Danh sách tiêu đề cột tiếng Anh
                var headers = new[]
                {
            "Waybill Number", // 1
            "Shipment Reference No.", // 2
            "Company Name", // 3
            "Contact Name", // 4
            "Tel", // 5
            "Address", // 6
            "Country", // 7
            "Postal Code", // 8
            "Company Name", // 9 (Receiver)
            "Contact Name", // 10 (Receiver)
            "Tel", // 11 (Receiver)
            "Address", // 12 (Receiver)
            "Country", // 13 (Receiver)
            "Postal Code", // 14 (Receiver)
            "Currency", // 15
            "Content", // 16
            "Quantity", // 17
            "Unit Price", // 18
            "Total Value", // 19
            "More than 2 items", // 20
            "Shipment Type", // 21
            "Total Package", // 22
            "Total Actual Weight", // 23
            "Origin", // 24
            "Destination", // 25
            "Creation Date", // 26
            "Processing Shipment Date", // 27
            "HSCODE", // 28
            "Category", // 29
            "BoxID", // 30
            "Package Number", // 31
            "Payment Method", // 32
            "COD Value", // 33
            "Product Category ID", // 34
            "Sort Code", // 35
            "SKU", // 36
            "Total COD Value", // 37
            "Category (ZH)", // 38
            "Package Code", // 39
            "Order ID", // 40
            "Link", // 41
            "Id Order Item", // 42
            "Code Item", // 43
            "Unit (blank)", // 44
            "Purchasing Staff", // 45
            "Translated", // 46
            "Translated Short", // 47
            "Original Short", // 48
            "Tiếng Trung AI", // 49
            "Tiếng Việt AI", // 50
            "Unit", // 51
            "CITIZEN IDENTITY CARD", // 52
            "Agency Name", // 53
            "Properties", // 54
            "Services", // 55
            "Status Checking", // 56
            "Declared Value", // 57
            "Lastmile Tracking No", // 58
            "HsCode", // 59
            "Is Fake Package" // 60
        };

                // Ghi tiêu đề vào hàng đầu tiên
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#c9daf8");
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                // Lấy thông tin giả lập nếu cần
                var fakeCompanyName = await _configurationSettingAppService.GetValueAsync("fake_company");
                var fakeAddress = await _configurationSettingAppService.GetValueAsync("fake_address");
                var fakePhone = await _configurationSettingAppService.GetValueAsync("fake_phone");
                var noneCN = "无";

                int currentRow = 2;
                foreach (var package in packages.Items)
                {
                    // 1. Waybill Number
                    worksheet.Cell(currentRow, 1).Value = package.TrackingNumber ?? "";

                    // 2. Shipment Reference No.
                    worksheet.Cell(currentRow, 2).Value = package.OrderCode ?? "";

                    // 3. Company Name (Sender)
                    worksheet.Cell(currentRow, 3).Value = fakeCompanyName;

                    // 4. Contact Name (Sender)
                    worksheet.Cell(currentRow, 4).Value = fakeCompanyName;

                    // 5. Tel (Sender)
                    worksheet.Cell(currentRow, 5).Value = fakePhone;

                    // 6. Address (Sender)
                    worksheet.Cell(currentRow, 6).Value = fakeAddress;

                    // 7. Country (Sender)
                    worksheet.Cell(currentRow, 7).Value = "China"; // hoặc lấy từ kho nguồn nếu có

                    // 8. Postal Code (Sender)
                    worksheet.Cell(currentRow, 8).Value = "N/A"; // Chưa có thông tin

                    // 9. Company Name (Receiver)
                    worksheet.Cell(currentRow, 9).Value = package.CustomerFakeName ?? "";

                    // 10. Contact Name (Receiver)
                    worksheet.Cell(currentRow, 10).Value = package.CustomerFakeName ?? "";

                    // 11. Tel (Receiver)
                    worksheet.Cell(currentRow, 11).Value = package.CustomerFakePhone ?? "";

                    // 12. Address (Receiver)
                    worksheet.Cell(currentRow, 12).Value = package.CustomerFakeAddress ?? "";

                    // 13. Country (Receiver)
                    worksheet.Cell(currentRow, 13).Value = "VN"; // hoặc lấy từ kho đích nếu có

                    // 14. Postal Code (Receiver)
                    worksheet.Cell(currentRow, 14).Value = ""; // Chưa có thông tin

                    // 15. Currency
                    worksheet.Cell(currentRow, 15).Value = "VND"; // hoặc lấy từ đơn hàng nếu có

                    // 16. Content
                    worksheet.Cell(currentRow, 16).Value = package.ProductNameVi ?? "";

                    // 17. Quantity
                    worksheet.Cell(currentRow, 17).SetValue(package.Quantity ?? 0);

                    // 18. Unit Price
                    worksheet.Cell(currentRow, 18).SetValue(package.Price ?? 0);

                    // 19. Total Value
                    worksheet.Cell(currentRow, 19).SetValue((package.Quantity ?? 0) * (package.Price ?? 0));

                    // 20. More than 2 items
                    worksheet.Cell(currentRow, 20).Value = (package.Quantity ?? 0) > 2 ? "Yes" : "No";

                    // 21. Shipment Type
                    worksheet.Cell(currentRow, 21).Value = package.ShippingLineString ?? "";

                    // 22. Total Package
                    worksheet.Cell(currentRow, 22).Value = ""; // Chưa rõ, có thể là tổng số kiện trong bao

                    // 23. Total Actual Weight
                    worksheet.Cell(currentRow, 23).SetValue(package.Weight ?? 0);

                    // 24. Origin
                    worksheet.Cell(currentRow, 24).Value = "CN"; // hoặc lấy từ kho nguồn

                    // 25. Destination
                    worksheet.Cell(currentRow, 25).Value = "VN"; // hoặc lấy từ kho đích

                    // 26. Creation Date
                    worksheet.Cell(currentRow, 26).Value = package.CreationTimeFormat;

                    // 27. Processing Shipment Date
                    worksheet.Cell(currentRow, 27).Value = package.ExportDate?.ToString("dd/MM/yyyy HH:mm") ?? "";

                    // 28. HSCODE
                    worksheet.Cell(currentRow, 28).Value = ""; // Chưa có thông tin

                    // 29. Category
                    worksheet.Cell(currentRow, 29).Value = ""; // Chưa có thông tin

                    // 30. BoxID
                    worksheet.Cell(currentRow, 30).Value = package.BagNumber ?? "";

                    // 31. Package Number
                    worksheet.Cell(currentRow, 31).Value = package.PackageNumber ?? "";

                    // 32. Payment Method
                    worksheet.Cell(currentRow, 32).Value = ""; // Chưa có thông tin

                    // 33. COD Value
                    worksheet.Cell(currentRow, 33).Value = ""; // Chưa có thông tin

                    // 34. Product Category ID
                    worksheet.Cell(currentRow, 34).Value = package.ProductGroupTypeId?.ToString() ?? "";

                    // 35. Sort Code
                    worksheet.Cell(currentRow, 35).Value = ""; // Chưa có thông tin

                    // 36. SKU
                    worksheet.Cell(currentRow, 36).Value = ""; // Chưa có thông tin

                    // 37. Total COD Value
                    worksheet.Cell(currentRow, 37).Value = ""; // Chưa có thông tin

                    // 38. Category (ZH)
                    worksheet.Cell(currentRow, 38).Value = ""; // Chưa có thông tin

                    // 39. Package Code
                    worksheet.Cell(currentRow, 39).Value = package.PackageNumber ?? "";

                    // 40. Order ID
                    worksheet.Cell(currentRow, 40).Value = package.OrderId;

                    // 41. Link
                    worksheet.Cell(currentRow, 41).Value = package.ProductLink ?? "";

                    // 42. Id Order Item
                    worksheet.Cell(currentRow, 42).Value = ""; // Chưa có thông tin

                    // 43. Code Item
                    worksheet.Cell(currentRow, 43).Value = ""; // Chưa có thông tin

                    // 44. Unit (blank)
                    worksheet.Cell(currentRow, 44).Value = "";

                    // 45. Purchasing Staff
                    worksheet.Cell(currentRow, 45).Value = ""; // Chưa có thông tin

                    // 46. Translated
                    worksheet.Cell(currentRow, 46).Value = package.ProductNameVi; // Chưa có thông tin

                    // 47. Translated Short
                    worksheet.Cell(currentRow, 47).Value = package.ProductNameVi; // Chưa có thông tin

                    // 48. Original Short
                    worksheet.Cell(currentRow, 48).Value = ""; // Chưa có thông tin

                    // 49. Tiếng Trung AI
                    worksheet.Cell(currentRow, 49).Value = package.ProductNameCn; // Chưa có thông tin

                    // 50. Tiếng Việt AI
                    worksheet.Cell(currentRow, 50).Value = package.ProductNameVi; // Chưa có thông tin

                    // 51. Unit
                    worksheet.Cell(currentRow, 51).Value = ""; // Chưa có thông tin

                    // 52. CITIZEN IDENTITY CARD
                    worksheet.Cell(currentRow, 52).Value = ""; // Chưa có thông tin

                    // 53. Agency Name
                    worksheet.Cell(currentRow, 53).Value = "pbtt"; // hoặc lấy từ cấu hình

                    // 54. Properties
                    worksheet.Cell(currentRow, 54).Value = ""; // Chưa có thông tin

                    // 55. Services
                    worksheet.Cell(currentRow, 55).Value = package.ShippingLineString ?? "";

                    // 56. Status Checking
                    worksheet.Cell(currentRow, 56).Value = "checked"; // hoặc trạng thái kiểm tra

                    // 57. Declared Value
                    worksheet.Cell(currentRow, 57).Value = "";

                    // 58. Lastmile Tracking No
                    worksheet.Cell(currentRow, 58).Value = ""; // Chưa có thông tin

                    // 59. HsCode
                    worksheet.Cell(currentRow, 59).Value = ""; // Chưa có thông tin

                    // 60. Is Fake Package
                    worksheet.Cell(currentRow, 60).Value = "No";

                    currentRow++;
                }

                // Định dạng bảng
                var range = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Tự động điều chỉnh kích thước cột
                worksheet.Columns().AdjustToContents();

                // Xuất file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Packages_EN_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        private int  GetPermissionChecker()
        {
            try
            {
                var currentUserId = AbpSession.UserId;

                var permissionCase = -1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;

                var customerDtoIds = new List<CustomerIdDto>();

                // admin thì nhìn thấy tất cả kiện hàng
                if (PermissionChecker.IsGranted(PermissionNames.Role_Admin)
                    )
                {
                    permissionCase = 1;
                }

                // Sale admin nhìn thấy tất cả các kiện hàng của mình, của sale dưới quyền và của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                }

                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 3;
                   
                }
               
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseCN))
                {
                    permissionCase = 5;
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseVN))
                {
                    permissionCase = 6;
                }
                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleCustom))
                {
                    permissionCase = 7;  
                }
                // customer chỉ nhìn thấy kiện hàng của mình
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 4;
                }
                else
                {
                    permissionCase = -1;
                }

                return permissionCase;
            }
            catch (Exception ex)
            {
                Logger.Error("Gặp lỗi khi kiểm tra quyền lấy thông tin khách hàng theo người dùng", ex);
                return -1;
            }
        }
    }
}
