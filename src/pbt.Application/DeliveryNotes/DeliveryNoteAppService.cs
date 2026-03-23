using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Extensions;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.Formula.Functions;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Users;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.ConfigurationSettings;
using pbt.Core;
using pbt.Customers;
using pbt.Customers.Dto;
using pbt.DeliveryNotes.Dto;
using pbt.Entities;
using pbt.EntityAuditLogs;
using pbt.EntityAuditLogs.Dto;
using pbt.FundAccounts;
using pbt.FundAccounts.Dto;
using pbt.OrderNumbers;
using pbt.Orders.Dto;
using pbt.Packages.Dto;
using pbt.ShippingPartners;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace pbt.DeliveryNotes
{

    [Authorize]
    public class DeliveryNoteAppService :
        AsyncCrudAppService<DeliveryNote, DeliveryNoteDto, int, PagedResultRequestDto, CreateUpdateDeliveryNoteDto,
            DeliveryNoteDto>,
        IDeliveryNoteAppService
    {
        private readonly IRepository<Bag, int> _bagRepository;
        private readonly IRepository<Package, int> _packageRepository;
        private readonly UserManager _userManager;

        private readonly IRepository<Warehouse, int> _warehousesRepository;
        private readonly IRepository<Customer, long> _customerRepository;
        private readonly IRepository<DeliveryRequest, int> _deliveryRequestRepository;
        private readonly IRepository<DeliveryRequestOrder, int> _deliveryRequestOrderRepository;
        private readonly IRepository<Order, long> _orderRepository;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IRepository<Transaction, long> _transactionRepository;
        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly IRepository<CustomerTransaction, long> _walletTransactionRepository;
        private readonly pbtAppSession _pbtAppSession;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;
        private readonly IConfigurationSettingAppService _configurationSettingAppService;
        private readonly IRepository<ShippingPartner, int> _shippingPartnerRepository;
        private readonly string[] _roles;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEntityAuditLogApiClient _entityAuditLogApiClient;

        public DeliveryNoteAppService(
            IRepository<DeliveryNote> repository,
            IRepository<Bag, int> bagRepository,
            UserManager userManager,
            IRepository<Package, int> packageRepository,
            IRepository<Warehouse, int> warehousesRepository,
            IRepository<DeliveryRequest, int> deliveryRequestRepository,
            IRepository<DeliveryRequestOrder, int> deliveryRequestOrderRepository,
            IRepository<Order, long> orderRepository,
            IRepository<Customer, long> customerRepository,
            IIdentityCodeAppService identityCodeAppService,
            IRepository<Transaction, long> transactionRepository,
            IFundAccountAppService fundAccountAppService,
            IRepository<CustomerTransaction, long> walletTransactionRepository,
            pbtAppSession pbtAppSession,
            IEntityChangeLoggerAppService entityChangeLoggerAppService,
            IConfigurationSettingAppService configurationSettingAppService,
            IRepository<ShippingPartner> shippingPartnerRepository,
            IHttpContextAccessor httpContextAccessor,
            IEntityAuditLogApiClient entityAuditLogApiClient
        )
            : base(repository)
        {
            _bagRepository = bagRepository;
            _packageRepository = packageRepository;
            _userManager = userManager;
            _warehousesRepository = warehousesRepository;
            _deliveryRequestRepository = deliveryRequestRepository;
            _deliveryRequestOrderRepository = deliveryRequestOrderRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _identityCodeAppService = identityCodeAppService;
            _transactionRepository = transactionRepository;
            _fundAccountAppService = fundAccountAppService;
            _walletTransactionRepository = walletTransactionRepository;
            _pbtAppSession = pbtAppSession;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;
            _configurationSettingAppService = configurationSettingAppService;
            _shippingPartnerRepository = shippingPartnerRepository;
            _httpContextAccessor = httpContextAccessor;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();

            _entityAuditLogApiClient = entityAuditLogApiClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        /// 
        public override async Task<DeliveryNoteDto> GetAsync(EntityDto<int> input)
        {
            var deliveryNote = await Repository.GetAsync(input.Id);

            var exportDto = ObjectMapper.Map<DeliveryNoteDto>(deliveryNote);
            return exportDto;
        }

        public async Task<DeliveryNoteDto> GetWithCreatorInfoAsync(EntityDto<int> input)
        {
            var deliveryNote = await Repository.GetAsync(input.Id);

            if (deliveryNote == null)
            {
                throw new EntityNotFoundException(typeof(DeliveryNote), input.Id);
            }

            var exportDto = ObjectMapper.Map<DeliveryNoteDto>(deliveryNote);
            if (deliveryNote.CreatorUserId.HasValue)
            {
                var creatorUser = await _userManager.FindByIdAsync(deliveryNote.CreatorUserId.ToString());
                var creatorName = creatorUser?.FullName ?? "";

                if (deliveryNote.ExporterId.HasValue)
                {
                    var exporter = await _userManager.FindByIdAsync(deliveryNote.ExporterId.ToString());
                    if (exporter != null) exportDto.ExporterName = exporter.FullName;
                }

                if (deliveryNote.ExportWarehouse.HasValue)
                {
                    var warehouse =
                        await _warehousesRepository.FirstOrDefaultAsync(x => x.Id == deliveryNote.ExportWarehouse);
                    if (warehouse != null)
                    {
                        exportDto.ExportWarehouseName = warehouse.Name;
                    }
                }
                exportDto.CreatorName = creatorName; // Thêm trường tên người tạo vào DTO
            }

            return exportDto;
        }

        /// <summary>
        /// </summary>
        /// <param name="deliveryNoteId"></param>
        /// <returns></returns>
        public async Task<DeliveryNoteItemsDto> GetItemByDeliveryNote(int deliveryNoteId)
        {
            // get bag
            var bags = (await _bagRepository.GetAllAsync())
                .Include(x => x.Packages)
                .Where(x => x.DeliveryNoteId == deliveryNoteId);

            var bagDto = ObjectMapper.Map<List<BagDeliveryRequestDto>>(bags);

            foreach (var dto in bagDto)
            {
                var lstPackages = bags.FirstOrDefault(x => x.Id == dto.Id)?.Packages;
                dto.TotalPackages = lstPackages?.Count() ?? 0;

                if (lstPackages != null && lstPackages.Any())
                {
                    var firstPackage = lstPackages.First();
                    var deliveryOrderRequest = (await _deliveryRequestOrderRepository.GetAllAsync())
                        .FirstOrDefault(x => x.OrderId == firstPackage.OrderId);

                }
            }

            // get package
            var packages = (await _packageRepository.GetAllAsync()).Include(x => x.Bag)
                .Where(x => x.DeliveryNoteId == deliveryNoteId
                            && (x.Bag == null || x.Bag.BagType == (int)BagTypeEnum.InclusiveBag));
            var packageDeliveryRequestDto = ObjectMapper.Map<List<PackageDeliveryRequestDto>>(packages);
            List<PackageDeliveryRequestDto> packageDeliveryRequestDtoResult = new List<PackageDeliveryRequestDto>();
            foreach (var package in packageDeliveryRequestDto)
            {
                var deliveryOrderRequest =
                    (await _deliveryRequestOrderRepository.GetAllAsync()).FirstOrDefault(x =>
                        x.OrderId == package.OrderId);
                if (deliveryOrderRequest != null)
                {
                    var deliveryRequest =
                        (await _deliveryRequestRepository.GetAllAsync()).FirstOrDefault(x =>
                            x.Id == deliveryOrderRequest.DeliveryRequestId);
                    if (deliveryRequest != null)
                    {
                        package.DeliveryRequestOrderCode = deliveryRequest.RequestCode;
                    }
                }
                var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == package.OrderId);
                if (order != null)
                {
                    package.OrderCode = order?.OrderNumber;
                }

                packageDeliveryRequestDtoResult.Add(package);
            }

            return new DeliveryNoteItemsDto()
            {
                Bags = bagDto,
                Packages = packageDeliveryRequestDtoResult
            };
        }

        public async Task<ListResultDto<DeliveryNoteDto>> getDeliveryNotesFilter(
            PagedDeliveryNoteResultRequestDto input)
        {
            var query = this.CreateFilteredQuery(input);

            // get current user warehouse id
            var warehouseId = _pbtAppSession.WarehouseId;
            if (!PermissionChecker.IsGranted(PermissionNames.Pages_DeliveryNote_ViewAllWarehouse))
            {
                query = query.Where(x => x.ExportWarehouse == warehouseId);
            }

            if (input.CustomerId.HasValue && input.CustomerId > 0)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId);
            }
            if (input.Status.HasValue && input.Status >= 0)
            {
                query = query.Where(x => x.Status == input.Status);
            }
            if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
            {
                query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
            }
            var deliveryIdsByTrackingNumber = new List<int>();
            if (!string.IsNullOrEmpty(input.Keyword))
            {

                var deliveryIdsQuery = await ConnectDb.GetListAsync<DeliveryNoteIdDto>(
                    "SP_DeliveryNotes_GetIdsBySearchCodes",
                    CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter(){ ParameterName = "@keyword", Value = input.Keyword , SqlDbType = SqlDbType.NVarChar },
                    }
                );

                deliveryIdsByTrackingNumber = deliveryIdsQuery.Select(u => u.Id).ToList();

                query = query.Where(x => x.DeliveryNoteCode.Contains(input.Keyword)
                                         || x.Receiver.Contains(input.Keyword)
                                         || x.RecipientPhoneNumber.Contains(input.Keyword)
                                         || deliveryIdsByTrackingNumber.Contains(x.Id)
                                        );
            }

            if (input.StartCreateDate.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
            }
            if (input.EndCreateDate.HasValue)
            {
                query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
            }

            if (input.StartExportDateVN.HasValue)
            {
                query = query.Where(x => x.ExportTime >= input.StartExportDateVN.Value);
            }
            if (input.EndExportDateVN.HasValue)
            {
                query = query.Where(x => x.ExportTime <= input.EndExportDateVN.Value);
            }

            query = query.OrderByDescending(x => x.ExportTime).ThenByDescending(u => u.Id)

                .Include(x => x.Customer)
                .Include(x => x.ShippingPartner);

            var count = await query.CountAsync();

            var data = query.Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var exportDto = ObjectMapper.Map<List<DeliveryNoteDto>>(data);
            return new PagedResultDto<DeliveryNoteDto>()
            {
                Items = exportDto,
                TotalCount = DynamicQueryableExtensions.Count(query),
            };
        }



        public async Task<ListResultDto<DeliveryNoteDto>> getMyDeliveryNotesFilterAsync(
          PagedDeliveryNoteResultRequestDto input)
        {

            var permissionCheckResult = GetPermissionCheckerWithCustomerIds();

            if (permissionCheckResult.permissionCase <= 0)
            {
                return new PagedResultDto<DeliveryNoteDto>()
                {
                    Items = new List<DeliveryNoteDto>(),
                    TotalCount = 0,
                };
            }

            var permissionCase = permissionCheckResult.permissionCase;
            var customerIds = permissionCheckResult.CustomerIds;

            var customerIdsString = string.Join(",", customerIds);
            var totalCountParam = new SqlParameter
            {
                ParameterName = "@TotalCount",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@PermissionCase", permissionCase),
                new SqlParameter("@CustomerIds", customerIdsString),
                new SqlParameter("@CustomerId", input.CustomerId ?? -1),
                new SqlParameter("@Status", input.Status ?? -1),
                new SqlParameter("@Keyword", string.IsNullOrEmpty(input.Keyword) ? "" : input.Keyword),
                new SqlParameter("@StartCreateDate", input.StartCreateDate.HasValue ? (object)input.StartCreateDate.Value : DBNull.Value),
                new SqlParameter("@EndCreateDate", input.EndCreateDate.HasValue ? (object)input.EndCreateDate.Value : DBNull.Value),
                new SqlParameter("@StartExportDateVN", input.StartExportDateVN.HasValue ? (object)input.StartExportDateVN.Value : DBNull.Value),
                new SqlParameter("@EndExportDateVN", input.EndExportDateVN.HasValue ? (object)input.EndExportDateVN.Value : DBNull.Value),
                new SqlParameter("@ShippingPartnerId", input.ShippingPartnerId ?? -1),
                new SqlParameter("@SkipCount", input.SkipCount),
                new SqlParameter("@MaxResultCount", input.MaxResultCount),
                totalCountParam
            };
            var deliveryNotes = await ConnectDb.GetListAsync<DeliveryNoteDto>(
                "SP_DeliveryNotes_GetPaged",
                CommandType.StoredProcedure,
                sqlParameters
            );
            var totalCount = (int)(totalCountParam.Value ?? 0);
            return new PagedResultDto<DeliveryNoteDto>()
            {
                Items = deliveryNotes,
                TotalCount = totalCount,
            };

        }

        /// <summary>
        /// Xử lý thanh toán chi phí giao hàng cho khách hàng.
        /// Nếu số tiền phí giao hàng lớn hơn số dư hiện tại của khách hàng, phần vượt quá sẽ được cộng vào công nợ và số dư ví về 0.
        /// Nếu số dư đủ, trừ trực tiếp vào ví và ghi nhận giao dịch vào lịch sử giao dịch của khách hàng.
        /// Sau khi xử lý, cập nhật lại thông tin khách hàng.
        /// </summary>
        /// <param name="customerId">ID khách hàng</param>
        /// <param name="deliveryNote">Phiếu xuất kho liên quan</param>
        /// <param name="description">Mô tả giao dịch</param>
        /// <returns>Task bất đồng bộ</returns>
        private async Task<JsonObject> HandleCustomerPayment(Customer customer, DeliveryNote deliveryNote, int deliveryFeeReason)
        {
            // Lấy thông tin khách hàng
            //var customer = await _customerRepository.FirstOrDefaultAsync(u => u.Id == customerId);

            decimal totalFee = 0; // Tổng phí giao hàng khách cần thanh toán
            string description = $"Thanh toán phí giao hàng cho phiếu xuất kho #{deliveryNote.DeliveryNoteCode}";

            if (deliveryFeeReason == (int)DeliveryFeeType.WithoutFee) // Kho không thu tiền phí vận chuyển nội địa VN của khách hàng
            {
                totalFee = (deliveryNote.ShippingFee ?? 0);  // Tổng phí KH cần thanh toán chỉ bao gồm phí ship quốc tế
                Logger.Info($"Kho chịu phí VC nội địa: {deliveryNote.DeliveryFee} VNĐ, tổng phí KH cần thanh toán: {totalFee} VNĐ");
            }
            else // Kho thu tiền phí vận chuyển nội địa VN của khách hàng ( khách hàng chịu phí VN nội địa VN)
            {
                // Tổng phí KH cần thanh toán bao gồm cả phí ship quốc tế và phí ship nội địa VN
                totalFee = (deliveryNote.DeliveryFee ?? 0) + (deliveryNote.ShippingFee ?? 0);
                Logger.Info($"Khách hàng chịu phí VC nội địa: {deliveryNote.DeliveryFee} VNĐ, phí ship quốc tế: {deliveryNote.ShippingFee} VNĐ, tổng phí KH cần thanh toán: {totalFee} VNĐ");
            }

            // Sử dụng CurrentAmount cho cả số dư ví và công nợ (dương là số dư ví, âm là công nợ)
            customer.CurrentAmount -= totalFee;

            // Ghi log giao dịch
            var walletTransaction = new CustomerTransaction()
            {
                CustomerId = customer.Id,
                Amount = totalFee,
                TransactionType = (int)TransactionTypeEnum.Payment,
                BalanceAfterTransaction = customer.CurrentAmount,
                ReferenceCode = deliveryNote.Id + " | " + deliveryNote.DeliveryNoteCode,
                Description = description,
                TransactionDate = DateTime.Now,
            };
            await _walletTransactionRepository.InsertAsync(walletTransaction);

            // Cập nhật thông tin khách hàng
            await _customerRepository.UpdateAsync(customer);

            return new JsonObject
            {
                ["isSuccess"] = true,
                ["message"] = "Xử lý thanh toán phí giao hàng thành công."
            };
        }

        /// <summary>
        /// Xử lý thanh toán chi phí giao hàng cho khách hàng từ tài khoản quỹ của kho.
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="deliveryNote"></param>
        /// <param name="deliveryFeeReason"></param>
        /// <param name="currentFundAccount"></param>
        /// <returns></returns>
        private async Task<JsonObject> HandleFundPayment(Customer customer, DeliveryNote deliveryNote, int deliveryFeeReason, FundAccountDto currentFundAccount)
        {
            // Lấy thông tin khách hàng
            //var customer = await _customerRepository.FirstOrDefaultAsync(u => u.Id == customerId);

            decimal totalFee = 0; // Tổng phí giao hàng khách cần thanh toán
            string description = $"Thanh toán phí giao hàng cho phiếu xuất kho #{deliveryNote.DeliveryNoteCode}";

            if (deliveryFeeReason == (int)DeliveryFeeType.WithoutFee) // Kho không thu tiền phí vận chuyển nội địa VN của khách hàng
            {
                totalFee = (deliveryNote.ShippingFee ?? 0);  // Tổng phí KH cần thanh toán chỉ bao gồm phí ship quốc tế
                Logger.Info($"Kho chịu phí VC nội địa: {deliveryNote.DeliveryFee} VNĐ, tổng phí KH cần thanh toán: {totalFee} VNĐ");

                var deliveryFee = deliveryNote.DeliveryFee ?? 0;
                if (deliveryFee > 0)
                {
                    description += $" (Kho không thu tiền phí vận chuyển nội địa VN của khách hàng, nhưng thu hộ phí giao hàng: {deliveryNote.DeliveryFee} VNĐ)";

                    // Tạo phiếu chi: trừ phí vận chuyển từ tài khoản quỹ
                    var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("GD");
                    var transaction = new Transaction()
                    {
                        TransactionId = identityCode.Code,
                        Amount = deliveryFee,
                        TransactionType = (int)TransactionTypeEnum.Payment,
                        Status = (int)TransactionStatusEnum.Approved,
                        ExecutionSource = (int)TransactionSourceEnum.Auto,
                        TransactionDirection = (int)TransactionDirectionEnum.Expense,
                        TotalAmount = currentFundAccount.TotalAmount - deliveryFee,
                        Currency = "VNĐ",
                        TransactionContent = "Chi phí giao hàng (Kho không thu tiền của khách hàng)",
                        ExpensePurpose = "Chi phí giao hàng",
                        FundAccountId = currentFundAccount.Id,
                        RefCode = deliveryNote.Id + " | " + deliveryNote.DeliveryNoteCode,
                    };
                    await _transactionRepository.InsertAsync(transaction);

                    // Cập nhật số dư tài khoản quỹ
                    currentFundAccount.TotalAmount -= deliveryFee;
                    await _fundAccountAppService.UpdateAsync(currentFundAccount);
                }
            }
            else // Kho thu tiền phí vận chuyển nội địa VN của khách hàng ( khách hàng chịu phí VN nội địa VN)
            {
                // Tổng phí KH cần thanh toán bao gồm cả phí ship quốc tế và phí ship nội địa VN
                totalFee = (deliveryNote.DeliveryFee ?? 0) + (deliveryNote.ShippingFee ?? 0);
                Logger.Info($"Khách hàng chịu phí VC nội địa: {deliveryNote.DeliveryFee} VNĐ, phí ship quốc tế: {deliveryNote.ShippingFee} VNĐ, tổng phí KH cần thanh toán: {totalFee} VNĐ");


                // Tạo phiếu chi: trừ phí vận chuyển từ tài khoản quỹ
                var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("GD");
                var transaction = new Transaction()
                {
                    TransactionId = identityCode.Code,
                    Amount = (deliveryNote.DeliveryFee ?? 0),
                    TransactionType = (int)TransactionTypeEnum.Payment,
                    Status = (int)TransactionStatusEnum.Approved,
                    ExecutionSource = (int)TransactionSourceEnum.Manual,
                    TransactionDirection = (int)TransactionDirectionEnum.Expense,
                    TotalAmount = currentFundAccount.TotalAmount - (deliveryNote.DeliveryFee ?? 0),
                    Currency = "VNĐ",
                    TransactionContent = "Chi phí giao hàng (Kho không thu tiền của khách hàng)",
                    ExpensePurpose = "Chi phí giao hàng",
                    FundAccountId = currentFundAccount.Id,
                    RefCode = deliveryNote.Id + " | " + deliveryNote.DeliveryNoteCode,
                };
                await _transactionRepository.InsertAsync(transaction);

                // Cập nhật số dư tài khoản quỹ
                currentFundAccount.TotalAmount -= (deliveryNote.DeliveryFee ?? 0);
                await _fundAccountAppService.UpdateAsync(currentFundAccount);

            }

            //// Sử dụng CurrentAmount cho cả số dư ví và công nợ (dương là số dư ví, âm là công nợ)
            //customer.CurrentAmount -= totalFee;

            //// Ghi log giao dịch
            //var walletTransaction = new CustomerTransaction()
            //{
            //    CustomerId = customer.Id,
            //    Amount = totalFee,
            //    TransactionType = (int)TransactionTypeEnum.Payment,
            //    BalanceAfterTransaction = customer.CurrentAmount,
            //    ReferenceCode = deliveryNote.Id + " | " + deliveryNote.DeliveryNoteCode,
            //    Description = description,
            //    TransactionDate = DateTime.Now,
            //};
            //await _walletTransactionRepository.InsertAsync(walletTransaction);

            //// Cập nhật thông tin khách hàng
            //await _customerRepository.UpdateAsync(customer);

            return new JsonObject
            {
                ["isSuccess"] = true,
                ["message"] = "Xử lý thanh toán phí giao hàng thành công."
            };
        }
        


        /// <summary>
        /// Hàm này dùng để cập nhật thông tin phiếu xuất kho (DeliveryNote) và thực hiện các giao dịch liên quan.
        /// - Kiểm tra và lấy thông tin quỹ của kho nếu có lý do thu phí giao hàng và số tiền phí lớn hơn 0.
        /// - Kiểm tra trạng thái phiếu xuất kho, nếu đã xuất thì không cho phép cập nhật.
        /// - Cập nhật các trường thông tin của phiếu xuất kho như thời gian xuất, trạng thái, phí giao hàng, người nhận, ghi chú, v.v...
        /// - Ghi log thay đổi phiếu xuất kho.
        /// - Gọi store procedure để cập nhật trạng thái xuất kho của bao, kiện, đơn hàng liên quan.
        /// - Ghi log thay đổi cho từng bao và kiện thuộc phiếu xuất kho.
        /// - Thực hiện xử lý thanh toán phí giao hàng cho khách hàng (trừ ví hoặc tăng công nợ) tùy theo lý do thu phí.
        /// - Trả về thông tin phiếu xuất kho sau khi cập nhật.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DeliveryNoteDto> SaveWithTransactionsAsync(CreateUpdateDeliveryNoteDto input)
        {
            try
            {
                Logger.Info($"SaveWithTransactionsAsync - input: {JsonConvert.SerializeObject(input)} ");

                FundAccountDto currentFundAccount = null;
                if (input.DeliveryFeeReason.HasValue && input.DeliveryFeeReason > 0 && input.DeliveryFee > 0)
                {
                    // Lấy quỹ của kho
                    currentFundAccount =
                       (await _fundAccountAppService.GetFundAccountsByCurrentUserAsync()).FirstOrDefault();

                    if (currentFundAccount == null)
                    {
                        throw new UserFriendlyException("Tài khoản đăng nhập chưa được phân cho quỹ.");
                    }
                }

                var deliveryNote = await Repository.GetAsync(input.Id);
                if (deliveryNote == null)
                {
                    Logger.Error($"Delivery note with ID {input.Id} not found.");
                    throw new EntityNotFoundException(typeof(DeliveryNote), input.Id);
                }
                if (deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                {
                    Logger.Error($"Delivery note with ID {input.Id} has already been delivered.");
                    throw new UserFriendlyException("Phiếu xuất kho đã được xuất kho. Không thể cập nhật.");
                }

                var customer = await _customerRepository.FirstOrDefaultAsync(c => c.Id == input.CustomerId);

                if (customer == null)
                {
                    Logger.Error($"Customer with ID {input.CustomerId} not found.");
                    throw new UserFriendlyException("Không tìm thấy thông tin khách hàng.");
                }
                var canCustomerPayFee = CanCustomerPayFee(customer, input.DeliveryFee, input.ShippingFee, input.DeliveryFeeReason.Value);
                if (!canCustomerPayFee.canPay)
                {
                    // Bổ sung logic: Nếu khách hàng không đủ khả năng thanh toán, trả về lỗi và không thực hiện xuất kho
                    Logger.Error($"Khách hàng với ID {input.CustomerId} vượt quá hạn mức công nợ khi thanh toán phí giao hàng.");
                    throw new UserFriendlyException("Khách hàng vượt quá hạn mức công nợ hoặc số dư không đủ để thanh toán phí giao hàng. Vui lòng kiểm tra lại.");
                }
                deliveryNote.ShippingPartnerId = input.ShippingPartnerId <= 0 ? null : input.ShippingPartnerId.Value;
                deliveryNote.ExportTime = DateTime.Now;
                deliveryNote.Status = (int)DeliveryNoteStatus.Delivered;
                deliveryNote.DeliveryFee = input.DeliveryFee; // Phí vận chuyển nội địa việt nam
                deliveryNote.ShippingFee = input.ShippingFee; // phí vận chuyển quốc tế
                deliveryNote.DeliveryFeeReason = input.DeliveryFeeReason;
                deliveryNote.Receiver = input.Receiver;
                deliveryNote.RecipientPhoneNumber = input.RecipientPhoneNumber;
                deliveryNote.RecipientAddress = input.RecipientAddress;
                deliveryNote.Note = input.Note;
                deliveryNote.ExporterId = _pbtAppSession.UserId;
                deliveryNote.ExportWarehouse = _pbtAppSession.WarehouseId;
                deliveryNote.TotalFee = canCustomerPayFee.totalFee;
                deliveryNote.BalanceBefore = customer.CurrentAmount;
                deliveryNote.BalanceAfter = customer.CurrentAmount - canCustomerPayFee.totalFee;
                deliveryNote.FinancialNegativePart = customer.CurrentAmount - canCustomerPayFee.totalFee;

                //  await Repository.UpdateAsync(deliveryNote);

                try
                {
                    Logger.Info($"Xuất kho. DeliveryNoteId: {deliveryNote.Id} ");

                    var sqlParameterStatusCode = new SqlParameter
                    {
                        ParameterName = "@StatusCode",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    var sqlParameterMessage = new SqlParameter
                    {
                        ParameterName = "@Message",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 500,
                        Direction = ParameterDirection.Output
                    };

                    var excuteResult = await ConnectDb.ExecuteNonQueryAsync("SP_DeliveryNotes_ConfirmExport", CommandType.StoredProcedure,
                        new[] {
                            new SqlParameter("@DeliveryNoteId", deliveryNote.Id),
                            new SqlParameter("@DeliveryFee", deliveryNote.DeliveryFee ?? 0),
                            new SqlParameter("@ShippingFee", deliveryNote.ShippingFee ?? 0),
                            new SqlParameter("@DeliveryFeeReason", deliveryNote.DeliveryFeeReason ?? (object)DBNull.Value),
                            new SqlParameter("@ShippingPartnerId", deliveryNote.ShippingPartnerId ?? (object)DBNull.Value),
                            new SqlParameter("@Receiver", deliveryNote.Receiver ?? (object)DBNull.Value),
                            new SqlParameter("@RecipientPhoneNumber", deliveryNote.RecipientPhoneNumber ?? (object)DBNull.Value),
                            new SqlParameter("@RecipientAddress", deliveryNote.RecipientAddress ?? (object)DBNull.Value),
                            new SqlParameter("@Note", deliveryNote.Note ?? (object)DBNull.Value),
                            new SqlParameter("@ExporterId", deliveryNote.ExporterId ?? (object)DBNull.Value),
                            new SqlParameter("@ExportWarehouse", deliveryNote.ExportWarehouse ?? (object)DBNull.Value),
                            new SqlParameter("@TotalFee", deliveryNote.TotalFee ?? 0),
                            new SqlParameter("@BalanceBefore", deliveryNote.BalanceBefore ?? 0),
                            new SqlParameter("@BalanceAfter", deliveryNote.BalanceAfter ?? 0),
                            new SqlParameter("@FinancialNegativePart", deliveryNote.FinancialNegativePart ?? 0),
                            new SqlParameter("@DeliveryStatus", DeliveryNoteStatus.Delivered),
                            new SqlParameter("@BagStatus", (int)BagShippingStatus.Delivery),
                            new SqlParameter("@PackageStatus", (int)PackageDeliveryStatusEnum.DeliveryInProgress),
                            new SqlParameter("@OrderStatus", (int)OrderStatus.OutForDelivery),
                            new SqlParameter("@WarehouseStatus", (int)WarehouseStatus.OutOfStock),
                            sqlParameterStatusCode,
                            sqlParameterMessage
                        });

                    if (Convert.ToInt32(sqlParameterStatusCode.Value) > 0)
                    {
                        _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                              null
                              , deliveryNote
                              , "created"
                              , $"Phiếu xuất kho đã được xuất: #{deliveryNote.DeliveryNoteCode} "
                              , true
                          );

                        var itemBags = await _bagRepository.GetAllListAsync(x => x.DeliveryNoteId == input.Id);
                        foreach (var item in itemBags)
                        {
                            _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                                   null
                                   , item
                                   , "updated"
                                   , $"Bao hàng đã được tạo phiếu xuất kho: <a target='_blank' href='/DeliveryNote/detail/{deliveryNote.Id}'>#{deliveryNote.DeliveryNoteCode}</a>  "
                                   , true
                               );
                        }

                        var itemPackages = await _packageRepository.GetAllListAsync(x => x.DeliveryNoteId == input.Id);
                        foreach (var package in itemPackages)
                        {
                            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                              null
                              , package
                              , "updated"
                              , $"Kiện hàng đã được tạo phiếu xuất kho: #{deliveryNote.DeliveryNoteCode} "
                              , true
                          );
                        }
                    }

                    else
                    {
                        throw new UserFriendlyException("Không thể cập nhật trạng thái xuất kho của bao, kiện, đơn. Vui lòng thử lại sau.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error updating delivery status: {ex.Message}", ex);
                    throw new UserFriendlyException($"Error updating delivery status: {ex.Message}");
                }


                Logger.Info($"DeliveryNoteId: {deliveryNote.Id} - DeliveryFee: {input.DeliveryFee.Value} ");

                if (currentFundAccount != null)
                {
                    // update TotalWeight
                    HandleFundPayment(
                         customer,
                         deliveryNote,
                        input.DeliveryFeeReason.Value,
                        currentFundAccount
                     );
                }
                HandleCustomerPayment(
                    customer,
                    deliveryNote,
                   input.DeliveryFeeReason.Value
                   
                );


                return MapToEntityDto(deliveryNote);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in SaveWithTransactionsAsync: {ex.Message}", ex);
                throw;
            }
        }



        public async Task<DeliveryNoteDto> CreateQuickDeliveryNote(CreateQuickDeliveryNoteDto input)
        {
            try
            {
                if (input.Items == null || !input.Items.Any())
                {
                    throw new UserFriendlyException("Vui lòng thêm bao hoặc kiện vào phiếu xuất.");
                }

                Logger.Info($"CreateQuickDeliveryNote - input: {JsonConvert.SerializeObject(input)}");

                // Lấy thông tin khách hàng
                var customer = await _customerRepository.FirstOrDefaultAsync(c => c.Id == input.CustomerId);
                if (customer == null)
                {
                    throw new UserFriendlyException("Không tìm thấy thông tin khách hàng.");
                }
                var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(PrefixConst.DeliveryNote);
                // Tạo phiếu xuất kho mới
                var deliveryNote = new DeliveryNote
                {
                    DeliveryNoteCode = identityCode.Code,
                    CustomerId = input.CustomerId,
                    Receiver = input.Receiver,
                    RecipientPhoneNumber = input.RecipientPhoneNumber,
                    RecipientAddress = input.RecipientAddress,
                    Note = input.Note,
                    TotalWeight = input.TotalWeight,
                    DeliveryFee = input.DeliveryFee,
                    DeliveryFeeReason = input.DeliveryFeeReason,
                    ShippingPartnerId = input.ShippingPartnerId,
                    Status = (int)DeliveryNoteStatus.Delivered,
                    ExportTime = DateTime.Now,
                    ExporterId = _pbtAppSession.UserId,
                    ExportWarehouse = _pbtAppSession.WarehouseId
                };

                // Lưu phiếu xuất kho
                deliveryNote.Id = await Repository.InsertAndGetIdAsync(deliveryNote);

                // Xử lý các bao và kiện
                foreach (var item in input.Items)
                {
                    if (item.Type == 1) // Bao
                    {
                        var bag = await _bagRepository.GetAllIncluding(x => x.Packages).FirstOrDefaultAsync(x => x.Id == item.Id);
                        if (bag != null)
                        {
                            bag.DeliveryNoteId = deliveryNote.Id;
                            bag.ShippingStatus = (int)BagShippingStatus.Delivery;
                            await _bagRepository.UpdateAsync(bag);

                            // update order status
                            if (bag.Packages != null && bag.Packages.Any())
                            {
                                foreach (var package in bag.Packages)
                                {
                                    var order = await _orderRepository.FirstOrDefaultAsync(x =>
                                        x.Id == package.OrderId);
                                    if (order != null)
                                    {
                                        order.ShippingStatus = (int)OrderStatus.OutForDelivery;
                                        order.OrderStatus = (int)OrderStatus.OutForDelivery;
                                        await _orderRepository.UpdateAsync(order);
                                    }
                                }
                            }
                        }
                    }
                    else if (item.Type == 2) // Kiện
                    {
                        var package = await _packageRepository.FirstOrDefaultAsync(x => x.Id == item.Id);
                        if (package != null)
                        {
                            package.DeliveryNoteId = deliveryNote.Id;
                            package.ShippingStatus = (int)PackageDeliveryStatusEnum.DeliveryInProgress;
                            await _packageRepository.UpdateAsync(package);

                            // update order status
                            var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == package.OrderId);
                            if (order != null)
                            {
                                order.ShippingStatus = (int)OrderStatus.OutForDelivery;
                                order.OrderStatus = (int)OrderStatus.OutForDelivery;
                                await _orderRepository.UpdateAsync(order);
                            }
                        }
                    }
                }

                Logger.Info($"CreateQuickDeliveryNote - DeliveryNoteId: {deliveryNote.Id}");

                return ObjectMapper.Map<DeliveryNoteDto>(deliveryNote);
            }
            catch (Exception ex)
            {
                Logger.Error($"CreateQuickDeliveryNote - Error: {ex.Message}", ex);
                throw new UserFriendlyException("Có lỗi xảy ra khi tạo phiếu xuất nhanh.");
            }
        }

        [Authorize]
        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<DeliveryNoteDto> UpdateStatusDeliveryNoteAsync(UpdateDeliveryNoteStatusDto input)
        {
            var deliveryNote = await Repository.GetAsync(input.Id);
            if (deliveryNote == null) return null;
            deliveryNote.Status = input.Status;
            deliveryNote.ExportTime = DateTime.Now;
            await Repository.UpdateAsync(deliveryNote);

            // add log
            _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                null
                , deliveryNote
                , "updated"
                , $"Xuất kho"
                , true
            );

            var bags = (await _bagRepository.GetAllAsync()).Include(x => x.Packages)
                .Where(x => x.DeliveryNoteId == deliveryNote.Id);
            var packages = (await _packageRepository.GetAllAsync()).Include(x => x.Bag).Where(
                x => x.DeliveryNoteId == deliveryNote.Id
                     && x.Bag.BagType == (int)BagTypeEnum.InclusiveBag
            );
            if (bags != null && bags.Count() > 0)
            {
                foreach (var bag in bags)
                {
                    bag.WarehouseStatus = (int)WarehouseStatus.OutOfStock;
                    await _bagRepository.UpdateAsync(bag);
                    // add log bag
                    _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                        null
                        , bag
                        , "updated"
                        , $"Xuất kho bao theo phiếu xuất kho {deliveryNote.DeliveryNoteCode}"
                        , true
                    );

                    if (bag.Packages != null && bag.Packages.Any())
                    {
                        foreach (var package in bag.Packages)
                        {
                            package.WarehouseStatus = (int)WarehouseStatus.OutOfStock;
                            package.ShippingStatus = (int)PackageDeliveryStatusEnum.DeliveryInProgress;
                            package.DeliveryTime = DateTime.Now;
                            await _packageRepository.UpdateAsync(package);
                            // add log package
                            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                                null
                                , package
                                , "updated"
                                , $"Xuất kho kiện theo phiếu xuất kho {deliveryNote.DeliveryNoteCode}"
                                , true
                            );
                        }
                    }
                }
            }

            if (packages != null && packages.Any())
            {
                foreach (var package in packages)
                {
                    package.WarehouseStatus = (int)WarehouseStatus.OutOfStock;
                    package.ShippingStatus = (int)PackageDeliveryStatusEnum.DeliveryInProgress;
                    package.DeliveryTime = DateTime.Now;
                    await _packageRepository.UpdateAsync(package);

                    // add log package
                    _entityChangeLoggerAppService.LogChangeAsync<Package>(
                        null
                        , package
                        , "updated"
                        , $"Xuất kho kiện theo phiếu xuất kho {deliveryNote.DeliveryNoteCode}"
                        , true
                    );
                }
            }

            return MapToEntityDto(deliveryNote);
        }

        public async Task<DeliveryNoteDetail> getDeliveryNoteByIdAsync(int id)
        {
            var _dto = (await Repository.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            DeliveryNoteDto dto = ObjectMapper.Map<DeliveryNoteDto>(_dto);
            // get user login
            dto.CreatorName = _pbtAppSession.UserName;
            var model = new DeliveryNoteDetail()
            {
                Dto = dto
            };
            return model;
        }

        [Authorize]
        public async Task<DeliveryNoteDto> GetOrCreateByCustomerIdAsync(long customerId)
        {
            try
            {
                var currentWarehouseId = _pbtAppSession.WarehouseId.Value;
                var customer = await GetCustomerByIdAsync(customerId);// await _customerRepository.FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                {
                    throw new UserFriendlyException("Không tìm thấy khách hàng.");
                }
                var deliveryNote = await GetDeliveryNoteByCustomerAndStatus(customerId, currentWarehouseId, (int)DeliveryNoteStatus.New);

                if (deliveryNote == null)
                {
                    // Nếu không có thì tạo mới 
                    var identityDeliveryNoteCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(PrefixConst.DeliveryNote);

                    deliveryNote = new DeliveryNote()
                    {
                        Cod = 0,
                        Receiver = customer.Username,
                        RecipientPhoneNumber = customer.PhoneNumber,
                        RecipientAddress = customer.Address,
                        ShippingFee = 0,
                        DeliveryFee = 0,
                        TotalWeight = 0,
                        Length = 0,
                        Width = 0,
                        Height = 0,
                        DeliveryNoteCode = identityDeliveryNoteCode.Code,
                        CustomerId = customerId,
                        ExporterId = _pbtAppSession.UserId,
                        Status = (int)DeliveryNoteStatus.New,
                        ExportWarehouse = currentWarehouseId,
                        WarehouseCreateId = currentWarehouseId,
                    };
                    // Lưu phiếu xuất kho
                    deliveryNote.Id = await CreateAndGetIdDeliveryNoteAsync(deliveryNote);

                    _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                        null
                        , deliveryNote
                        , "create"
                        , $"Phiếu xuất #{deliveryNote.DeliveryNoteCode} được tạo bởi {_pbtAppSession.UserName}"
                        , true
                     );

                    var deliveryAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = deliveryNote.Id.ToString(),
                        EntityType = nameof(DeliveryNote),
                        MethodName = EntityAuditLogMethodName.Create.ToString(),
                        Title = $"Phiếu xuất kho #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} được tạo",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(deliveryNote, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };

                    _entityAuditLogApiClient.SendAsync(deliveryAuditLog);
                }

                var dto = ObjectMapper.Map<DeliveryNoteDto>(deliveryNote);
                dto.Receiver = dto.Receiver ?? customer.Username;
                dto.RecipientPhoneNumber = dto.RecipientPhoneNumber ?? customer.PhoneNumber;
                dto.RecipientAddress = dto.RecipientAddress ?? customer.Address;
                return dto;
            }
            catch (Exception exx)
            {
                Logger.Error($"Error in GetOrCreateByCustomerIdAsync. customerId: {customerId}, {exx.Message}", exx);
                throw;
            }
        }


        [Authorize]
        public async Task<JsonResult> RemoveItem(RemoveItemFromDeliveryNoteDto input)
        {

            if (input == null || input.ItemId <= 0)
            {
                return new JsonResult(new
                {
                    Status = 400,
                    Message = "Mã kiện không hợp lệ."
                });
            }

            Logger.Info($"RemoveItem - input: {JsonConvert.SerializeObject(input)} ");
            // Xóa bao hoặc kiện khỏi phiếu xuất kho
            if (input.ItemType == DeliveryNoteRemoveItemType.Bag)
            {
                var bag = await _bagRepository.FirstOrDefaultAsync(x => x.Id == input.ItemId);
                if (bag != null && bag.ShippingStatus == (int)BagShippingStatus.WaitingForDelivery && bag.DeliveryNoteId.HasValue)
                {
                    var deliveryNoteId = bag.DeliveryNoteId;

                    //                 CREATE OR ALTER PROC[dbo].[SP_Bags_RemoveFromDelivery]
                    //                 (
                    //@BagId BIGINT,
                    //@DeliveryNoteId BIGINT,
                    //@BagStatus INT,
                    //@PackageStatus INT,

                    //@StatusCode INT OUTPUT,
                    //@Message NVARCHAR(MAX) OUTPUT

                    var statusCodeParam = new SqlParameter
                    {
                        ParameterName = "@StatusCode",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    var messageParam = new SqlParameter
                    {
                        ParameterName = "@Message",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 500,
                        Direction = ParameterDirection.Output
                    };

                    var execResult = await ConnectDb.ExecuteNonQueryAsync("SP_Bags_RemoveFromDelivery",
                         CommandType.StoredProcedure,
                         new[]
                         {
                            new SqlParameter("@BagId", bag.Id),
                            new SqlParameter("@DeliveryNoteId", deliveryNoteId),
                            new SqlParameter("@BagStatus", (int)BagShippingStatus.GoToWarehouse),
                            new SqlParameter("@PackageStatus", (int)PackageDeliveryStatusEnum.InWarehouseVN),
                            statusCodeParam,
                            messageParam
                         });

                    int statusCode = Convert.ToInt32(statusCodeParam.Value);
                    if (statusCode <= 0)
                    {
                        string errorMessage = messageParam.Value.ToString();
                        return new JsonResult(new
                        {
                            Status = 400,
                            Message = errorMessage
                        });
                    }

                    var deliveryNote = await Repository.GetAsync(deliveryNoteId.Value);
                    _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                        null
                        , bag
                        , "updated"
                        , $"Xóa khỏi phiếu xuất #{deliveryNote.DeliveryNoteCode}"
                        , true
                     );


                    _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                    null
                    , deliveryNote
                    , "updated"
                    , $"Xóa bao #{bag.BagCode} xóa khỏi phiếu xuất"
                    , true
                 );

                    var packageInBag = await GetPackagesByBagIdAsync(bag.Id);
                    foreach (var package in packageInBag)
                    {
                        _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                           null
                           , package
                           , "updated"
                           , $"Xóa khỏi phiếu xuất #{deliveryNote.DeliveryNoteCode} theo bao ${bag.BagCode}"
                           , true
                        );

                        _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                         null
                         , deliveryNote
                         , "updated"
                         , $"Xóa kiện #{package.PackageNumber} theo bao ${bag.BagCode}"
                         , true
                      );

                    }

                    return new JsonResult(new
                    {
                        Status = 200,
                        Message = "Xóa thành công.",
                        DeliveryNote = deliveryNote
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Bao không hợp lệ hoặc không thuộc phiếu xuất kho."
                    });
                }
            }
            else if (input.ItemType == DeliveryNoteRemoveItemType.Package)
            {
                var package = await _packageRepository.FirstOrDefaultAsync(x => x.Id == input.ItemId);
                if (package != null && package.ShippingStatus == (int)PackageDeliveryStatusEnum.WaitingForDelivery)
                {
                    var deliveryNoteId = package.DeliveryNoteId;

                    var statusCodeParam = new SqlParameter
                    {
                        ParameterName = "@StatusCode",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    var messageParam = new SqlParameter
                    {
                        ParameterName = "@Message",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 500,
                        Direction = ParameterDirection.Output
                    };
                    var execResult = await ConnectDb.ExecuteNonQueryAsync("SP_Packages_RemoveFromDelivery",
                         CommandType.StoredProcedure,
                         new[]
                         {
                            new SqlParameter("@PackageId", package.Id),
                            new SqlParameter("@DeliveryNoteId", deliveryNoteId),
                            new SqlParameter("@PackageStatus", (int)PackageDeliveryStatusEnum.InWarehouseVN),
                            statusCodeParam,
                            messageParam
                         });
                    int statusCode = Convert.ToInt32(statusCodeParam.Value);
                    if (statusCode <= 0)
                    {
                        string errorMessage = messageParam.Value.ToString();
                        return new JsonResult(new
                        {
                            Status = 400,
                            Message = errorMessage
                        });
                    }

                    var deliveryNote = await Repository.GetAsync(deliveryNoteId.Value);

                    _entityChangeLoggerAppService.LogChangeAsync<Package>(
                       null
                       , package
                       , "updated"
                       , $"Xóa khỏi phiếu xuất #{deliveryNote.DeliveryNoteCode}"
                       , true
                    );


                    _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                    null
                    , deliveryNote
                    , "updated"
                    , $"Xóa kiện #{package.PackageNumber} xóa khỏi phiếu xuất"
                    , true
                 );

                    return new JsonResult(new
                    {
                        Status = 200,
                        Message = "Xóa thành công.",
                        DeliveryNote = deliveryNote
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Kiện không hợp lệ hoặc không thuộc phiếu xuất kho."
                    });
                }
            }
            else
            {
                return new JsonResult(new
                {
                    Status = 400,
                    Message = "Loại mục không hợp lệ."
                });
            }
        }

        [Authorize]
        public async Task<JsonResult> ScanCodeAsync(ScanCodeDto data)
        {
            if (data == null || string.IsNullOrEmpty(data.Code))
            {
                return new JsonResult(new
                {
                    Status = 400,
                    Message = "Mã quét không hợp lệ."
                });
            }

            var result = new JsonResult(new { Status = 200, Message = "Success" });
            var packagePrefixList = await _configurationSettingAppService.GetValueAsync("PackagePrefixCode");
            if (string.IsNullOrEmpty(packagePrefixList))
            {
                Logger.Error("Configuration 'PackageCode' is not set. Using default prefixes.");
                return new JsonResult(new
                {
                    Status = 400,
                    Message = "Mã kiện chưa được khai báo trong thiết lập."
                });
            }

            string[] packagePrefix = packagePrefixList.Split(","); // new[] { "BT", "PBT", "KH", "MP", "PB", "SH", "CT" };

            Logger.Info($"[DeliveryNote] ScanCodeAsync - data: {JsonConvert.SerializeObject(data)} ");

            var currentWarehouseId = _pbtAppSession.WarehouseId.Value;

            if (packagePrefix.Any(prefix => data.Code.StartsWith(prefix)))
            {
                var package = await GetPackageByCodeAsync(data.Code);
                if (package == null)
                {
                    return new JsonResult(new
                    {
                        Status = 404,
                        Message = "Không tìm thấy kiện với mã này."
                    });
                }
                // Nếu trạng thái không phải đang ở Việt Nam
                if ((package.ShippingStatus != (int)PackageDeliveryStatusEnum.InWarehouseVN && package.ShippingStatus != (int)PackageDeliveryStatusEnum.DeliveryRequest)
                    || package.WarehouseStatus != (int)WarehouseStatus.InStock)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Kiện đã được xuất hoặc không ở kho, không thể quét mã."
                    });
                }

                // Nếu kiện đã có trong phiếu xuất kho
                if (package.DeliveryNoteId.HasValue || package.DeliveryNoteId > 0)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Kiện đã được tạo phiếu xuất kho, không thể quét mã."
                    });
                }

                if (!package.CustomerId.HasValue || package.CustomerId <= 0)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Kiện chưa có thông tin khách hàng, không thể quét mã."
                    });

                }
                // Lấy thông tin phiếu xuất theo trạng thái là mới và thông tin khách hàng
                var customerId = package.CustomerId.Value;

                // Nếu như data.LockCustomer == true 
                // Kiểm tra dữ liệu đầu vào. Nếu CustomerId khác null, lớn hơn 0, và khách với package.CustomerId thì return lỗi. 
                if (data.LockCustomer && data.CustomerId.HasValue && data.CustomerId > 0 && data.CustomerId != package.CustomerId)
                {
                    return new JsonResult(new
                    {
                        Status = 401,
                        Message = "Mã kiện này không thuộc về khách hàng đã chọn."
                    });
                }

                EntityAuditLogDto deliveryAuditLog;
                var deliveryNote = await GetDeliveryNoteByCustomerAndStatus(customerId, currentWarehouseId, (int)DeliveryNoteStatus.New); //GetNewDeliveryNoteByCustomerId(customerId);

                // Nếu như chưa có phiếu xuất kho thì tạo mới phiếu xuất kho
                if (deliveryNote == null)
                {
                    var identityDeliveryNoteCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(PrefixConst.DeliveryNote);
                    deliveryNote = new DeliveryNote()
                    {
                        DeliveryNoteCode = identityDeliveryNoteCode.Code,
                        CustomerId = customerId,
                        ExporterId = _pbtAppSession.UserId,
                        ExportTime = DateTime.Now,
                        Status = (int)DeliveryNoteStatus.New,
                        ExportWarehouse = _pbtAppSession.WarehouseId,
                        TotalWeight = package.Weight,
                        ShippingFee = package.TotalPrice
                    };
                    //// Lưu phiếu xuất kho
                    deliveryNote.Id = await CreateAndGetIdDeliveryNoteAsync(deliveryNote);

                    _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                    null
                    , deliveryNote
                    , "create"
                    , $"Phiếu xuất #{deliveryNote.DeliveryNoteCode} được tạo bởi {_pbtAppSession.UserName}"
                    , true
                 );

                    deliveryAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = deliveryNote.Id.ToString(),
                        EntityType = nameof(DeliveryNote),
                        MethodName = EntityAuditLogMethodName.Create.ToString(),
                        Title = $"Phiếu xuất kho #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} được tạo",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(deliveryNote, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };

                    _entityAuditLogApiClient.SendAsync(deliveryAuditLog);
                }

                var sqlParameterStatusCode = new SqlParameter
                {
                    ParameterName = "@StatusCode",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                var sqlParameterMessage = new SqlParameter
                {
                    ParameterName = "@Message",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 500,
                    Direction = ParameterDirection.Output
                };

                await ConnectDb.ExecuteNonQueryAsync("SP_Packages_AddToDelivery", CommandType.StoredProcedure,
                      new[]
                      {
                        new SqlParameter("@DeliveryNoteId", deliveryNote.Id),
                        new SqlParameter("@PackageId", package.Id),
                        new SqlParameter("@PackageStatus", (int)PackageDeliveryStatusEnum.WaitingForDelivery),
                        sqlParameterStatusCode,
                        sqlParameterMessage
                      });

                if (Convert.ToInt32(sqlParameterStatusCode.Value) <= 0)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = sqlParameterMessage.Value.ToString()
                    });
                }

                var packageUpdated = await GetPackageByIdAsync(package.Id);
                // var deliveryNoteUpdated = await (await Repository.GetAllAsync()).Where(x => x.Id == deliveryNote.Id).FirstOrDefaultAsync();
                _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                   null
                   , package
                   , "updated"
                   , $"Kiện #{packageUpdated.PackageNumber} đưa vào phiếu xuất #{packageUpdated.DeliveryNoteCode}"
                   , true
               );


                _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                null
                , deliveryNote
                , "updated"
                , $"Phiếu xuất kho #{deliveryNote.DeliveryNoteCode} thêm kiện #{packageUpdated.PackageNumber} "
                , true
             );

                var packageAuditLog = new EntityAuditLogDto()
                {
                    EntityId = packageUpdated.Id.ToString(),
                    EntityType = nameof(Package),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Kiện hàng #{packageUpdated.Id} - {packageUpdated.PackageNumber} thêm vào phiếu xuất #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode}",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(packageUpdated, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                };

                _entityAuditLogApiClient.SendAsync(packageAuditLog);

                var deliveryNoteUpdated = await GetDeliveryNoteByIdAsync(deliveryNote.Id);

                deliveryAuditLog = new EntityAuditLogDto()
                {
                    EntityId = deliveryNote.Id.ToString(),
                    EntityType = nameof(DeliveryNote),
                    MethodName = EntityAuditLogMethodName.Update.ToString(),
                    Title = $"Phiếu xuất kho #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} thêm kiện #{packageUpdated.Id} - {packageUpdated.PackageNumber} ",
                    UserId = _pbtAppSession.UserId,
                    UserName = _pbtAppSession.UserName,
                    Data = JsonConvert.SerializeObject(deliveryNoteUpdated, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                };
                _entityAuditLogApiClient.SendAsync(deliveryAuditLog);

                return new JsonResult(new
                {
                    Status = 200,
                    Message = "Quét mã kiện thành công.",
                    Data = new
                    {
                        DeliveryNoteId = deliveryNote.Id,
                        DeliveryNoteCode = deliveryNote.DeliveryNoteCode,
                        CustomerId = deliveryNote.CustomerId,
                        //  DeliveryNote = deliveryNoteUpdated
                    }
                });
            }
            // Phân tích mã code là mã bao hay mã kiện bằng các ký tự đầu tiên 
            else if (data.Code.StartsWith(PrefixConst.BagCode))
            {
                var bag = await GetBagByCodeAsync(data.Code);
                // Nếu trạng thái không phải đang ở Việt Nam
                if (bag == null)
                {
                    return new JsonResult(new
                    {
                        Status = 404,
                        Message = "Không tìm thấy bao với mã này."
                    });
                }
                // Nếu bao đã được xuất kho
                if (bag.WarehouseStatus != (int)WarehouseStatus.InStock ||
                    (bag.ShippingStatus != (int)BagShippingStatus.GoToWarehouse)
                    && bag.ShippingStatus != (int)BagShippingStatus.DeliveryRequest)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Bao đã được xuất hoặc không ở kho, không thể quét mã."
                    });
                }

                if (bag.BagType == (int)BagTypeEnum.InclusiveBag)
                {
                    // Nếu bao là bao ghép thì không thể quét mã
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Không thể tạo yêu cầu cho bao ghép"
                    });
                }
                if (bag.DeliveryNoteId.HasValue || bag.DeliveryNoteId > 0)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Bao đã được tạo phiếu xuất kho, không thể quét mã."
                    });
                }


                if (!bag.CustomerId.HasValue || bag.CustomerId <= 0)
                {
                    return new JsonResult(new
                    {
                        Status = 400,
                        Message = "Bao chưa có thông tin khách hàng, không thể quét mã."
                    });
                }

                var customerId = bag.CustomerId.Value;

                // Nếu như data.LockCustomer == true 
                // Kiểm tra dữ liệu đầu vào. Nếu CustomerId khác null, lớn hơn 0, và khách với package.CustomerId thì return lỗi. 
                if (data.LockCustomer && data.CustomerId.HasValue && data.CustomerId > 0 && data.CustomerId != bag.CustomerId)
                {
                    return new JsonResult(new
                    {
                        Status = 401,
                        Message = "Mã bao này không thuộc về khách hàng đã chọn."
                    });
                }

                try
                {
                    var deliveryNote = await GetDeliveryNoteByCustomerAndStatus(customerId, currentWarehouseId, (int)DeliveryNoteStatus.New); // GetNewDeliveryNoteByCustomerId(customerId);
                    // Nếu như chưa có phiếu xuất kho thì tạo mới phiếu xuất kho
                    if (deliveryNote == null)
                    {
                        var identityDeliveryNoteCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(PrefixConst.DeliveryNote);
                        deliveryNote = new DeliveryNote()
                        {
                            DeliveryNoteCode = identityDeliveryNoteCode.Code,
                            CustomerId = customerId,
                            ExporterId = _pbtAppSession.UserId,
                            ExportTime = DateTime.Now,
                            Status = (int)DeliveryNoteStatus.New,
                            ExportWarehouse = _pbtAppSession.WarehouseId,
                        };

                        // Lưu phiếu xuất kho
                        deliveryNote.Id = await CreateAndGetIdDeliveryNoteAsync(deliveryNote);

                        var deliveryAuditLog = new EntityAuditLogDto()
                        {
                            EntityId = deliveryNote.Id.ToString(),
                            EntityType = nameof(DeliveryNote),
                            MethodName = EntityAuditLogMethodName.Create.ToString(),
                            Title = $"Phiếu xuất kho #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} được tạo",
                            UserId = _pbtAppSession.UserId,
                            UserName = _pbtAppSession.UserName,
                            Data = JsonConvert.SerializeObject(deliveryNote, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                        };
                        await _entityAuditLogApiClient.SendAsync(deliveryAuditLog);
                    }

                    var sqlParameterStatusCode = new SqlParameter
                    {
                        ParameterName = "@StatusCode",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };
                    var sqlParameterMessage = new SqlParameter
                    {
                        ParameterName = "@Message",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 500,
                        Direction = ParameterDirection.Output
                    };
                    await ConnectDb.ExecuteNonQueryAsync("SP_Bags_AddToDelivery", CommandType.StoredProcedure,
                          new[]
                          {
                            new SqlParameter("@DeliveryNoteId", deliveryNote.Id),
                            new SqlParameter("@BagId", bag.Id),
                            new SqlParameter("@BagStatus", (int)BagShippingStatus.WaitingForDelivery),
                            new SqlParameter("@PackageStatus", (int)PackageDeliveryStatusEnum.WaitingForDelivery),
                            sqlParameterStatusCode,
                            sqlParameterMessage
                          });
                    var statusCode = Convert.ToInt32(sqlParameterStatusCode.Value);
                    if (statusCode <= 0)
                    {
                        return new JsonResult(new
                        {
                            Status = 400,
                            Message = sqlParameterMessage.Value.ToString()
                        });
                    }


                    _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                       null
                       , deliveryNote
                       , "updated"
                       , $"Phiếu xuất kho #{deliveryNote.DeliveryNoteCode} thêm bao #{bag.BagCode} "
                       , true
                    );

                    _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                       null
                       , bag
                       , "updated"
                       , $"Bao #{bag.BagCode} đưa vào phiếu xuất #{deliveryNote.DeliveryNoteCode}"
                       , true
                    );

                    // Ghi log bao

                    var bagUpdated = await GetBagByCodeAsync(bag.BagCode);
                    var bagAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = bag.Id.ToString(),
                        EntityType = nameof(Bag),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                        Title = $"Bao hàng #{bag.Id} - {bag.BagCode} thêm vào phiếu xuất #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode}",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(bagUpdated, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    };

                    _entityAuditLogApiClient.SendAsync(bagAuditLog);

                    var deliveryNoteUpdated = await GetDeliveryNoteByIdAsync(deliveryNote.Id);
                    var deliveryNoteAuditLog = new EntityAuditLogDto()
                    {
                        EntityId = deliveryNote.Id.ToString(),
                        EntityType = nameof(DeliveryNote),
                        MethodName = EntityAuditLogMethodName.Update.ToString(),
                        Title = $"Phiếu xuất kho #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} thêm bao #{bag.Id} - {bag.BagCode} ",
                        UserId = _pbtAppSession.UserId,
                        UserName = _pbtAppSession.UserName,
                        Data = JsonConvert.SerializeObject(deliveryNoteUpdated, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    };
                    await _entityAuditLogApiClient.SendAsync(deliveryNoteAuditLog);

                    var packagesInBag = await GetPackagesByBagIdAsync(bag.Id);

                    foreach (var package in packagesInBag)
                    {

                        _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                            null
                            , package
                            , "updated"
                            , $"Thêm vào phiếu xuất #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} qua bao #{bag.Id} - {bag.BagCode}"
                            , true
                         );

                        var packageAuditLog = new EntityAuditLogDto()
                        {
                            EntityId = package.Id.ToString(),
                            EntityType = nameof(Package),
                            MethodName = EntityAuditLogMethodName.Update.ToString(),
                            Title = $"Kiện hàng #{package.Id} - {package.PackageNumber} thêm vào phiếu xuất #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} qua bao #{bag.Id} - {bag.BagCode}",
                            UserId = _pbtAppSession.UserId,
                            UserName = _pbtAppSession.UserName,
                            Data = JsonConvert.SerializeObject(package, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                        };

                        _entityAuditLogApiClient.SendAsync(packageAuditLog);

                        deliveryNoteAuditLog = new EntityAuditLogDto()
                        {
                            EntityId = deliveryNote.Id.ToString(),
                            EntityType = nameof(DeliveryNote),
                            MethodName = EntityAuditLogMethodName.Update.ToString(),
                            Title = $"Phiếu xuất kho #{deliveryNote.Id} - {deliveryNote.DeliveryNoteCode} thêm kiện #{package.Id} - {package.PackageNumber} qua bao #{bag.Id} - {bag.BagCode} ",
                            UserId = _pbtAppSession.UserId,
                            UserName = _pbtAppSession.UserName,
                            Data = JsonConvert.SerializeObject(deliveryNoteUpdated, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                        };

                        _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                           null
                           , deliveryNote
                           , "updated"
                          , $"Thêm kiện #{package.Id} - {package.PackageNumber} qua bao #{bag.Id} - {bag.BagCode} "
                           , true
                        );

                    }

                    return new JsonResult(new
                    {
                        Status = 200,
                        Message = "Quét mã kiện thành công.",
                        Data = new
                        {
                            DeliveryNoteId = deliveryNote.Id,
                            DeliveryNoteCode = deliveryNote.DeliveryNoteCode,
                            CustomerId = deliveryNote.CustomerId,
                            //  DeliveryNote = deliveryNoteUpdated
                        }
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error in ScanCodeAsync - BagId: {bag.Id}, msg: {ex.Message}", ex);
                    return new JsonResult(new
                    {
                        Status = 500,
                        Message = "Lỗi hệ thống khi quét mã bao. Vui lòng thử lại hoặc liên hệ quản trị viên."
                    });
                }
            }

            else
            {
                return new JsonResult(new
                {
                    Status = 400,
                    Message = "Mã không hợp lệ. Vui lòng kiểm tra lại mã bao hoặc mã kiện."
                });
            }

        }

        private async Task<DeliveryNoteDto> GetDeliveryNoteByIdAsync(int id)
        {
            var data = await ConnectDb.GetItemAsync<DeliveryNoteDto>("SP_DeliveryNotes_GetById",
                CommandType.StoredProcedure,
                new[] {
                    new SqlParameter()
                    {
                        ParameterName = "@id",
                        Value = id
                    }
                });

            return data;
        }

        public async Task<PagedResultDto<DeliveryNoteDto>> GetAllPagedAsync(
            PagedDeliveryNoteResultRequestDto input)
        {
            try
            {
                var query = (await Repository.GetAllAsync());
                if (input.CustomerId.HasValue && input.CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerId == input.CustomerId);
                }
                if (input.Status.HasValue && input.Status >= 0)
                {
                    query = query.Where(x => x.Status == input.Status);
                }
                if (input.ShippingPartnerId.HasValue && input.ShippingPartnerId > 0)
                {
                    query = query.Where(x => x.ShippingPartnerId == input.ShippingPartnerId);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    query = query.Where(x => x.DeliveryNoteCode.Contains(input.Keyword)
                                             || x.Receiver.Contains(input.Keyword)
                                             || x.RecipientPhoneNumber.Contains(input.Keyword)
                                            );
                }

                if (input.StartCreateDate.HasValue)
                {
                    query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
                }
                if (input.EndCreateDate.HasValue)
                {
                    query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
                }

                if (input.StartExportDateVN.HasValue)
                {
                    query = query.Where(x => x.ExportTime >= input.StartExportDateVN.Value);
                }
                if (input.EndExportDateVN.HasValue)
                {
                    query = query.Where(x => x.ExportTime <= input.EndExportDateVN.Value);
                }

                query = query.OrderByDescending(x => x.CreationTime);
                var exportDto = ObjectMapper.Map<List<DeliveryNoteDto>>(query);
                return new PagedResultDto<DeliveryNoteDto>()
                {
                    Items = exportDto,
                    TotalCount = DynamicQueryableExtensions.Count(query),
                };
            }
            catch (Exception e)
            {
                Logger.Error($"Error in GetAllPagedAsync: {e.Message}", e);
                throw;
            }
        }


        public async Task<List<DeliveryNoteExportViewDto>> GetDeliveryNotesByExportView(DeliveryNoteExportViewInputDto input)
        {
            var permissionCheckResult = GetPermissionCheckerWithCustomerIds();

            if (permissionCheckResult.permissionCase < 1 || permissionCheckResult.permissionCase > 3)
            {
                return new List<DeliveryNoteExportViewDto>();
            }
            var permissionCase = permissionCheckResult.permissionCase;
            var customerIds = permissionCheckResult.CustomerIds;
            int maxDayRange = 2;
            var query = base.CreateFilteredQuery(input);

            //var query = (await Repository.GetAllAsync());
            // Tạo query để lấy danh sách kiện hàng
            query = query
                 .Where(x =>
                   (permissionCase == 1) || // admin  nhìn thấy tất cả
                   ((permissionCase == 2 || permissionCase == 3) && x.CustomerId.HasValue && customerIds.Contains(x.CustomerId.Value))
                // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                );
            if (string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x => x.DeliveryNoteCode == input.Keyword
                                             || x.Receiver.Contains(input.Keyword)
                                             || x.RecipientPhoneNumber.Contains(input.Keyword)
                                            );
            }

            query = query.Where(x => x.Status == (int)DeliveryNoteStatus.Delivered);
            DateTime? StartExportVNDate = input.StartExportVNDate;
            DateTime? EndExportVNDate = input.EndExportVNDate;
            // --- normalize date range: ensure start/end range <= 7 days ---
            if (input.StartExportVNDate.HasValue && input.EndExportVNDate.HasValue)
            {
                var start = input.StartExportVNDate.Value;
                var end = input.EndExportVNDate.Value;

                // if start > end swap
                if (start > end)
                {
                    var tmp = start;
                    start = end;
                    end = tmp;
                }

                // if range greater than 7 days, set start = end - 7 days
                if ((end.Date - start.Date).TotalDays > maxDayRange)
                {
                    start = end.Date.AddDays(-maxDayRange);
                }

                StartExportVNDate = start;
                EndExportVNDate = end;
            }
            else
            {
                if (input.StartExportVNDate.HasValue && !input.EndExportVNDate.HasValue)
                {
                    StartExportVNDate = input.StartExportVNDate.Value;
                    EndExportVNDate = input.StartExportVNDate.Value.AddDays(maxDayRange);
                }
                else if (!input.StartExportVNDate.HasValue && input.EndExportVNDate.HasValue)
                {
                    EndExportVNDate = input.EndExportVNDate.Value;
                    StartExportVNDate = input.EndExportVNDate.Value.AddDays(-maxDayRange);
                }
                else
                {
                    // both null
                    EndExportVNDate = DateTime.Now;
                    StartExportVNDate = EndExportVNDate.Value.AddDays(-maxDayRange);
                }
            }

            query = query.Where(x => x.ExportTime >= StartExportVNDate.Value && x.ExportTime <= EndExportVNDate.Value);

            if (input.CustomerId.HasValue && input.CustomerId > 0)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId);
            }

            var result = new List<DeliveryNoteExportViewDto>();
            var data = query.OrderByDescending(u => u.Id).ToList();
            var shippingPartnerIds = data.Select(x => x.ShippingPartnerId).Distinct().ToList();
            var shippingPartnerDict = (await _shippingPartnerRepository.GetAllAsync()).Where(sp => shippingPartnerIds.Contains(sp.Id))
                .ToDictionary(sp => sp.Id, sp => sp.Name);

            foreach (var item in data)
            {
                var dnExportViewItem = new DeliveryNoteExportViewDto()
                {
                    Id = item.Id,
                    DeliveryNoteCode = item.DeliveryNoteCode,
                    CustomerId = item.CustomerId.Value,
                    Receiver = item.Receiver,
                    ShippingPartnerId = item.ShippingPartnerId,
                    DeliveryFee = item.DeliveryFeeReason == (int)DeliveryFeeType.WithFee ? (item.DeliveryFee ?? 0) : 0,
                    ExportTime = item.ExportTime,

                };

                dnExportViewItem.ShippingPartnerName = item.ShippingPartnerId.HasValue
                    && shippingPartnerDict != null
                    && shippingPartnerDict.ContainsKey(item.ShippingPartnerId.Value)
                    ? shippingPartnerDict[item.ShippingPartnerId.Value]
                    : string.Empty;

                //dnExportViewItem.Items = new List<DeliveryNoteExportItemViewDto>();
                //var bags = (await _bagRepository.GetAllAsync())
                //    .Where(b => b.DeliveryNoteId == item.Id && b.BagType == (int) BagTypeEnum.SeparateBag) 
                //    .ToList();

                var dnExportViewItems = await ConnectDb.GetListAsync<DeliveryNoteExportItemViewDto>("SP_DeliveryNote_GetExportItems",
                    System.Data.CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter() {
                            ParameterName = "@DeliveryNoteId",
                            Value = item.Id
                        }
                    });

                dnExportViewItem.Items = dnExportViewItems;
                if (dnExportViewItem.Items != null && dnExportViewItem.Items.Count > 0)
                {
                    var totalPackageFee = dnExportViewItem.Items.Sum(i => i.TotalFee);
                    dnExportViewItem.TotalFee = totalPackageFee + dnExportViewItem.DeliveryFee;
                }

                result.Add(dnExportViewItem);

            }

            return result;

        }

        /// <summary>
        /// Kiểm tra khả năng thanh toán của khách hàng trước khi trừ tiền phí giao hàng.
        /// Nếu số dư ví + hạn mức công nợ >= tổng phí cần thanh toán thì trả về true, ngược lại trả về false.
        /// </summary>
        /// <param name="customer">Thông tin khách hàng</param>
        /// <param name="deliveryFee">Phí giao hàng nội địa</param>
        /// <param name="shippingFee">Phí ship quốc tế</param>
        /// <param name="deliveryFeeReason">Lý do thu phí giao hàng</param>
        /// <returns>True nếu khách hàng có khả năng thanh toán, False nếu vượt quá hạn mức công nợ</returns>
        public (bool canPay, decimal totalFee) CanCustomerPayFee(Customer customer, decimal? deliveryFee, decimal? shippingFee, int deliveryFeeReason)
        {
            decimal totalFee = 0;
            if (deliveryFeeReason == (int)DeliveryFeeType.WithoutFee)
            {
                // Chỉ tính phí ship quốc tế
                totalFee = shippingFee ?? 0;
            }
            else
            {
                // Tính cả phí giao hàng nội địa và phí ship quốc tế
                totalFee = (deliveryFee ?? 0) + (shippingFee ?? 0);
            }

            // Số dư sau khi trừ phí
            decimal afterPaymentAmount = customer.CurrentAmount - totalFee;

            // Nếu sau khi trừ phí, số dư < 0 thì kiểm tra hạn mức công nợ
            if (afterPaymentAmount < 0)
            {
                decimal debtAfterPayment = Math.Abs(afterPaymentAmount);
                // Nếu công nợ sau thanh toán vượt quá MaxDebt thì không cho phép thanh toán
                if (debtAfterPayment > customer.MaxDebt)
                {
                    return (false, totalFee);
                }
            }
            // Nếu số dư đủ hoặc công nợ trong hạn mức thì cho phép thanh toán
            return (true, totalFee);
        }

        private Task<CustomerDto> GetCustomerByIdAsync(long customerId)
        {
            return ConnectDb.GetItemAsync<CustomerDto>("sp_Customer_GetById",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@CustomerId",
                    Value = customerId
                } });
        }



        /// <summary>
        /// Xóa phiếu xuất kho
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task DeleteDeliveryNoteAsync(int deliveryNoteId, int deliveryRequestId)
        {
            var deliveryNote = await Repository.GetAsync(deliveryNoteId);
            if (deliveryNote == null)
            {
                throw new EntityNotFoundException(typeof(DeliveryNote), deliveryNoteId);
            }

            // Check if delivery note can be deleted
            if (deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
            {
                throw new UserFriendlyException("Không thể xóa phiếu xuất kho đã được xuất.");
            }

            // xóa kiện bao ra khỏi phiếu xuất kho hiện tại
            // Clear DeliveryNoteId references in related Packages
            var relatedPackages = await (await _packageRepository.GetAllAsync())
                .Where(p => p.DeliveryNoteId == deliveryNoteId)
                .ToListAsync();

            foreach (var package in relatedPackages)
            {
                package.DeliveryNoteId = null;
                package.ShippingStatus = (int)PackageDeliveryStatusEnum.InWarehouseVN;
                await _packageRepository.UpdateAsync(package);
            }

            // Clear DeliveryNoteId references in related Bags and update their status
            var relatedBags = await (await _bagRepository.GetAllAsync())
                .Include(b => b.Packages) // Include packages to update them as well
                .Where(b => b.DeliveryNoteId == deliveryNoteId)
                .ToListAsync();

            foreach (var bag in relatedBags)
            {
                bag.DeliveryNoteId = null;
                // Update bag status to GoToWarehouse as requested
                bag.ShippingStatus = (int)BagShippingStatus.GoToWarehouse;
                await _bagRepository.UpdateAsync(bag);

                // Also update all packages inside this bag
                if (bag.Packages != null)
                {
                    foreach (var package in bag.Packages)
                    {
                        package.ShippingStatus = (int)PackageDeliveryStatusEnum.InWarehouseVN;
                        await _packageRepository.UpdateAsync(package);
                    }
                }
            }

            // thêm kiện bao vào phiếu xuất kho hiện tại
            var bagDeliveryRequests = await (await _bagRepository.GetAllAsync())
                .Where(d => d.DeliveryRequestId == deliveryRequestId)
                .ToListAsync();
            var packageDeliveryRequests = await (await _packageRepository.GetAllAsync())
                .Where(d => d.DeliveryRequestId == deliveryRequestId)
                .ToListAsync();
            foreach (var bagDeliveryRequest in bagDeliveryRequests)
            {
                bagDeliveryRequest.DeliveryNoteId = deliveryNoteId;
                bagDeliveryRequest.ShippingStatus = (int)BagShippingStatus.WaitingForDelivery;
                await _bagRepository.UpdateAsync(bagDeliveryRequest);
            }
            foreach (var packageDeliveryRequest in packageDeliveryRequests)
            {
                packageDeliveryRequest.DeliveryNoteId = deliveryNoteId;
                packageDeliveryRequest.ShippingStatus = (int)PackageDeliveryStatusEnum.WaitingForDelivery;
                await _packageRepository.UpdateAsync(packageDeliveryRequest);
            }

        }

        public async Task<JsonResult> CancelDeliveryNoteAsync(int deliveryNoteId)
        {
            try
            {

                var affectedEntities = await GetDeliveryNoteItemsAsync(deliveryNoteId);

                var statusCodePr = new SqlParameter
                {
                    ParameterName = "@StatusCode",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };

                var messagePr = new SqlParameter
                {
                    ParameterName = "@Message",
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 500, // Độ dài tối đa của thông báo
                    Direction = ParameterDirection.Output
                };

                await ConnectDb.ExecuteNonQueryAsync(
                    "SP_DeliveryNotes_CancelDraft",
                    CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@DeliveryNoteId", deliveryNoteId),
                        new SqlParameter("@BagStatus", (int)BagShippingStatus.GoToWarehouse),
                        new SqlParameter("@PackageStatus", (int)PackageDeliveryStatusEnum.InWarehouseVN),
                        new SqlParameter("@DeliveryNoteStatus", (int)DeliveryNoteStatus.Cancel),
                        new SqlParameter("@LastModifierUserId", AbpSession.UserId ?? 0),
                        statusCodePr,
                        messagePr
                    }
                );

                var statusCode = (int)statusCodePr.Value;
                var message = messagePr.Value.ToString();

                if (statusCode != 200)
                {
                    return new JsonResult(new
                    {
                        Status = statusCode,
                        Message = message
                    });
                }

                var deliveryNote = new DeliveryNoteDto()
                {
                    Id = deliveryNoteId
                };


                await _entityChangeLoggerAppService.LogChangeAsync<DeliveryNoteDto>(
                    null,
                    deliveryNote,
                    "updated",
                    $"Hủy phiếu xuất kho #{deliveryNoteId}",
                    true
                );

                foreach (var entity in affectedEntities)
                {
                    if (entity.ItemType == (int)DeliveryNoteItemType.Bag)
                    {
                        var bagDto = new BagDto()
                        {
                            Id = entity.Id
                        };
                        await _entityChangeLoggerAppService.LogChangeAsync<BagDto>(
                            null,
                            bagDto,
                            "updated",
                            $"Hủy phiếu xuất kho #{deliveryNoteId} - Cập nhật trạng thái cho bao #{entity.Id} - {entity.BagCode}",
                            true
                        );
                        continue;
                    }
                    else
                    {
                        var packageDto = new PackageDto()
                        {
                            Id = entity.Id
                        };
                        await _entityChangeLoggerAppService.LogChangeAsync<PackageDto>(
                            null,
                            packageDto,
                            "updated",
                            $"Hủy phiếu xuất kho #{deliveryNoteId} - Cập nhật trạng thái cho kiện #{entity.Id} - {entity.PackageCode}",
                            true
                        );
                        continue;
                    }

                }

                return new JsonResult(new
                {
                    Status = 200,
                    Message = "Hủy phiếu xuất thành công."
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi hủy phiếu xuất. DeliveryNoteId: {deliveryNoteId}", ex);
                return new JsonResult(new
                {
                    Status = 500,
                    Message = "Có lỗi xảy ra khi hủy phiếu xuất."
                });
            }
        }

        public async Task<List<DeliveryNotePackageAndBagItemDto>> GetDeliveryNoteItemsAsync(long deliveryNoteId)
        {
            try
            {
                var items = await ConnectDb.GetListAsync<DeliveryNotePackageAndBagItemDto>(
                    "SP_DeliveryNotes_GetItems",
                    CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@DeliveryNoteId", SqlDbType.BigInt) { Value = deliveryNoteId }
                    });

                return items;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetDeliveryNoteItemsAsync: {ex.Message}", ex);
                throw new UserFriendlyException("Có lỗi xảy ra khi lấy danh sách các mục trong phiếu xuất kho.");
            }
        }

        /// <summary>
        /// 1. Admin
        /// 2. Sale Admin
        /// 3. Sale
        /// 4. Customer
        /// 5. WarehouseCN
        /// 6. WarehouseVN
        /// </summary>
        /// <returns></returns>
        private (int permissionCase, List<long> CustomerIds) GetPermissionCheckerWithCustomerIds()
        {
            try
            {
                var currentUserId = AbpSession.UserId;

                var permissionCase = -1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;

                var customerDtoIds = new List<CustomerIdDto>();

                // admin thì nhìn thấy tất cả kiện hàng
                if (PermissionChecker.IsGranted(PermissionNames.Role_Admin)
                    || PermissionChecker.IsGranted(PermissionNames.Function_PackageViewAll)
                    )
                {
                    // query = query.Where(x => x.CustomerId == _pbtAppSession.CustomerId);
                    permissionCase = 1;
                    Logger.Info("Admin hoặc Sale Admin truy cập vào danh sách kiện hàng.");
                }

                // Sale admin nhìn thấy tất cả các kiện hàng của mình, của sale dưới quyền và của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                    Logger.Info($"Sale Admin truy cập vào danh sách kiện hàng. SaleAdminId: {_pbtAppSession.CustomerId}");

                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleAdminUserId", CommandType.StoredProcedure,
                    new[]
                    {
                    new SqlParameter("@SaleAdminUserId", currentUserId)
                    });
                }

                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 3;
                    Logger.Info($"Sale truy cập vào danh sách kiện hàng. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleId", CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }
                // customer chỉ nhìn thấy kiện hàng của mình
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 4;
                    Logger.Info($"Customer truy cập vào danh sách kiện hàng. CustomerId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>(
                        "SP_Customers_GetIdsByParentId",
                        CommandType.StoredProcedure,
                        new[]
                        {
                        new SqlParameter("@CustomerId", _pbtAppSession.CustomerId)
                        }
                    );
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseCN))
                {
                    Logger.Info($"Warehouse CN truy cập vào danh sách kiện hàng. WarehouseId: {currentUserWarehouseId}");
                    permissionCase = 5;
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseVN))
                {
                    Logger.Info($"Warehouse VN truy cập vào danh sách kiện hàng. WarehouseId: {currentUserWarehouseId}");
                    permissionCase = 6;
                }
                else
                {
                    permissionCase = -1;
                    Logger.Warn($"Người dùng không có quyền truy cập danh sách kiện hàng. UserId: {currentUserId}");
                }

                return (permissionCase, customerDtoIds.Select(u => u.CustomerId).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Logger.Error("Gặp lỗi khi kiểm tra quyền lấy thông tin khách hàng theo người dùng", ex);
            }
            return (-1, new List<long>());
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

        private async Task<PackageDto> GetPackageByIdAsync(int packageId)
        {
            return await ConnectDb.GetItemAsync<PackageDto>("SP_Packages_GetById",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@id",
                    Value = packageId
                } });
        }

        private async Task<BagDto> GetBagByCodeAsync(string bagCode)
        {
            return await ConnectDb.GetItemAsync<BagDto>("SP_Bags_GetByBagCode",
                System.Data.CommandType.StoredProcedure,
                new[] { new SqlParameter() {
                    ParameterName = "@BagCode",
                    Value = bagCode
                } });
        }

        private async Task<DeliveryNote> GetDeliveryNoteByCustomerAndStatus(long customerId, int warehouseId, int status)
        {
            return await ConnectDb.GetItemAsync<DeliveryNote>("SP_DeliveryNotes_GetByCustomerAndWarehouseId",
               System.Data.CommandType.StoredProcedure,
               new[] {
                   new SqlParameter() {
                    ParameterName = "@CustomerId",
                    Value = customerId
                    } ,
                     new SqlParameter() {
                    ParameterName = "@WarehouseId",
                    Value = warehouseId
                    } ,
                    new SqlParameter() {
                    ParameterName = "@status",
                    Value = status
                    }
               });
        }

        private async Task<int> CreateAndGetIdDeliveryNoteAsync(DeliveryNote deliveryNote)
        {
            var outputDeliveryNoteIdParam = new SqlParameter
            {
                ParameterName = "@DeliveryNoteId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var outputStatusCodeParam = new SqlParameter
            {
                ParameterName = "@StatusCode",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var outputMessageParam = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = SqlDbType.NVarChar,
                Size = 255,
                Direction = ParameterDirection.Output
            };
            await ConnectDb.ExecuteNonQueryAsync("SP_DeliveryNotes_CreateSimple", CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@DeliveryNoteCode", deliveryNote.DeliveryNoteCode),
                    new SqlParameter("@CustomerId", deliveryNote.CustomerId),
                    new SqlParameter("@ExporterId", deliveryNote.ExporterId),
                    new SqlParameter("@ExportWarehouse", deliveryNote.ExportWarehouse),
                    new SqlParameter("@Status", deliveryNote.Status),
                    outputDeliveryNoteIdParam,
                    outputStatusCodeParam,
                    outputMessageParam
                });
            int statusCode = (int)outputStatusCodeParam.Value;
            if (statusCode < 0)
            {
                string message = outputMessageParam.Value.ToString();
                Logger.Error($"Error in CreateAndGetIdDeliveryNoteAsync: {message}");
                throw new Exception(message);
            }
            else
            {
                int deliveryNoteId = (int)outputDeliveryNoteIdParam.Value;
                return deliveryNoteId;
            }
        }

        private async Task<List<PackageDto>> GetPackagesByBagIdAsync(int bagId)
        {
            var data = await ConnectDb.GetListAsync<PackageDto>("SP_Packages_GetByBagId", CommandType.StoredProcedure, new[]
            {
                new SqlParameter("@BagId", SqlDbType.Int) { Value = bagId }
            });
            return data;
        }
    }
}