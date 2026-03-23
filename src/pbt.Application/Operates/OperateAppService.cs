using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.Core;
using pbt.Departments;
using pbt.Entities;
using pbt.EntityAuditLogs;
using pbt.EntityAuditLogs.Dto;
using pbt.FundAccounts;
using pbt.FundAccounts.Dto;
using pbt.Operates.Dto;
using pbt.OrderNumbers;
using pbt.Orders.Dto;
using pbt.Packages.Dto;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static MassTransit.ValidationResultExtensions;

namespace pbt.Operates
{
    /// <summary>
    /// 
    /// </summary>
    [Authorize]
    [UnitOfWork(false)]
    public class OperateAppService : AbpServiceBase, IOperateAppService
    {
        private readonly IRepository<Package, int> _packageRepository;
        private readonly IRepository<Bag, int> _bagRepository;
        private readonly IRepository<IdentityCode, long> _identityCodeRepository;
        private readonly IRepository<Order, long> _orderRepository;
        private readonly IRepository<BarCode, long> _barCodeRepository;
        private readonly IRepository<Customer, long> _customerRepository;

        private readonly pbtAppSession _pbtAppSession;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly FundAccountDto currentFundAccount = null;
        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly IRepository<CustomerTransaction, long> _walletTransactionRepository;
        private readonly IRepository<Transaction, long> _transactionRepository;
        private readonly IEntityAuditLogApiClient _entityAuditLogApiClient;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageRepository"></param>
        /// <param name="bagRepository"></param>
        /// <param name="identityCodeRepository"></param>
        public OperateAppService(
            IRepository<Package, int> packageRepository,
            IRepository<Bag, int> bagRepository,
            IRepository<IdentityCode, long> identityCodeRepository,
            IRepository<Order, long> orderRepository,
            IRepository<BarCode, long> barCodeRepository,
            IIdentityCodeAppService identityCodeAppService,
            pbtAppSession pbtAppSession,
            IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IRepository<Customer, long> customerRepository,
            IFundAccountAppService fundAccountAppService,
            IRepository<CustomerTransaction, long> walletTransactionRepository,
            IRepository<Transaction, long> transactionRepository,
             IEntityAuditLogApiClient entityAuditLogApiClient
            )
        {
            _packageRepository = packageRepository;
            _bagRepository = bagRepository;
            _identityCodeRepository = identityCodeRepository;
            _orderRepository = orderRepository;
            _barCodeRepository = barCodeRepository;
            _pbtAppSession = pbtAppSession;
            // _waybillRepository = waybillRepository;
            _identityCodeAppService = identityCodeAppService;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _customerRepository = customerRepository;

            _fundAccountAppService = fundAccountAppService;
            _walletTransactionRepository = walletTransactionRepository;
            _transactionRepository = transactionRepository;
            _entityAuditLogApiClient = entityAuditLogApiClient;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<JsonResult> UpdatePackageShippingStatus(OperateActionDto input)
        {

            try
            {
                if (string.IsNullOrEmpty(input.ScanCode) || string.IsNullOrEmpty(input.ScanCode))
                {
                    return new JsonResult(new { Success = false, StatusCode = 400, Message = "InvalidScanCode" });
                }
                Logger.Info($"UpdatePackageShippingStatus: {input.ScanCode} - {input.ActionType} - {input.Warehouse} - {input.CodeType}");

                if (input.ActionType != (int)OperateActionType.In && input.ActionType != (int)OperateActionType.Out)
                {
                    return new JsonResult(new { Success = false, StatusCode = 400, Message = "Invalid action type" });
                }

                if (input.Warehouse != (int)WarehouseType.Cn && input.Warehouse != (int)WarehouseType.Vn)
                {
                    return new JsonResult(new { Success = false, StatusCode = 400, Message = "Invalid warehouse type" });
                }

                Logger.Info($"[ScanCode] input: ${JsonConvert.SerializeObject(input)}");

                input.ScanCode = input.ScanCode.Trim();

                // Nhập 
                if (input.ActionType == (int)OperateActionType.In)
                {
                    // Kho TQ
                    if (input.Warehouse == (int)WarehouseType.Cn)
                    {
                        var result = await CNImport(input);
                        return result; ;
                    }
                    // Kho VN
                    else if (input.Warehouse == (int)WarehouseType.Vn)
                    {
                        var result = await VNImport(input);
                        return result;
                    }
                }
                // Xuất
                else
                {
                    if (input.Warehouse == (int)WarehouseType.Cn)
                    {
                        var result = await CNExport(input);
                        return result;
                    }
                    else
                    {
                        var result = await VNWarehouseExport(input);
                        return result;
                    }
                }

                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = 400,
                    Value = new { Message = "Yêu cầu không hợp lệ" }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in UpdatePackageShippingStatus: {ex.Message}", ex);
                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = 500,
                    Value = new { Message = "Lỗi hệ thống: " + ex.Message }
                });
            }
        }

        private async Task<JsonResult> VNImport(OperateActionDto input)
        {
            switch ((CodeType)input.CodeType)
            {
                case CodeType.Package:
                    {

                        var packageExisted = await GetPackageByCodeAsync(input.ScanCode);
                        if (packageExisted == null)
                        {
                            return new JsonResult(new { Success = false, StatusCode = 400, Message = "Mã kiện không tồn tại" });
                        }
                        Bag bag = null;
                        if (packageExisted.BagId.HasValue && packageExisted.BagId > 0)
                        {
                            // return new JsonResult(new { Success = false, StatusCode = 400, Message = "Kiện đang trong bao, không thể nhập kho kiện lẻ" });
                            bag = await _bagRepository.FirstOrDefaultAsync(x => x.Id == packageExisted.BagId.Value);
                            if (bag != null && bag.BagType == (int)BagTypeEnum.SeparateBag)
                            {
                                return new JsonResult(new { Success = false, StatusCode = 400, Message = $"Kiện đang trong bao {bag.BagCode}, không thể nhập kho kiện lẻ" });
                            }
                            if (bag != null && bag.BagType == (int)BagTypeEnum.InclusiveBag
                            && bag.ShippingStatus != (int)BagShippingStatus.GoToWarehouse
                            )
                            {
                                return new JsonResult(new { Success = false, StatusCode = 400, Message = $"Kiện đang trong bao {bag.BagCode}, vui lòng nhập kho bao trước" });
                            }
                        }

                        try
                        {
                            var currentShippingStatus = packageExisted.ShippingStatus;
                            if (currentShippingStatus != (int)PackageDeliveryStatusEnum.Shipping
                                && currentShippingStatus != (int)PackageDeliveryStatusEnum.DeliveryInProgress
                                && currentShippingStatus != (int)PackageDeliveryStatusEnum.Delivered
                                && currentShippingStatus != (int)PackageDeliveryStatusEnum.Completed
                                && currentShippingStatus != (int)PackageDeliveryStatusEnum.WarehouseTransfer
                                )
                            {
                                return new JsonResult(new { Success = false, StatusCode = 400, Message = "Kiện không ở trạng thái vận chuyển" });
                            }


                            // Lưu ở đây để xác định trước khi thay đổi
                            var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == packageExisted.OrderId);
                            var statusCodePr = new SqlParameter("@StatusCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            var messagePr = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                            var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Packages_ImportToVNWarehouse", CommandType.StoredProcedure,
                                new[]
                                {
                                    new SqlParameter("@PackageId", SqlDbType.Int) { Value = packageExisted.Id },
                                    new SqlParameter("@CurrentWarehouseId", SqlDbType.Int) { Value = _pbtAppSession.WarehouseId.Value },
                                    new SqlParameter("@ModifierUserId", SqlDbType.BigInt) { Value = _pbtAppSession.UserId.Value },
                                    new SqlParameter("@ShippingStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.InWarehouseVN },
                                    new SqlParameter("@WarehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock },
                                    new SqlParameter("@OrderStatus", SqlDbType.Int) { Value = (int)OrderStatus.InVietnamWarehouse},
                                    statusCodePr,
                                    messagePr
                                }
                            );

                            string unbagLog = "";
                            if (bag != null)
                            {
                                unbagLog = $"(Kiện được bỏ khỏi bao {bag.BagCode})";
                            }

                            // add log
                            _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                                null
                                , packageExisted
                                , "updated"
                                , $"Cập nhật trạng thái vận chuyển từ <b>{((PackageDeliveryStatusEnum)currentShippingStatus).GetDescription()}</b> sang <b>{PackageDeliveryStatusEnum.InWarehouseVN.GetDescription()}</b> {unbagLog}"
                                , true
                            );

                            var packageAuditLog = new EntityAuditLogDto()
                            {
                                EntityId = packageExisted.Id.ToString(),
                                EntityType = nameof(Package),
                                MethodName = EntityAuditLogMethodName.Update.ToString(),
                                Title = $"Kiện hàng #{packageExisted.Id} - {packageExisted.PackageNumber} được nhập kho VN",
                                UserId = _pbtAppSession.UserId,
                                UserName = _pbtAppSession.UserName,
                                Data = JsonConvert.SerializeObject(packageExisted),

                            };
                            _entityAuditLogApiClient.SendAsync(packageAuditLog);

                            // update order status

                            if (order != null)
                            {
                                // Nếu trạng thái đơn hàng đã là InVietnamWarehouse thì không cập nhật nữa
                                if (order.OrderStatus < (int)OrderStatus.InVietnamWarehouse)
                                {
                                    order.OrderStatus = (int)OrderStatus.InVietnamWarehouse;
                                    _entityChangeLoggerAppService.LogChangeAsync<Order>(
                                        null
                                        , order
                                        , "updated"
                                        , $"Cập nhật trạng thái đơn hàng sang <b>{OrderStatus.InVietnamWarehouse.GetDescription()}</b>"
                                        , true
                                    );

                                    var orderDto = ObjectMapper.Map<OrderDto>(order);

                                    var orderAuditLog = new EntityAuditLogDto()
                                    {
                                        EntityId = packageExisted.Id.ToString(),
                                        EntityType = nameof(Order),
                                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                                        Title = $"Đơn hàng #{orderDto.Id} - {orderDto.WaybillNumber} được cập nhật trạng thái thông qua nhập kiện #{packageExisted.Id} - {packageExisted.PackageNumber}",
                                        UserId = _pbtAppSession.UserId,
                                        UserName = _pbtAppSession.UserName,
                                        Data = JsonConvert.SerializeObject(orderDto),

                                    };
                                    _entityAuditLogApiClient.SendAsync(orderAuditLog);


                                }
                            }
                            // Nếu không phải trạng thái là đang vận chuyển quốc tế
                            if (currentShippingStatus != (int)PackageDeliveryStatusEnum.Shipping)
                            {
                                await UpdateWalletReturnByPackage(packageExisted.TotalPrice.Value, packageExisted.Id, packageExisted.PackageNumber, packageExisted.CustomerId.Value);
                            }

                            // get customer
                            //   var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == packageExisted.CustomerId);

                            await _barCodeRepository.InsertAsync(new BarCode
                            {
                                ScanCode = input.ScanCode,
                                CodeType = input.CodeType,
                                Action = input.ActionType,
                                SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                                CreatorUserName = _pbtAppSession.UserName,
                                CustomerName = order.CustomerName,
                            });

                            return new JsonResult(new
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Cập nhật trạng thái kiện thành công"
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error updating import status for package {input.ScanCode}: {ex.Message}", ex);
                            return new JsonResult(new { Success = false, StatusCode = 500, Message = "Lỗi hệ thống khi cập nhật trạng thái kiện" });
                        }
                    }
                case CodeType.Bag:
                    {
                        var bagExists = await _bagRepository.FirstOrDefaultAsync(x => x.BagCode == input.ScanCode);
                        if (bagExists == null)
                        {
                            return new JsonResult(new { Success = false, StatusCode = 400, Message = "Mã bao không tồn tại" });
                        }
                        var currentShippingStatus = bagExists.ShippingStatus;

                        if (currentShippingStatus != (int)BagShippingStatus.InTransit
                            && currentShippingStatus != (int)BagShippingStatus.Delivery
                            && currentShippingStatus != (int)BagShippingStatus.Delivered
                            && currentShippingStatus != (int)BagShippingStatus.WarehouseTransfer
                            )
                        {
                            return new JsonResult(new { Success = false, StatusCode = 400, Message = "Bao không ở trạng thái vận chuyển" });
                        }

                        if (bagExists.WarehouseStatus != (int)WarehouseStatus.OutOfStock)
                        {
                            return new JsonResult(new { Success = false, StatusCode = 400, Message = "Bao không ở trạng thái đã xuất kho" });
                        }

                        // Cập nhật trạng thái bao
                        try
                        {
                            int excuteResult = -1;
                            // Nếu đang vận chuyển quốc tế thì thực hiện nhập kho
                            if (bagExists.ShippingStatus == (int)BagShippingStatus.InTransit)
                            {
                                excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Bag_UpdateImportStatus", CommandType.StoredProcedure,
                                 new[]
                                 {
                                    new SqlParameter("@bagId", SqlDbType.Int) { Value = bagExists.Id },
                                    new SqlParameter("@bagStatus", SqlDbType.Int) { Value = (int)BagShippingStatus.GoToWarehouse },
                                    new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock },
                                    new SqlParameter("@orderStatus", SqlDbType.Int) { Value = (int)OrderStatus.InVietnamWarehouse },
                                    new SqlParameter("@packageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.InWarehouseVN },
                                    new SqlParameter("@bagType", SqlDbType.Int) { Value = bagExists.BagType },
                                    new SqlParameter("@warehouseId", SqlDbType.Int) { Value = _pbtAppSession.WarehouseId.Value }
                                 });

                                // Gửi log tập trung sử dụng EntityAuditLogApiClient

                                var bagDto = ObjectMapper.Map<BagDto>(bagExists);
                                var bagAuditLog = new EntityAuditLogDto()
                                {
                                    EntityId = bagDto.Id.ToString(),
                                    EntityType = nameof(Bag),
                                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                                    Title = $"Bao hàng #{bagDto.Id} - {bagDto.BagCode} được nhập kho VN",
                                    UserId = _pbtAppSession.UserId,
                                    UserName = _pbtAppSession.UserName,
                                    Data = JsonConvert.SerializeObject(bagDto),
                                };

                                _entityAuditLogApiClient.SendAsync(bagAuditLog);
                                if (bagExists.BagType == (int)BagTypeEnum.SeparateBag)
                                {
                                    // Lấy đơn hàng đã được cập bến về kho VN để update
                                    var ordersInBag = await GetOrdersByBagWithStatus(bagExists.Id, (int)OrderStatus.InVietnamWarehouse);
                                    foreach (var order in ordersInBag)
                                    {
                                        // add log
                                        _entityChangeLoggerAppService.LogChangeAsync<OrderDto>(
                                            null
                                            , order
                                            , "updated"
                                            , $"Cập nhật trạng thái đơn hàng sang <b>{OrderStatus.InVietnamWarehouse.GetDescription()}</b>"
                                            , true
                                        );
                                        var orderAuditLog = new EntityAuditLogDto()
                                        {
                                            EntityId = order.Id.ToString(),
                                            EntityType = nameof(Order),
                                            MethodName = EntityAuditLogMethodName.Update.ToString(),
                                            Title = $"Đơn hàng #{order.Id} - {order.WaybillNumber} được cập nhật trạng thái thông qua nhập bao #{bagExists.Id} - {bagExists.BagCode}",
                                            UserId = _pbtAppSession.UserId,
                                            UserName = _pbtAppSession.UserName,
                                            Data = JsonConvert.SerializeObject(order),
                                        };
                                        _entityAuditLogApiClient.SendAsync(orderAuditLog);
                                    }

                                }
                            }
                            else
                            {
                                // Trường hợp kiện bị trả về
                                excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Bag_ReturnImport", CommandType.StoredProcedure,
                                 new[]
                                 {
                                                new SqlParameter("@bagId", SqlDbType.Int) { Value = bagExists.Id },
                                                new SqlParameter("@bagStatus", SqlDbType.Int) { Value = (int)BagShippingStatus.GoToWarehouse },
                                                new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.InStock },
                                                new SqlParameter("@orderStatus", SqlDbType.Int) { Value = (int)OrderStatus.InVietnamWarehouse },
                                                new SqlParameter("@packageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.InWarehouseVN },
                                                new SqlParameter("@bagType", SqlDbType.Int) { Value = bagExists.BagType },
                                                new SqlParameter("@warehouseId", SqlDbType.Int) { Value = _pbtAppSession.WarehouseId ?? 7 }
                                 });
                            }
                            // add log
                            _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                                null
                                , bagExists
                                , "updated"
                                , $"Cập nhật trạng thái vận chuyển từ <b>{((BagShippingStatus)bagExists.ShippingStatus).GetDescription()}</b> sang <b>{BagShippingStatus.GoToWarehouse.GetDescription()}</b>"
                                , true
                            );

                            // get list package of bag
                            var packages = await GetPackagesByBagIdAsync(bagExists.Id);
                            foreach (var package in packages)
                            {
                                // add log
                                _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                                    null
                                    , package
                                    , "updated"
                                    , $"Cập nhật trạng thái vận chuyển từ <b>{PackageDeliveryStatusEnum.Shipping.GetDescription()}</b> sang <b>{PackageDeliveryStatusEnum.InWarehouseVN.GetDescription()}</b>"
                                    , true
                                );

                                var packageAuditLog = new EntityAuditLogDto()
                                {
                                    EntityId = package.Id.ToString(),
                                    EntityType = nameof(Package),
                                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                                    Title = $"Kiện #{package.Id} - {package.PackageNumber} nhập kho VN cùng bao #{bagExists.Id} - {bagExists.BagCode}",
                                    UserId = _pbtAppSession.UserId,
                                    UserName = _pbtAppSession.UserName,
                                    Data = JsonConvert.SerializeObject(package),
                                };
                                _entityAuditLogApiClient.SendAsync(packageAuditLog);
                            }


                            if (currentShippingStatus != (int)BagShippingStatus.InTransit)
                            {
                                await UpdateWalletReturnByBag(bagExists.Id, bagExists.BagType, bagExists.BagCode, bagExists.IsWeightCover, bagExists.WeightCoverFee, bagExists.CustomerId.Value);
                            }

                            if (bagExists.BagType == (int)BagTypeEnum.SeparateBag && bagExists.CustomerId.HasValue && bagExists.CustomerId > 0)
                            {
                                //get customer
                                var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == bagExists.CustomerId);
                                await _barCodeRepository.InsertAsync(new BarCode
                                {
                                    ScanCode = input.ScanCode,
                                    CodeType = input.CodeType,
                                    Action = input.ActionType,
                                    SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                                    CreatorUserName = _pbtAppSession.UserName,
                                    CustomerName = customer?.Username
                                });
                            }
                            else
                            {
                                await _barCodeRepository.InsertAsync(new BarCode
                                {
                                    ScanCode = input.ScanCode,
                                    CodeType = input.CodeType,
                                    Action = input.ActionType,
                                    SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                                    CreatorUserName = _pbtAppSession.UserName,
                                });
                            }

                            return new JsonResult(new
                            {
                                Success = true,
                                StatusCode = 200,
                                Message = "Cập nhật trạng thái bao thành công"
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Error updating import status for bag {input.ScanCode}: {ex.Message}", ex);
                            throw new UserFriendlyException($"Error updating import status: {ex.Message}");
                        }

                    }
                case CodeType.Order:
                case CodeType.Waybill:
                    break;
                default:
                    return new JsonResult(new { Success = false, StatusCode = 400, Message = "Loại mã không hợp lệ" });

            }

            return new JsonResult(new
            {
                Success = false,
                StatusCode = 400,
                Value = new { Message = "Yêu cầu không hợp lệ" }
            });
        }

        private async Task<JsonResult> CNExport(OperateActionDto input)
        {
            var bagExists = await GetBagWithCustomerNameByCode(input.ScanCode);
            if (bagExists == null)
                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = 400,
                    Value = new { Message = "Dữ liệu không tồn tại" }
                });

            if (bagExists.ShippingStatus != (int)BagShippingStatus.WaitingForShipping || bagExists.WarehouseStatus != (int)WarehouseStatus.InStock)
            {
                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = 400,
                    Value = new { Message = "Bao không có ở kho" }
                });
            }

            await _barCodeRepository.InsertAsync(new BarCode
            {
                ScanCode = input.ScanCode,
                CodeType = input.CodeType,
                Action = input.ActionType,
                SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                CreatorUserName = _pbtAppSession.UserName,
                CustomerName = bagExists.CustomerName
            });

            // Cập nhật trạng thái bao
            try
            {
                var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Bag_UpdateExportStatus", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@bagId", SqlDbType.Int) { Value = bagExists.Id },
                        new SqlParameter("@bagStatus", SqlDbType.Int) { Value = (int)BagShippingStatus.InTransit },
                        new SqlParameter("@warehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.OutOfStock },
                        new SqlParameter("@orderStatus", SqlDbType.Int) { Value = (int)OrderStatus.InTransit },
                        new SqlParameter("@packageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.Shipping }
                    });

                var bagDto = new BagDto()
                {
                    Id = bagExists.Id,
                    BagCode = bagExists.BagCode,
                };
                _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                   null
                   , bagDto
                   , "updated"
                   , $"Cập nhật trạng thái vận chuyển từ <b>{BagShippingStatus.WaitingForShipping.GetDescription()}</b> sang <b>{BagShippingStatus.InTransit.GetDescription()}</b>"
                   , true
               );

                var bagAuditLog = new EntityAuditLogDto()
                {
                    EntityId = bagExists.Id.ToString(),
                    EntityType = nameof(Bag),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Bao #{bagExists.Id} - {bagExists.BagCode} được xuất khỏi kho TQ",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(bagExists),
                };

                _entityAuditLogApiClient.SendAsync(bagAuditLog);

                // get list package of bag
                var packages = await GetPackagesByBagIdAsync(bagExists.Id);
                foreach (var package in packages)
                {

                    // add log
                    _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                        null
                        , package
                        , "updated"
                        , $"Cập nhật trạng thái vận chuyển từ <b>{PackageDeliveryStatusEnum.WaitingForShipping.GetDescription()}</b> sang <b>{PackageDeliveryStatusEnum.Shipping.GetDescription()}</b>"
                        , true
                    );

                    var packageAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = package.Id.ToString(),
                        EntityType = nameof(Package),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                        Title = $"Kiện #{package.Id} - {package.PackageNumber} xuất khỏi kho TQ cùng bao #{bagExists.Id} - {bagExists.BagCode}",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(package),
                    };

                    _entityAuditLogApiClient.SendAsync(packageAuditLog);


                }

                var ordersInBag = await GetOrdersByBagWithStatus(bagExists.Id, (int)OrderStatus.InTransit);

                foreach (var order in ordersInBag)
                {
                    // add log
                    _entityChangeLoggerAppService.LogChangeAsync<OrderDto>(
                        null
                        , order
                        , "updated"
                        , $"Cập nhật trạng thái đơn hàng sang <b>{OrderStatus.InTransit.GetDescription()}</b>"
                        , true
                    );
                    var orderAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = order.Id.ToString(),
                        EntityType = nameof(Order),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                        Title = $"Đơn hàng #{order.Id} - {order.WaybillNumber} được cập nhật trạng thái thông qua xuất bao #{bagExists.Id} - {bagExists.BagCode}",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(order),
                    };
                    _entityAuditLogApiClient.SendAsync(orderAuditLog);

                }

                return new JsonResult(new
                {
                    Success = true,
                    StatusCode = 200,
                    Value = new { Message = "Xuất kho thành công" }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating export status for bag {input.ScanCode}: {ex.Message}", ex);
                throw new UserFriendlyException($"Error updating import status: {ex.Message}");
            }
        }

        private async Task<JsonResult> VNWarehouseExport(OperateActionDto input)
        {
            try
            {
                if (input.CodeType == (int)CodeType.Bag)
                {
                    var bagExists = await GetBagWithCustomerNameByCode(input.ScanCode);
                    if (bagExists == null)
                    {
                        return new JsonResult(new
                        {
                            Success = false,
                            StatusCode = 400,
                            Value = new { Message = "Dữ liệu không tồn tại" }
                        });
                    }

                    if (bagExists.ShippingStatus != (int)BagShippingStatus.GoToWarehouse || bagExists.WarehouseStatus != (int)WarehouseStatus.InStock)
                    {
                        return new JsonResult(new
                        {
                            Success = false,
                            StatusCode = 400,
                            Value = new { Message = "Bao không có ở kho" }
                        });
                    }
                    var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_BagS_UpdateVNExportStatus", CommandType.StoredProcedure,
                        new[]
                    {
                        new SqlParameter("@BagId", SqlDbType.BigInt) { Value = bagExists.Id },
                        new SqlParameter("@BagStatus", SqlDbType.Int) { Value = (int)BagShippingStatus.WarehouseTransfer },
                        new SqlParameter("@WarehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.OutOfStock },
                        new SqlParameter("@OrderStatus", SqlDbType.Int) { Value = (int)OrderStatus.WarehouseTransfer },
                        new SqlParameter("@PackageStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.WarehouseTransfer },
                        new SqlParameter("@ModifierUserId", SqlDbType.BigInt) { Value = _pbtAppSession.UserId.Value }
                    });

                    await _barCodeRepository.InsertAsync(new BarCode
                    {
                        ScanCode = input.ScanCode,
                        CodeType = input.CodeType,
                        Action = input.ActionType,
                        SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                        CreatorUserName = _pbtAppSession.UserName,
                        CustomerName = bagExists.CustomerName
                    });

                    var bagDto = new BagDto()
                    {
                        Id = bagExists.Id,
                        BagCode = bagExists.BagCode,
                    };
                    // add log bag
                    _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                       null
                       , bagDto
                       , "updated"
                       , $"Cập nhật trạng thái vận chuyển từ <b>{BagShippingStatus.GoToWarehouse.GetDescription()}</b> sang <b>{BagShippingStatus.InTransit.GetDescription()}</b>"
                       , true
                   );

                    var bagAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = bagExists.Id.ToString(),
                        EntityType = nameof(Bag),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                        Title = $"Bao #{bagExists.Id} - {bagExists.BagCode} được xuất khỏi kho VN",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(bagExists),
                    };

                    _entityAuditLogApiClient.SendAsync(bagAuditLog);

                    // get list package of bag
                    var packages = await GetPackagesByBagIdAsync(bagExists.Id);
                    foreach (var package in packages)
                    {
                        // add log
                        _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                            null
                            , package
                            , "updated"
                            , $"Cập nhật trạng thái vận chuyển từ <b>{PackageDeliveryStatusEnum.InWarehouseVN.GetDescription()}</b> sang <b>{PackageDeliveryStatusEnum.Shipping.GetDescription()}</b>"
                            , true
                        );

                        var packageAuditLog = new EntityAuditLogDto()
                        {
                            EntityId = package.Id.ToString(),
                            EntityType = nameof(Package),
                            MethodName = EntityAuditLogMethodName.Update.ToString(),
                            Title = $"Kiện #{package.Id} - {package.PackageNumber} xuất khỏi kho VN cùng bao #{bagExists.Id} - {bagExists.BagCode}",
                            UserId = _pbtAppSession.UserId,
                            UserName = _pbtAppSession.UserName,
                            Data = JsonConvert.SerializeObject(package),
                        };

                        _entityAuditLogApiClient.SendAsync(packageAuditLog);
                    }

                    var ordersInBag = await GetOrdersByBagWithStatus(bagExists.Id, (int)OrderStatus.InTransit);

                    foreach (var order in ordersInBag)
                    {
                        // add log
                        _entityChangeLoggerAppService.LogChangeAsync<OrderDto>(
                            null
                            , order
                            , "updated"
                            , $"Cập nhật trạng thái đơn hàng sang <b>{OrderStatus.InTransit.GetDescription()}</b>"
                            , true
                        );
                        var orderAuditLog = new EntityAuditLogDto()
                        {
                            EntityId = order.Id.ToString(),
                            EntityType = nameof(Order),
                            MethodName = EntityAuditLogMethodName.Update.ToString(),
                            Title = $"Đơn hàng #{order.Id} - {order.WaybillNumber} được cập nhật trạng thái thông qua xuất bao #{bagExists.Id} - {bagExists.BagCode}",
                            UserId = _pbtAppSession.UserId,
                            UserName = _pbtAppSession.UserName,
                            Data = JsonConvert.SerializeObject(order),
                        };
                        _entityAuditLogApiClient.SendAsync(orderAuditLog);
                    }

                    return new JsonResult(new
                    {
                        Success = true,
                        StatusCode = 200,
                        Value = new { Message = "Xuất kho thành công" }
                    });
                }
                else if (input.CodeType == (int)CodeType.Package)
                {
                    var packageExisted = await GetPackageByCodeAsync(input.ScanCode);
                    if (packageExisted == null)
                    {
                        return new JsonResult(new
                        {
                            Success = false,
                            StatusCode = 400,
                            Value = new { Message = "Kiện không tồn tại" }
                        });
                    }

                    var currentShippingStatus = packageExisted.ShippingStatus;
                    if (currentShippingStatus != (int)PackageDeliveryStatusEnum.InWarehouseVN || packageExisted.WarehouseStatus != (int)WarehouseStatus.InStock)
                    {
                        return new JsonResult(new
                        {
                            Success = false,
                            StatusCode = 400,
                            Value = new { Message = "Kiện không có ở kho" }
                        });
                    }

                    if (packageExisted.BagId.HasValue && packageExisted.BagId > 0)
                    {
                        return new JsonResult(new
                        {
                            Success = false,
                            StatusCode = 400,
                            Value = new { Message = "Kiện đang trong bao, không thể xuất kho riêng lẻ" }
                        });
                    }

                    // Lưu ở đây để xác định trước khi thay đổi
                    var order = _orderRepository.FirstOrDefault(x => x.Id == packageExisted.OrderId);

                    var statusCodePr = new SqlParameter("@StatusCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var messagePr = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                    var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_Packages_VNWarehouseExport", CommandType.StoredProcedure,
                        new[]
                        {
                            new SqlParameter("@PackageId", SqlDbType.Int) { Value = packageExisted.Id },
                            new SqlParameter("@CurrentWarehouseId", SqlDbType.Int) { Value = _pbtAppSession.WarehouseId.Value },
                            new SqlParameter("@ModifierUserId", SqlDbType.BigInt) { Value = _pbtAppSession.UserId.Value },
                            new SqlParameter("@ShippingStatus", SqlDbType.Int) { Value = (int)PackageDeliveryStatusEnum.WarehouseTransfer },
                            new SqlParameter("@WarehouseStatus", SqlDbType.Int) { Value = (int)WarehouseStatus.OutOfStock },
                            new SqlParameter("@OrderStatus", SqlDbType.Int) { Value = (int)OrderStatus.WarehouseTransfer},
                            statusCodePr,
                            messagePr
                        }
                    );

                    // add log
                    _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                        null
                        , packageExisted
                        , "updated"
                        , $"Cập nhật trạng thái vận chuyển từ <b>{PackageDeliveryStatusEnum.InWarehouseVN.GetDescription()}</b> sang <b>{PackageDeliveryStatusEnum.Shipping.GetDescription()}</b>"
                        , true
                    );

                    var packageAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = packageExisted.Id.ToString(),
                        EntityType = nameof(Package),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                        Title = $"Kiện hàng #{packageExisted.Id} - {packageExisted.PackageNumber} được nhập kho VN",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(packageExisted),

                    };
                    _entityAuditLogApiClient.SendAsync(packageAuditLog);

                    // update order status
                    if (order != null)
                    {
                        // Nếu trạng thái đơn hàng đã là InVietnamWarehouse thì không cập nhật nữa
                        if (order.OrderStatus < (int)OrderStatus.InVietnamWarehouse)
                        {
                            order.OrderStatus = (int)OrderStatus.InVietnamWarehouse;
                            _entityChangeLoggerAppService.LogChangeAsync<Order>(
                                null
                                , order
                                , "updated"
                                , $"Cập nhật trạng thái đơn hàng sang <b>{OrderStatus.InVietnamWarehouse.GetDescription()}</b>"
                                , true
                            );

                            var orderDto = ObjectMapper.Map<OrderDto>(order);

                            var orderAuditLog = new EntityAuditLogDto()
                            {
                                EntityId = packageExisted.Id.ToString(),
                                EntityType = nameof(Order),
                                MethodName = EntityAuditLogMethodName.Update.ToString(),
                                Title = $"Đơn hàng #{orderDto.Id} - {orderDto.WaybillNumber} được cập nhật trạng thái thông qua nhập kiện #{packageExisted.Id} - {packageExisted.PackageNumber}",
                                UserId = _pbtAppSession.UserId,
                                UserName = _pbtAppSession.UserName,
                                Data = JsonConvert.SerializeObject(orderDto),

                            };
                            _entityAuditLogApiClient.SendAsync(orderAuditLog);
                        }
                    }

                    // get customer
                    //   var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == packageExisted.CustomerId);

                    await _barCodeRepository.InsertAsync(new BarCode
                    {
                        ScanCode = input.ScanCode,
                        CodeType = input.CodeType,
                        Action = input.ActionType,
                        SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                        CreatorUserName = _pbtAppSession.UserName,
                        CustomerName = order.CustomerName,
                    });

                    return new JsonResult(new
                    {
                        Success = true,
                        StatusCode = 200,
                        Value = new
                        {
                            Message = "Xuất kho thành công"
                        }
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Success = false,
                        StatusCode = 400,
                        Value = new { Message = "Loại mã không hợp lệ" }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in VNWarehouseExport: {ex.Message}", ex);
                return new JsonResult(new
                {
                    Success = false,
                    StatusCode = 500,
                    Value = new { Message = "Lỗi hệ thống" }
                });
            }
        }

        private async Task<JsonResult> CNImport(OperateActionDto input)
        {

            // Nếu là kiện thì thực hiện tạo đơn sau đó tạo kiện cho đơn đó`
            switch ((CodeType)input.CodeType)
            {
                case CodeType.Package:
                case CodeType.Bag:
                    break;
                case CodeType.Order:
                case CodeType.Waybill:
                    {
                        var scanCode = input.ScanCode.Trim();
                        var status = new SqlParameter("@StatusCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                        var orderId = new SqlParameter("@OrderId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var note = new SqlParameter("@Note", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                        var customerName = new SqlParameter("@CustomerName", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };

                        await ConnectDb.ExecuteNonQueryAsync(
                            "SP_Orders_ReceiveOrder",
                            CommandType.StoredProcedure,
                            new[]
                            {
                            new SqlParameter("@ScanCode", scanCode),
                            new SqlParameter("@WarehouseId", _pbtAppSession.WarehouseId.Value),
                            new SqlParameter("@UserId", _pbtAppSession.UserId.Value),
                            new SqlParameter("@UserName", _pbtAppSession.UserName),
                            status, message, orderId, note, customerName
                            }
                        );

                        var orderDto = new OrderDto()
                        {
                            Id = Convert.ToInt64(orderId.Value),
                            WaybillNumber = scanCode,
                            CustomerName = Convert.ToString(customerName.Value)
                        };

                        _entityChangeLoggerAppService.LogChangeAsync<OrderDto>(
                           null
                           , orderDto
                           , "updated"
                           , $"Quét vận đơn, " + (customerName.Value != null ? $"khách hàng: <b>{customerName.Value}</b>, " : "") + $"trạng thái đơn hàng được cập nhật thành <b>{OrderStatus.InChinaWarehouse.GetDescription()}</b>"
                           , true
                       );

                        // Insert log barcode
                        await _barCodeRepository.InsertAsync(new BarCode
                        {
                            ScanCode = scanCode,
                            CodeType = input.CodeType,
                            Action = input.ActionType,
                            CreatorUserName = _pbtAppSession.UserName,
                            Content = Convert.ToString(note.Value),
                            SourceWarehouseId = _pbtAppSession.WarehouseId.Value,
                            CustomerName = Convert.ToString(customerName.Value)
                        });

                        return new JsonResult(new
                        {
                            Success = true,
                            StatusCode = (int)status.Value,
                            Message = message.Value?.ToString(),
                            OrderId = orderId.Value != DBNull.Value ? (long?)orderId.Value : null,
                            Note = note.Value?.ToString()
                        });
                    }

                default:
                    return new JsonResult(new { Success = false, StatusCode = 400, Message = "Loại mã không hợp lệ" });

            }

            // Implementation for CNImport if needed
            return new JsonResult(new
            {
                Success = false,
                StatusCode = 501,
                Value = new { Message = "Chức năng CNImport chưa được triển khai." }
            });
        }

        private async Task<BagWithCustomerNameDto> GetBagWithCustomerNameByCode(string code)
        {
            //CREATE PROCEDURE[dbo].[SP_Bags_GetWithCustomerByCode]
            //@BagCode NVARCHAR(100)
            var data = await ConnectDb.GetItemAsync<BagWithCustomerNameDto>("SP_Bags_GetWithCustomerByCode", CommandType.StoredProcedure, new[]
            {
                new SqlParameter("@BagCode", SqlDbType.NVarChar, 100) { Value = code }
            });

            return data;
        }

        private async Task<List<PackageDto>> GetPackagesByBagIdAsync(int bagId)
        {
            var data = await ConnectDb.GetListAsync<PackageDto>("SP_Packages_GetByBagId", CommandType.StoredProcedure, new[]
            {
                new SqlParameter("@BagId", SqlDbType.Int) { Value = bagId }
            });
            return data;
        }

        public async Task<JsonObject> UpdateWalletReturnByPackage(decimal packageTotalPrice, long packageId, string packageNumber, long customerId)
        {

            var totalMoney = packageTotalPrice;

            if (totalMoney < 0)
            {
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 400,
                    ["Message"] = "Tổng tiền của kiện hàng không hợp lệ."
                };
            }
            if (totalMoney == 0)
            {
                return new JsonObject
                {
                    ["Success"] = true,
                    ["Code"] = 200,
                    ["Message"] = "Kiện hàng không có tiền để hoàn."
                };
            }

            var currentFundAccount = (await _fundAccountAppService.GetFundAccountsByCurrentUserAsync()).FirstOrDefault();
            // Kiểm tra tài khoản quỹ
            if (currentFundAccount == null)
            {
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 400,
                    ["Message"] = "Tài khoản quỹ chưa được gán vào kho."
                };
            }

            // Lấy thông tin khách hàng
            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == customerId);
            if (customer == null)
            {
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 400,
                    ["Message"] = "Khách hàng không tồn tại."
                };
            }

            try
            {

                var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("GD");
                // Ghi nhận giao dịch vào bảng Transaction
                var transaction = new Transaction
                {
                    TransactionId = identityCode.Code,
                    RecipientPayer = customerId,
                    Amount = totalMoney,
                    TransactionType = (int)TransactionTypeEnum.Deposit, // Loại giao dịch: Hoàn tiền
                    TransactionDirection = (int)TransactionDirectionEnum.Expense, // Giao dịch cộng tiền
                    TransactionContent = $"Hoàn tiền cho kiện hàng: #{packageNumber}",
                    RefCode = packageNumber,
                    CreationTime = DateTime.Now,
                    CreatorUserId = _pbtAppSession.UserId
                };
                await _transactionRepository.InsertAsync(transaction);

                // Ghi nhận giao dịch vào bảng CustomerTransaction
                var customerTransaction = new CustomerTransaction
                {
                    CustomerId = customerId,
                    BalanceAfterTransaction = customer.CurrentAmount + totalMoney,
                    Amount = totalMoney,
                    TransactionType = (int)TransactionTypeEnum.Deposit,
                    TransactionDate = DateTime.Now,
                    CreatorUserId = _pbtAppSession.UserId,
                    Description = $"Hoàn tiền cho kiện hàng: #{packageNumber}",
                    ReferenceCode = $"trans: #{identityCode.Code} package: #{packageNumber}",
                };
                await _walletTransactionRepository.InsertAsync(customerTransaction);



                // Ghi log thay đổi
                await _entityChangeLoggerAppService.LogChangeAsync<Customer>(
                    null,
                    customer,
                    "updated",
                    $"Hoàn tiền {totalMoney:N0} VND cho khách hàng {customer.Username} (kiện hàng: #{packageNumber})",
                    true
                );

                // Cộng tiền vào ví của khách hàng
                customer.CurrentAmount += totalMoney;
                await _customerRepository.UpdateAsync(customer);

                Logger.Info($"WalletReturnByPackage - Hoàn tiền thành công cho khách hàng {customer.Username}, số tiền: {totalMoney:N0}");

                return new JsonObject
                {
                    ["Success"] = true,
                    ["Code"] = 200,
                    ["Message"] = "Hoàn tiền thành công."
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"WalletReturnByPackage - Lỗi khi hoàn tiền: {ex.Message}", ex);
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 500,
                    ["Message"] = "Có lỗi xảy ra khi hoàn tiền."
                };
            }
        }

        public async Task<JsonObject> UpdateWalletReturnByBag(int bagId, int bagType, string bagCode, bool isWeightCover, decimal? weightCoverFee, long customerId)
        {
            //  Logger.Info($"WalletReturnByBag - bag: {JsonConvert.SerializeObject(bag)}");

            // Kiểm tra tổng tiền của bao
            decimal totalMoney = 0;

            if (bagType == (int)BagTypeEnum.SeparateBag)
            {
                var packagesInBag = await GetPackagesByBagIdAsync(bagId);
                foreach (var pkg in packagesInBag)
                {
                    if (pkg.TotalPrice.HasValue && pkg.TotalPrice > 0)
                    {
                        totalMoney += pkg.TotalPrice.Value;
                    }
                }
                if (isWeightCover && weightCoverFee.HasValue && weightCoverFee > 0)
                {
                    totalMoney += weightCoverFee.Value;
                }
            }


            if (totalMoney < 0)
            {
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 400,
                    ["Message"] = "Tổng tiền của bao không hợp lệ."
                };
            }


            if (totalMoney == 0)
            {
                return new JsonObject
                {
                    ["Success"] = true,
                    ["Code"] = 200,
                    ["Message"] = "Kiện hàng không có tiền để hoàn."
                };
            }

            var currentFundAccount = (await _fundAccountAppService.GetFundAccountsByCurrentUserAsync()).FirstOrDefault();
            // Kiểm tra tài khoản quỹ
            if (currentFundAccount == null)
            {
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 400,
                    ["Message"] = "Tài khoản quỹ chưa được gán vào kho."
                };
            }

            // Lấy thông tin khách hàng
            var customer = await _customerRepository.FirstOrDefaultAsync(x => x.Id == customerId);
            if (customer == null)
            {
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 400,
                    ["Message"] = "Khách hàng không tồn tại."
                };
            }

            try
            {
                // Tạo mã giao dịch
                var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("GD");

                // Ghi nhận giao dịch vào bảng Transaction
                var transaction = new Transaction
                {
                    TransactionId = identityCode.Code,
                    RecipientPayer = customer.Id,
                    Amount = totalMoney,
                    TransactionType = (int)TransactionTypeEnum.Deposit, // Loại giao dịch: Hoàn tiền
                    TransactionDirection = (int)TransactionDirectionEnum.Expense, // Giao dịch cộng tiền
                    TransactionContent = $"Hoàn tiền cho bao: #{bagCode}",
                    RefCode = bagCode,
                    CreationTime = DateTime.Now,
                    CreatorUserId = _pbtAppSession.UserId
                };
                await _transactionRepository.InsertAsync(transaction);

                // Ghi nhận giao dịch vào bảng CustomerTransaction
                var customerTransaction = new CustomerTransaction
                {
                    CustomerId = customer.Id,
                    BalanceAfterTransaction = customer.CurrentAmount + totalMoney,
                    Amount = totalMoney,
                    TransactionType = (int)TransactionTypeEnum.Deposit,
                    TransactionDate = DateTime.Now,
                    CreatorUserId = _pbtAppSession.UserId,
                    Description = $"Hoàn tiền cho bao: #{bagCode}",
                    ReferenceCode = $"trans: #{identityCode.Code} bag: #{bagCode}",
                };
                await _walletTransactionRepository.InsertAsync(customerTransaction);

                // Ghi log thay đổi
                await _entityChangeLoggerAppService.LogChangeAsync<Customer>(
                    null,
                    customer,
                    "updated",
                    $"Hoàn tiền {totalMoney:N0} VND cho khách hàng: {customer.Username} (bao: #{bagCode})",
                    true
                );

                // Cộng tiền vào ví của khách hàng
                customer.CurrentAmount += totalMoney;
                await _customerRepository.UpdateAsync(customer);

                Logger.Info($"WalletReturnByBag - Hoàn tiền thành công cho khách hàng: {customer.Username}, số tiền: {totalMoney:N0}");

                return new JsonObject
                {
                    ["Success"] = true,
                    ["Code"] = 200,
                    ["Message"] = "Hoàn tiền thành công."
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"WalletReturnByBag - Lỗi khi hoàn tiền: {ex.Message}", ex);
                return new JsonObject
                {
                    ["Success"] = false,
                    ["Code"] = 500,
                    ["Message"] = "Có lỗi xảy ra khi hoàn tiền."
                };
            }
        }

        private async Task<PackageDto> GetPackageByCodeAsync(string packageCode)
        {
            return await ConnectDb.GetItemAsync<PackageDto>("SP_Packages_GetByPackageNumber",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@PackageNumber",
                    Value = packageCode
                } });
        }

        public async Task<List<OrderDto>> GetOrdersByBagWithStatus(int bagId, int orderStatus)
        {
            var data = await ConnectDb.GetListAsync<OrderDto>("SP_Orders_GetByBagAndStatus", CommandType.StoredProcedure, new[]
            {
                new SqlParameter("@BagId", SqlDbType.Int) { Value = bagId },
                new SqlParameter("@OrderStatus", SqlDbType.Int) { Value = orderStatus}
            });

            return data;
        }
    }
}