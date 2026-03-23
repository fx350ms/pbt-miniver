using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using pbt.Application.WarehouseTransfers.Dto;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags.Dto;
using pbt.Commons.Dto;
using pbt.ConfigurationSettings;
using pbt.Core;
using pbt.Entities;
using pbt.OrderNumbers;
using pbt.Packages.Dto;
using pbt.WarehouseTransfers.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using pbt.Application.WarehouseTransfers;
using pbt.ChangeLogger;

namespace pbt.WarehouseTransfers
{
    [UnitOfWork(false)]
    [Audited]
    public class WarehouseTransferAppService : AsyncCrudAppService<
        WarehouseTransfer, // Entity
        WarehouseTransferDto, // DTO để trả về
        int, // Kiểu dữ liệu của khóa chính
        PagedWarehouseTransferResultRequestDto, // DTO để phân trang
        WarehouseTransferDto, // DTO để tạo mới
        WarehouseTransferDto>, // DTO để cập nhật
        IWarehouseTransferAppService
    {
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Bag> _bagRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IConfigurationSettingAppService _configurationSettingAppService;
        private readonly IRepository<Customer, long> _customerRepository;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly IWarehouseTransferDetailAppService _warehouseTransferDetailAppService;


        private pbtAppSession _pbtAppSession;

        public WarehouseTransferAppService(IRepository<WarehouseTransfer> repository,
            IIdentityCodeAppService identityCodeAppService,
            IRepository<Package> packageRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<Bag> bagRepository,
            IRepository<Customer, long> customerRepository,
            IConfigurationSettingAppService configurationSettingAppService,
            pbtAppSession pbtAppSession, IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IWarehouseTransferDetailAppService warehouseTransferDetailAppService)
            : base(repository)
        {

            _identityCodeAppService = identityCodeAppService;
            _packageRepository = packageRepository;
            _warehouseRepository = warehouseRepository;
            _bagRepository = bagRepository;
            _customerRepository = customerRepository;
            _pbtAppSession = pbtAppSession;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _warehouseTransferDetailAppService = warehouseTransferDetailAppService;
            _configurationSettingAppService = configurationSettingAppService;
        }

        public async Task<PagedResultDto<WarehouseTransferDto>> GetAllFilterAsync(PagedWarehouseTransferResultRequestDto input)
        {
            // Lấy danh sách phiếu chuyển kho từ repository
            var query = Repository.GetAll();

            // Áp dụng bộ lọc từ input
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(wt =>
                    wt.TransferCode.Contains(input.Keyword) ||
                    wt.Note.Contains(input.Keyword));
            }

            if (input.FromWarehouseId.HasValue && input.FromWarehouseId > 0)
            {
                query = query.Where(wt => wt.FromWarehouse == input.FromWarehouseId);
            }

            if (input.ToWarehouseId.HasValue && input.ToWarehouseId > 0)
            {
                query = query.Where(wt => wt.ToWarehouse == input.ToWarehouseId);
            }

            if (input.Status.HasValue && input.Status > 0)
            {
                query = query.Where(wt => wt.Status == input.Status);
            }

            if (input.CustomerId.HasValue && input.CustomerId > 0)
            {
                query = query.Where(wt => wt.CustomerId == input.CustomerId);
            }
            if (input.StartCreateDate.HasValue)
            {
                query = query.Where(wt => wt.CreationTime.Date >= input.StartCreateDate.Value.Date);
            }

            if (input.EndCreateDate.HasValue)
            {
                query = query.Where(wt => wt.CreationTime.Date <= input.EndCreateDate.Value.Date);
            }
            // Lấy tổng số lượng bản ghi
            var totalCount = await AsyncQueryableExecuter.CountAsync(query);

            // Áp dụng sắp xếp và phân trang
            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            // Lấy danh sách phiếu chuyển kho
            var items = await AsyncQueryableExecuter.ToListAsync(query);

            // Map dữ liệu sang DTO
            var dtos = ObjectMapper.Map<List<WarehouseTransferDto>>(items);

            // --- New logic: populate CustomerName for each dto ---
            // 1. Lấy danh sách CustomerId từ items
            var customerIds = items
                .Where(i => i.CustomerId > 0)
                .Select(i => i.CustomerId)
                .Distinct()
                .ToList();

            if (customerIds.Any() && _customerRepository != null)
            {
                // 2. Lấy danh sách khách hàng theo customerIds
                var customers = await _customerRepository.GetAllListAsync(c => customerIds.Contains(c.Id));

                // 3. Chuyển sang dictionary để tra nhanh
                var customerDict = customers.ToDictionary(c => c.Id, c => string.IsNullOrWhiteSpace(c.Username) ? string.Empty : c.Username);

                // 4. Gán CustomerName cho từng DTO nếu có
                foreach (var dto in dtos)
                {
                    if (dto == null) continue;
                    if (customerDict.TryGetValue(dto.CustomerId, out var name))
                    {
                        dto.CustomerName = name;
                    }
                    else
                    {
                        dto.CustomerName = string.Empty;
                    }
                }
            }

            // Trả về kết quả
            return new PagedResultDto<WarehouseTransferDto>(totalCount, dtos);
        }

        /// <summary>
        /// Chuyển kiện hàng
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<JsonResult> ChangePackageWarehouseAsync(CreatePackageWarehouseTransferDto dto)
        {
            try
            {
                // Lấy thông tin package
                var package = await _packageRepository.FirstOrDefaultAsync(p => p.Id == dto.PackageId);

                if (package == null)
                {
                    // Trả về kết quả
                    return new JsonResult(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin kiện hàng.",
                        StatusCode = 404,
                    });
                }

                // Kiểm tra trạng thái kiện hàng có đang ở Việt Nam không
                if (package.ShippingStatus != (int)PackageDeliveryStatusEnum.InWarehouseVN || package.WarehouseStatus != (int)WarehouseStatus.InStock)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        Message = "Kiện hàng không ở trạng thái 'Đã về kho Việt Nam'.",
                        StatusCode = 400,
                    });
                }

                if (package.WarehouseId == dto.ToWarehouse)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        Message = "Kho đích phải khác kho hiện tại.",
                        StatusCode = 400,
                    });
                }

                if (package.BagId.HasValue && package.BagId.Value > 0)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        Message = "Kiện hàng đang nằm trong bao, không thể chuyển kho riêng lẻ.",
                        StatusCode = 400,
                    });
                }


                // Lấy mã phiếu chuyển kho (có tiền tố PCK)
                var transferCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("PCK");

                var currentWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == package.WarehouseId);

                var newWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == dto.ToWarehouse);

                var data = new[]{
                    new  TransferItemDto()
                        {
                            Id = package.Id,
                            Code = package.PackageNumber
                        }};
                // Tạo mới phiếu chuyển kho
                var warehouseTransfer = new WarehouseTransfer
                {
                    TransferCode = transferCode.Code,
                    FromWarehouse = currentWarehouse.Id,
                    FromWarehouseName = currentWarehouse.Name,
                    FromWarehouseAddress = currentWarehouse.FullAddress,
                    ToWarehouse = dto.ToWarehouse,
                    ToWarehouseName = newWarehouse.Name,
                    ToWarehouseAddress = newWarehouse.FullAddress,
                    //PackageIds = package.Id.ToString(),
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                    Note = dto.Note,
                    Status = (int)WarehouseTransferStatusEnum.New, // Trạng thái phiếu chuyển kho: Chờ xử lý

                };

                await Repository.InsertAsync(warehouseTransfer);

                await ConnectDb.ExecuteNonQueryAsync("SP_Package_WarehouseTransfer", System.Data.CommandType.StoredProcedure, new[]{
                    new SqlParameter("@packageId", SqlDbType.Int) { Value = package.Id },
                    new SqlParameter("@packageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.Shipping },
                    new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock },
                    new SqlParameter("@warehouseId", SqlDbType.Int) { Value = (int)dto.ToWarehouse }
                    }
                  );

                // Trả về kết quả
                return new JsonResult(new
                {
                    Success = true,
                    Message = "Chuyển kho thành công.",
                    TransferCode = transferCode.Code
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi chuyển kho cho kiện hàng: " + ex.Message, ex);
                throw ex;
            }
        }

        public async Task<JsonResult> ChangeBagWarehouseAsync(CreateBagWarehouseTransferDto dto)
        {
            try
            {
                // Lấy thông tin package
                var bag = await _bagRepository.FirstOrDefaultAsync(p => p.Id == dto.BagId);

                if (bag == null)
                {
                    throw new UserFriendlyException("Không tìm thấy thông tin kiện hàng.");
                }

                // Kiểm tra trạng thái kiện hàng có đang ở Việt Nam không
                if (bag.ShippingStatus != (int)BagShippingStatus.GoToWarehouse)
                {
                    throw new UserFriendlyException("Kiện hàng không ở trạng thái 'Đã về kho Việt Nam'.");
                }

                if (bag.WarehouseDestinationId == dto.ToWarehouse)
                {
                    throw new UserFriendlyException("Kho đích phải khác kho hiện tại.");
                }

                // Lấy mã phiếu chuyển kho (có tiền tố PCK)
                var transferCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("PCK");

                var currentWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == bag.WarehouseDestinationId);

                var newWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == dto.ToWarehouse);

                var packages = (await _packageRepository.GetAllListAsync(p => p.BagId == bag.Id)).Select(p => new TransferItemDto
                {
                    Id = p.Id,
                    Code = p.PackageNumber
                }).ToList();

                var data = new[] {
                    new  TransferItemDto()
                        {
                            Id = bag.Id,
                            Code = bag.BagCode,
                            Packages = packages
                        }};

                // Tạo mới phiếu chuyển kho
                var warehouseTransfer = new WarehouseTransfer
                {
                    TransferCode = transferCode.Code,
                    FromWarehouse = currentWarehouse.Id,
                    FromWarehouseName = currentWarehouse.Name,
                    FromWarehouseAddress = currentWarehouse.FullAddress,
                    ToWarehouse = dto.ToWarehouse,
                    ToWarehouseName = newWarehouse.Name,
                    ToWarehouseAddress = newWarehouse.FullAddress,
                    //  BagIds = bag.Id.ToString(),
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(data),
                    Note = dto.Note,
                    Status = (int)WarehouseTransferStatusEnum.New, // Trạng thái phiếu chuyển kho: Chờ xử lý

                };

                await Repository.InsertAsync(warehouseTransfer);
                ConnectDb.ExecuteNonQuery("SP_Package_WarehouseTransfer", System.Data.CommandType.StoredProcedure, new[]{
                    new SqlParameter("@bagId", bag.Id),
                    new SqlParameter("@bagStatus", (int)BagShippingStatus.InTransit),
                    new SqlParameter("@packageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.Shipping },
                    new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock },
                    new SqlParameter("@warehouseId", SqlDbType.Int) { Value = (int)dto.ToWarehouse }
                    }
              );

                // Trả về kết quả
                return new JsonResult(new
                {
                    Success = true,
                    Message = "Chuyển kho thành công.",
                    TransferCode = transferCode.Code
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi chuyển kho cho kiện hàng: " + ex.Message, ex);
                throw ex;
            }
        }

        public async Task<WarehouseTransferDto> GetWarehouseTransferByCustomerId(long customerId, int warehouseId)
        {
            // Kiểm tra xem có phiếu chuyển kho nào đang ở trạng thái 'Mới' cho khách hàng này không
            var existingTransfer = await Repository
                .FirstOrDefaultAsync(wt => wt.CustomerId == customerId
                && wt.FromWarehouse == warehouseId
                && wt.Status == (int)WarehouseTransferStatusEnum.New);
            if (existingTransfer != null)
            {
                var warehouseTransferDto = ObjectMapper.Map<WarehouseTransferDto>(existingTransfer);
                return warehouseTransferDto;
            }
            return null;
        }

        public async Task<JsonResult> ScanCode(TransferWarehouseScanCodeDto data)
        {
            // kiểm tra xem mã scan là mã bao hay mã kiện hàng
            // lấy thông tin bao hoặc kiện hàng theo mã code
            // Nếu mã có tiền tố là kiện hàng
            // Lấy thông tin kiện và kiểm tra trạng thái, kho,...
            // Nếu thông tin là bao: Lấy thông tin theo bao và kiểm tra trạng thái, kho,...
            // Tìm phiếu chuyển tho theo bao hoặc kiện đã nêu ở trên

            // Nếu có phiếu chuyển kho ở trạng thái 'Mới' thì trả về phiếu đó
            try
            {
                // Kiểm tra xem mã quét có hợp lệ không
                if (data == null || string.IsNullOrEmpty(data.Code))
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        Status = 400,
                        Message = "Mã quét không hợp lệ."
                    });
                }

                if (data.FromWarehouseId == data.ToWarehouseId)
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        Status = 400,
                        Message = "Kho nguồn và kho đích không được trùng nhau."
                    });
                }
                Logger.Info($"[WarehouseTransfer] ScanCodeAsync - data: {JsonConvert.SerializeObject(data)} ");
                var result = new JsonResult(new { Status = 200, Message = "Success" });
                var packagePrefixList = await _configurationSettingAppService.GetValueAsync("PackagePrefixCode");
                if (string.IsNullOrEmpty(packagePrefixList))
                {
                    Logger.Error("Configuration 'PackageCode' is not set. Using default prefixes.");
                    return new JsonResult(new
                    {
                        Success = false,
                        Status = 400,
                        Message = "Mã kiện chưa được khai báo trong thiết lập."
                    });
                }

                string[] packagePrefix = packagePrefixList.Split(","); // new[] { "BT", "PBT", "KH", "MP", "PB", "SH", "CT" };


                if (packagePrefix.Any(prefix => data.Code.StartsWith(prefix)))
                {
                    var parameters = new[]
                        {
                            new SqlParameter("@PackageCode", data.Code),
                            new SqlParameter("@Status", (int) PackageDeliveryStatusEnum.InWarehouseVN)
                        };

                    var checkPackageStatusResult = await ConnectDb.GetItemAsync<CheckTransferResultDto>(
                    "SP_Packages_CheckValidForTransfer",
                    CommandType.StoredProcedure,
                    parameters);

                    if (checkPackageStatusResult == null || !checkPackageStatusResult.IsValid)
                    {
                        return new JsonResult(new
                        {
                            Success = false,
                            Status = 404,
                            Message = checkPackageStatusResult != null ? checkPackageStatusResult.Message : "Kiện hàng không hợp lệ để chuyển kho.",
                        });
                    }

                    // Kiểm tra xem mã là của kiện hàng hay bao
                    var package = await ConnectDb.GetItemAsync<PackageDto>("SP_Package_GetByPackageNumber", CommandType.StoredProcedure, new[] {
                        new SqlParameter("@packageNumber", data.Code)
                    });
                    if (package != null)
                    {
                        if (package.WarehouseId != data.FromWarehouseId)
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 404,
                                Message = "Kiện hàng không thuộc kho hiện tại.",
                            });
                        }

                        if (package.ShippingStatus != (int)PackageDeliveryStatusEnum.InWarehouseVN || package.WarehouseStatus != (int)WarehouseStatus.InStock)
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 404,
                                Message = "Kiện hàng không ở trong kho.",
                            });
                        }

                        if (package.BagId.HasValue && package.BagId.Value > 0)
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 404,
                                Message = "Kiện hàng đang nằm trong bao, không thể chuyển kho riêng lẻ.",
                            });
                        }

                        // Lấy phiếu chuyển kho
                        var existingTransfer = await ConnectDb.GetItemAsync<WarehouseTransferDto>("SP_WarehouseTransfers_GetByItemCode", CommandType.StoredProcedure, new[] {
                            new SqlParameter("@itemCode", data.Code),
                            new SqlParameter("@Status", (int)WarehouseTransferStatusEnum.New)
                        });

                        // Nếu đã tồn tại phiếu chuyển kho (existingTransfer != null) thì thông báo đã có trong phiếu chuyển kho khác
                        if (existingTransfer != null)
                        {
                            return new JsonResult(new
                            {
                                Status = 404,
                                Message = "Kiện hàng đã nằm trong phiếu chuyển kho khác",
                            });
                        }

                        existingTransfer = await ConnectDb.GetItemAsync<WarehouseTransferDto>("SP_WarehouseTransfers_GetByCustomerAndWarehouses", CommandType.StoredProcedure, new[] {
                            new SqlParameter("@customerId", package.CustomerId.HasValue ? package.CustomerId.Value : 0),
                            new SqlParameter("@fromWarehouseId", data.FromWarehouseId),
                            new SqlParameter("@toWarehouseId", data.ToWarehouseId),
                            new SqlParameter("@Status", (int)WarehouseTransferStatusEnum.New)
                        });
                        var wtId = 0;
                        var wtCode = "";
                        if (existingTransfer == null)

                        {

                            // Lấy thông tin của kho để tạo mới phiếu chuyển kho
                            var fromWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == data.FromWarehouseId);
                            var toWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == data.ToWarehouseId);
                            var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("PCK");
                            var wt = new WarehouseTransfer()
                            {
                                TransferCode = identityCode.Code,
                                FromWarehouse = fromWarehouse.Id,
                                FromWarehouseName = fromWarehouse.Name,
                                FromWarehouseAddress = fromWarehouse.FullAddress,
                                ToWarehouse = toWarehouse.Id,
                                ToWarehouseName = toWarehouse.Name,
                                ToWarehouseAddress = toWarehouse.FullAddress,
                                CustomerId = package.CustomerId.Value,
                                Status = (int)WarehouseTransferStatusEnum.New, // Trạng thái phiếu chuyển kho: Chờ xử lý
                                TotalQuantity = 1,
                                TotalWeight = package.Weight ?? 0,
                                ShippingFee = 0
                            };

                            // Thêm dữ liệu kiện hàng vào phiếu chuyển kho và lấy ID
                            wtId = await Repository.InsertAndGetIdAsync(wt);
                            wtCode = wt.TransferCode;
                        }
                        else
                        {
                            wtId = existingTransfer.Id;
                            wtCode = existingTransfer.TransferCode;
                        }

                        // thêm dữ liệu vào bảng phiếu chuyển kho chi tiết
                        var execResult = await ConnectDb.GetItemAsync<ExecuteResultDto>(
                            "SP_WarehouseTransferDetails_AddPackage",
                            CommandType.StoredProcedure, new[]{
                            new SqlParameter("@WarehouseTransferId", wtId),
                            new SqlParameter("@PackageId", package.Id)
                        });

                        // log kiện được thêm vào phiếu chuyển kho
                        var packageLog = ObjectMapper.Map<Package>(package);
                        _ = _entityChangeLoggerAppService.LogChangeAsync<Package>(
                            null,
                            packageLog,
                            "Thêm kiện hàng vào phiếu chuyển kho",
                            $"Thêm kiện hàng {package.PackageNumber} vào phiếu chuyển kho {wtCode}");

                        if (execResult.StatusCode > 0)
                        {
                            return new JsonResult(new
                            {
                                Success = true,
                                Status = 200,
                                Message = "Thêm kiện hàng vào phiếu chuyển kho thành công.",
                                WarehouseTransferId = wtId,
                                WarehouseTransferCode = wtCode,
                                CustomerId = package.CustomerId.Value
                            });
                        }
                        else
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 501,
                                Message = "Lỗi khi thêm kiện hàng vào phiếu chuyển kho.",
                            });
                        }
                    }
                }
                else
                {  // Nếu không phải kiện hàng, kiểm tra xem mã là của bao

                    var bag = await ConnectDb.GetItemAsync<BagDto>("SP_Bags_GetByBagCode", CommandType.StoredProcedure, new[] {
                        new SqlParameter("@bagCode", data.Code)
                    });

                    if (bag != null)
                    {
                        // Nếu là bao, kiểm tra trạng thái và kho
                        if (bag.ShippingStatus != (int)BagShippingStatus.GoToWarehouse || bag.WarehouseStatus != (int) WarehouseStatus.InStock)
                        {
                            return new JsonResult(new
                            { 
                                Success = false,
                                Status = 404,
                                Message = "Bao không ở trong kho.",
                            });
                        }

                        if (bag.WarehouseDestinationId != data.FromWarehouseId)
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 404,
                                Message = "Bao không thuộc kho hiện tại.",
                            });
                        }

                        // Kiểm tra xem bao đã nằm trong phiếu chuyển kho nào chưa
                        var existingTransfer = await ConnectDb.GetItemAsync<WarehouseTransferDto>("SP_WarehouseTransfers_GetByItemCode", CommandType.StoredProcedure, new[] {
                            new SqlParameter("@itemCode", data.Code),
                            new SqlParameter("@Status", (int)WarehouseTransferStatusEnum.New)
                        });

                        if (existingTransfer != null)
                        {
                            return new JsonResult(new
                            {
                                Status = 404,
                                Message = "Bao đã nằm trong phiếu chuyển kho khác.",
                            });
                        }

                        // Lấy danh sách kiện hàng trong bao
                        var packages = (await ConnectDb.GetListAsync<PackageDto>(
                            "SP_Packages_GetByBagId",
                                    CommandType.StoredProcedure, new[] { new SqlParameter("@bagId", bag.Id) }
                            )).ToList();

                        if (!packages.Any())
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 404,
                                Message = "Bao không có kiện hàng để chuyển kho.",
                            });
                        }

                        // Kiểm tra xem đã có phiếu chuyển kho cho bao này chưa
                        existingTransfer = await ConnectDb.GetItemAsync<WarehouseTransferDto>("SP_WarehouseTransfers_GetByCustomerAndWarehouses", CommandType.StoredProcedure, new[] {
                        new SqlParameter("@customerId", bag.CustomerId.HasValue ? bag.CustomerId.Value : 0),
                        new SqlParameter("@fromWarehouseId", data.FromWarehouseId),
                        new SqlParameter("@toWarehouseId", data.ToWarehouseId),
                        new SqlParameter("@Status", (int)WarehouseTransferStatusEnum.New)
                    });

                        var wtId = 0;
                        var wtCode = "";

                        if (existingTransfer == null)
                        {
                            // Lấy thông tin của kho để tạo mới phiếu chuyển kho
                            var fromWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == data.FromWarehouseId);
                            var toWarehouse = await _warehouseRepository.FirstOrDefaultAsync(w => w.Id == data.ToWarehouseId);
                            var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("PCK");

                            var wt = new WarehouseTransfer()
                            {
                                TransferCode = identityCode.Code,
                                FromWarehouse = fromWarehouse.Id,
                                FromWarehouseName = fromWarehouse.Name,
                                FromWarehouseAddress = fromWarehouse.FullAddress,
                                ToWarehouse = toWarehouse.Id,
                                ToWarehouseName = toWarehouse.Name,
                                ToWarehouseAddress = toWarehouse.FullAddress,
                                CustomerId = bag.CustomerId.Value,
                                Status = (int)WarehouseTransferStatusEnum.New, // Trạng thái phiếu chuyển kho: Chờ xử lý
                                TotalQuantity = packages.Count,
                                TotalWeight = packages.Sum(p => p.Weight ?? 0),
                                ShippingFee = 0
                            };

                            // Thêm phiếu chuyển kho mới
                            wtId = await Repository.InsertAndGetIdAsync(wt);
                            wtCode = wt.TransferCode;
                        }
                        else
                        {
                            wtId = existingTransfer.Id;
                            wtCode = existingTransfer.TransferCode;
                        }

                        // Thêm dữ liệu bao và các kiện hàng vào phiếu chuyển kho chi tiết
                        var execResult = await ConnectDb.GetItemAsync<ExecuteResultDto>("SP_WarehouseTransferDetails_AddBag", CommandType.StoredProcedure, new[] {
                            new SqlParameter("@WarehouseTransferId", wtId),
                            new SqlParameter("@BagId", bag.Id)
                        });

                        // log phiếu chuyển kho bao
                        var bagLog = ObjectMapper.Map<Bag>(bag);
                        _ = _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                            null,
                            bagLog,
                            "Add",
                            "Thêm bao vào phiếu chuyển kho " + wtCode,
                            true);

                        // log các phiếu chuyển kho kiện
                        foreach (var package in packages)
                        {
                            var packageLog = ObjectMapper.Map<Package>(package);
                            _ = _entityChangeLoggerAppService.LogChangeAsync<Package>(
                                null,
                                packageLog,
                                "Add",
                                "Thêm kiện hàng vào phiếu chuyển kho " + wtCode,
                                true);
                        }

                        if (execResult.StatusCode > 0)
                        {
                            return new JsonResult(new
                            {
                                Success = true,
                                Status = 200,
                                Message = "Thêm bao vào phiếu chuyển kho thành công.",
                                WarehouseTransferId = wtId,
                                WarehouseTransferCode = wtCode,
                                CustomerId = bag.CustomerId.Value
                            });
                        }
                        else
                        {
                            return new JsonResult(new
                            {
                                Success = false,
                                Status = 500,
                                Message = "Lỗi khi thêm bao vào phiếu chuyển kho.",
                            });
                        }
                    }
                }

                return new JsonResult(new
                {
                    Status = 404,
                    Message = "Không tìm thấy bao hoặc kiện hàng với mã đã quét."
                });
                // Nếu không tìm thấy mã, ném lỗi
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi xử lý mã quét: " + ex.Message, ex);
                throw new UserFriendlyException("Đã xảy ra lỗi khi xử lý mã quét. Vui lòng thử lại." + ex.Message);
            }
        }

        public async Task<JsonResult> Received(long id)
        {
            try
            {
                var warehouseTransfer = await Repository.FirstOrDefaultAsync(wt => wt.Id == id);
                if (warehouseTransfer == null)
                {
                    throw new UserFriendlyException("Không tìm thấy phiếu chuyển kho.");
                }
                if (warehouseTransfer.Status != (int)WarehouseTransferStatusEnum.InTransit)
                {
                    throw new UserFriendlyException("Chỉ có thể đóng các phiếu chuyển kho ở trạng thái 'Mới'.");
                }


                warehouseTransfer.Status = (int)WarehouseTransferStatusEnum.Received;
                await Repository.UpdateAsync(warehouseTransfer);

                // log trạng thái đã chuyển sang kho mới
                var warehouseTransferDetails =
                    await _warehouseTransferDetailAppService.GetDetailsByWarehouseTransferIdAsync(warehouseTransfer.Id);
                var logTasks = new List<Task>();
                foreach (var warehouseTransferDetail in warehouseTransferDetails)
                {
                    if (warehouseTransferDetail.PackageCode == null && warehouseTransferDetail.BagNumber != null)
                    {
                        var bag = (await _bagRepository.GetAllIncludingAsync(x => x.Packages)).FirstOrDefault(x => x.BagCode == warehouseTransferDetail.BagNumber);
                        if (bag != null)
                        {
                            // add log bag
                            logTasks.Add(_entityChangeLoggerAppService.LogChangeAsync<Bag>(
                                null,
                                bag,
                                "Add",
                                "Bao đã được chuyển đến kho " + warehouseTransfer.ToWarehouseName,
                                true));

                            // add log package in bag
                            if (bag.Packages != null)
                            {
                                foreach (var package in bag.Packages)
                                {
                                    logTasks.Add(_entityChangeLoggerAppService.LogChangeAsync<Package>(
                                        null,
                                        package,
                                        "Add",
                                        "Kiện hàng đã được chuyển đến kho " + warehouseTransfer.ToWarehouseName,
                                        true));
                                }
                            }
                        }
                    }
                    else if (warehouseTransferDetail.PackageCode != null)
                    {
                        var packageLog = await _packageRepository.FirstOrDefaultAsync(x => x.PackageNumber == warehouseTransferDetail.PackageCode);
                        if (packageLog != null)
                        {
                            logTasks.Add(_entityChangeLoggerAppService.LogChangeAsync<Package>(
                                null,
                                packageLog,
                                "Add",
                                "Kiện hàng đã được chuyển đến kho " + warehouseTransfer.ToWarehouseName,
                                true));
                        }
                    }
                }
                // Thực hiện tất cả các tác vụ log bất đồng bộ
                if (logTasks.Any())
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.WhenAll(logTasks);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Lỗi khi ghi log chuyển kho: " + ex.Message, ex);
                        }
                    });
                }

                return new JsonResult(new
                {
                    Success = true,
                    Message = "Đóng phiếu chuyển kho thành công."
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi đóng phiếu chuyển kho: " + ex.Message, ex);
                throw ex;
            }
        }

        public async Task<JsonResult> SaveWarehouseTransfer(WarehouseTransferSaveDto input)
        {
            try
            {
                // Lấy thông tin phiếu chuyển kho
                var warehouseTransfer = await Repository.FirstOrDefaultAsync(wt => wt.Id == input.Id);

                if (warehouseTransfer == null)
                {
                    throw new UserFriendlyException("Không tìm thấy phiếu chuyển kho.");
                }

                if (warehouseTransfer.Status != (int)WarehouseTransferStatusEnum.New)
                {
                    throw new UserFriendlyException("Chỉ có thể lưu phiếu chuyển kho ở trạng thái 'Mới'.");
                }

                var result = await ConnectDb.GetItemAsync<ExecuteResultDto>("SP_WarehouseTransfers_UpdateStatusAndDestinationEx",
                    CommandType.StoredProcedure, new[]
                {
                    new SqlParameter("@WarehouseTransferId", SqlDbType.Int) { Value = warehouseTransfer.Id },
                      // trạng thái hiện tại cần kiểm tra của kiện
                    new SqlParameter("@PackageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.InWarehouseVN },
                    // trạng thái hiện tại cần kiểm tra của bao
                    new SqlParameter("@BagStatus", SqlDbType.Int) { Value = (int)BagShippingStatus.GoToWarehouse },

                    // trạng thái mới cần cập nhật cho kiện
                    new SqlParameter("@NewPackageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.Shipping },
                    // trạng thái mới cần cập nhật cho bao
                    new SqlParameter("@NewBagStatus", SqlDbType.Int) { Value = (int)BagShippingStatus.InTransit }
                });

                if (result.StatusCode <= 0)
                {
                    throw new UserFriendlyException("Lỗi khi cập nhật kho đích cho phiếu chuyển kho." + result?.Message);
                }
                // Cập nhật phí chuyển kho
                warehouseTransfer.ShippingFee = input.ShippingFee;

                // Cập nhật trạng thái phiếu chuyển kho thành "Đang vận chuyển"
                warehouseTransfer.Status = (int)WarehouseTransferStatusEnum.InTransit;

                await Repository.UpdateAsync(warehouseTransfer);

                return new JsonResult(new
                {
                    Success = true,
                    Message = "Phiếu chuyển kho đã được lưu và chuyển sang trạng thái 'Đang vận chuyển'."
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lưu phiếu chuyển kho: " + ex.Message, ex);
                throw new UserFriendlyException("Đã xảy ra lỗi khi lưu phiếu chuyển kho. Vui lòng thử lại.");
            }
        }

    }
}