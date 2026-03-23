using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Users;
using pbt.Entities;
using pbt.Bags.Dto;
using pbt.ChangeLogger;
using pbt.Customers;
using pbt.Export;
using pbt.Export.Dto;
using pbt.Packages.Dto;
using pbt.OrderNumbers;
using pbt.FundAccounts;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.Data.SqlClient;
using NPOI.SS.Formula.Functions;

namespace pbt.Warehouses
{
    public class ExportAppService :
        AsyncCrudAppService<DeliveryNote, ExportDto, int, PagedBagResultRequestDto, CreateUpdateExportDto, ExportDto>,
        IExportAppService
    {
        private readonly IRepository<Bag, int> _bagRepository;
        private readonly IRepository<Package, int> _packageRepository;
        private readonly UserManager _userManager;
        private readonly ICustomerAppService _customerAppService;
        private readonly IRepository<Warehouse, int> _warehousesRepository;
        private readonly IRepository<DeliveryRequest, int> _deliveryRequestRepository;
        private readonly IRepository<DeliveryRequestOrder, int> _deliveryRequestOrderRepository;
        private readonly IRepository<Order, long> _orderRepository;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly IRepository<Transaction, long> _transactionRepository;
        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly IRepository<CustomerTransaction, long> _walletTransactionRepository;
        private readonly pbtAppSession _pbtAppSession;
        private readonly IEntityChangeLoggerAppService _entityChangeLoggerAppService;


        public ExportAppService(
            IRepository<DeliveryNote, int> repository,
            IRepository<Bag, int> bagRepository,
            UserManager userManager,
            IRepository<Package, int> packageRepository,
            IRepository<Warehouse, int> warehousesRepository,
            IRepository<DeliveryRequest, int> deliveryRequestRepository,
            IRepository<DeliveryRequestOrder, int> deliveryRequestOrderRepository,
            IRepository<Order, long> orderRepository,
            ICustomerAppService customerAppService,
            IIdentityCodeAppService identityCodeAppService,
            IRepository<Transaction, long> transactionRepository,
            IFundAccountAppService fundAccountAppService,
            IRepository<CustomerTransaction, long> walletTransactionRepository,
            pbtAppSession pbtAppSession,
            IEntityChangeLoggerAppService entityChangeLoggerAppService
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
            _customerAppService = customerAppService;
            _identityCodeAppService = identityCodeAppService;
            _transactionRepository = transactionRepository;
            _fundAccountAppService = fundAccountAppService;
            _walletTransactionRepository = walletTransactionRepository;
            _pbtAppSession = pbtAppSession;
            _entityChangeLoggerAppService = entityChangeLoggerAppService;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public override async Task<ExportDto> GetAsync(EntityDto<int> input)
        {
            var deliveryNote = await Repository.GetAsync(input.Id);

            if (deliveryNote == null)
            {
                throw new EntityNotFoundException(typeof(DeliveryNote), input.Id);
            }

            var creatorUser = await _userManager.FindByIdAsync(deliveryNote.CreatorUserId.ToString());
            var creatorName = creatorUser?.FullName ?? "Unknown";

            var exportDto = ObjectMapper.Map<ExportDto>(deliveryNote);

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

            var totalBag = (await _bagRepository.GetAllAsync()).AsEnumerable()
                .Count(x => x.DeliveryNoteId == deliveryNote.Id);
            var totalPackage = (await _packageRepository.GetAllAsync()).AsEnumerable()
                .Count(x => x.DeliveryNoteId == deliveryNote.Id);
            exportDto.TotalItem = totalBag + totalPackage;

            var totalWeightBag = (await _bagRepository.GetAllAsync()).AsEnumerable().Sum(x => x.Weight);
            var totalWeightPackage = (await _packageRepository.GetAllAsync()).AsEnumerable().Sum(x => x.Weight);
            exportDto.TotalWeight = totalWeightBag + totalWeightPackage;

            exportDto.CreatorName = creatorName; // Thêm trường tên người tạo vào DTO

            return exportDto;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deliveryNoteId"></param>
        /// <returns></returns>
        public async Task<ExportItemsDto> GetItemByDeliveryNote(int deliveryNoteId)
        {
            // get bag
            var bags = (await _bagRepository.GetAllAsync()).Include(x => x.Packages).Where(x => x.DeliveryNoteId == deliveryNoteId);
            var bagDto = ObjectMapper.Map<List<BagDto>>(bags);
            foreach (var dto in bagDto)
            {
                dto.TotalPackages = bags.FirstOrDefault(x => x.Id == dto.Id)?.Packages.Count();
            }
            // get package
            var packages = (await _packageRepository.GetAllAsync()).Include(x => x.Bag)
                .Where(x => x.DeliveryNoteId == deliveryNoteId && x.Bag.BagType == (int)BagTypeEnum.InclusiveBag);
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
                ;

                var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == package.OrderId);
                if (order != null)
                {
                    package.OrderCode = order?.OrderNumber;
                }
                packageDeliveryRequestDtoResult.Add(package);
            }

            return new ExportItemsDto()
            {
                Bags = bagDto,
                Packages = packageDeliveryRequestDtoResult
            };
        }

        public async Task<ListResultDto<ExportDto>> getDeliveryNotesFilter(PagedExportResultRequestDto input)
        {
            try
            {
                var query = (await Repository.GetAllAsync());

                query = query.OrderByDescending(x => x.CreationTime);
                var exportDto = ObjectMapper.Map<List<ExportDto>>(query);
                return new PagedResultDto<ExportDto>()
                {
                    Items = exportDto,
                    TotalCount = DynamicQueryableExtensions.Count(query),
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ExportDto> CreateWithTransactionsAsync(CreateUpdateExportDto input)
        {
            try
            {

                if (input.ItemBags == null && input.ItemPackages == null
                    || input.ItemBags is { Count: 0 } && input.ItemPackages is { Count: 0 })
                {
                    throw new UserFriendlyException("Vui lòng thêm bao, kiện.");
                }

                // Lấy quỹ của kho
                var currentFundAccount = (await _fundAccountAppService.GetFundAccountsByCurrentUserAsync()).FirstOrDefault();

                if (currentFundAccount == null)
                {
                    throw new UserFriendlyException("Không có quỹ nào.");
                }

                // Nếu 

                var deliveryNote = ObjectMapper.Map<DeliveryNote>(input);
                deliveryNote.DeliveryNoteCode = (await Repository.GetAllAsync())?.Max(x => x.DeliveryNoteCode);

                // create identityCode pxk
                var identityDeliveryNoteCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("PXK");

                deliveryNote.DeliveryNoteCode = identityDeliveryNoteCode.Code;
                deliveryNote.CustomerId = input.CustomerId;
                if (deliveryNote.Status == 1)
                {
                    deliveryNote.ExportTime = DateTime.Now;
                }

                var deliveryNoteInserted = await Repository.InsertAsync(deliveryNote);

                // add log DeliveryNote
                _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                    null
                    , deliveryNote
                    , "created"
                    , $"Phiếu xuất kho đã được tạo: #{deliveryNote.DeliveryNoteCode} "
                    , true
                );

                if (deliveryNote.Status == 1)
                {
                    // add log DeliveryNote
                    _entityChangeLoggerAppService.LogChangeAsync<DeliveryNote>(
                        null
                        , deliveryNote
                        , "updated"
                        , $"Xuất kho"
                        , true
                    );
                }

                decimal totalBagWeight = 0;
                decimal totalPackagesWeight = 0;
                string bagIds = string.Empty, packageIds = string.Empty;

                if (input.ItemBags != null)
                {
                    bagIds = string.Join(",", input.ItemBags ?? new List<int>());
                }
                #region
                //foreach (var itemId in input.ItemBags)
                //{
                //    var currentBag = _bagRepository.FirstOrDefault(x => x.Id == itemId);
                //    if (currentBag != null)
                //    {
                //        currentBag.DeliveryNoteId = deliveryNoteId;
                //        currentBag.ShippingStatus = (int)BagShippingStatus.Delivery;
                //        currentBag.DeliveryDate = DateTime.Now;
                //        currentBag.WarehouseStatus = (int)WarehouseStatus.OutOfStock;
                //        await _bagRepository.UpdateAsync(currentBag);

                //        // add log
                //        _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                //            null
                //            , currentBag
                //            , "updated"
                //            , $"Bao hàng đã được tạo phiếu xuất kho: #{deliveryNote.DeliveryNoteCode} "
                //            , true
                //        );

                //        if (deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                //        {
                //            // add log bag
                //            _entityChangeLoggerAppService.LogChangeAsync<Bag>(
                //                null
                //                , currentBag
                //                , "updated"
                //                , $"Xuất kho bao"
                //                , true
                //            );
                //        }

                //        totalBagWeight += currentBag.Weight ?? 0;
                //    }

                //    if (currentBag == null) continue;

                //    var packages = (await _packageRepository.GetAllAsync()).Where(x => x.BagId == currentBag.Id);
                //    foreach (var package in packages)
                //    {
                //        package.DeliveryNoteId = deliveryNoteId;
                //        if (deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                //        {
                //            package.WarehouseStatus = (int)WarehouseStatus.OutOfStock;
                //            package.ShippingStatus = (int)PackageDeliveryStatusEnum.DeliveryInProgress;
                //            package.DeliveryTime = DateTime.Now;
                //        }
                //        await _packageRepository.UpdateAsync(package);
                //        // add log
                //        _entityChangeLoggerAppService.LogChangeAsync<Package>(
                //            null
                //            , package
                //            , "updated"             
                //            , $"Kiện hàng đã được tạo phiếu xuất kho: #{deliveryNote.DeliveryNoteCode} "
                //            , true
                //        );

                //        if (deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                //        {
                //            // add log package
                //            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                //                null
                //                , package
                //                , "updated"
                //                , $"Xuất kho kiện"
                //                , true
                //            );
                //        }
                //    }
                //    var orderIds = currentBag.Packages?.Select(x => x.OrderId).Distinct();
                //    if (orderIds != null)
                //    {
                //        var order = (await _orderRepository.GetAllAsync()).Where(x => orderIds.Contains(x.Id));
                //        // update order status
                //        foreach (var orderItem in order)
                //        {
                //            orderItem.OrderStatus = (int)OrderStatus.OutForDelivery;
                //            await _orderRepository.UpdateAsync(orderItem);
                //        }
                //    }
                //}
                #endregion
                if (input.ItemPackages != null)
                {
                    packageIds = string.Join(",", input.ItemPackages ?? new List<int>());
                }
                /*
                foreach (var itemId in input.ItemPackages)
                {
                    var currentPackage = await _packageRepository.FirstOrDefaultAsync(x => x.Id == itemId);
                    if (currentPackage != null)
                    {
                        currentPackage.DeliveryNoteId = deliveryNoteId;
                        if (deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                        {
                            currentPackage.ShippingStatus = (int)PackageDeliveryStatusEnum.DeliveryInProgress;
                            currentPackage.WarehouseStatus = (int)WarehouseStatus.OutOfStock;
                            currentPackage.DeliveryTime = DateTime.Now;
                        }

                        await _packageRepository.UpdateAsync(currentPackage);

                        if(deliveryNote.Status == (int)DeliveryNoteStatus.Delivered)
                        {
                            // add log package
                            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                                null
                                , currentPackage
                                , "updated"
                                , $"Xuất kho kiện"
                                , true
                            );
                        }
                        else
                        {
                            // add log
                            _entityChangeLoggerAppService.LogChangeAsync<Package>(
                                null
                                , currentPackage
                                , "updated"
                                , $"Kiện hàng đã được tạo phiếu xuất kho: #{deliveryNote.DeliveryNoteCode} "
                                , true
                            );

                        }

                        totalPackagesWeight += currentPackage.Weight ?? 0;
                    }
                    // update order status
                    var order = await _orderRepository.FirstOrDefaultAsync(x => x.Id == currentPackage.OrderId);
                    if (order == null) continue;
                    order.OrderStatus = (int)OrderStatus.OutForDelivery;
                    await _orderRepository.UpdateAsync(order);
                }
                */

                try
                {
                    var excuteResult = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(

                      "EXEC SP_DeliveryUpdate  @DeliveryNoteId, @BagIds, @PackageIds, @BagStatus, @PackageStatus, @OrderStatus, @WarehouseStatus",
                        new SqlParameter("@DeliveryNoteId", deliveryNoteInserted.Id),
                        new SqlParameter("@BagIds", bagIds),
                        new SqlParameter("@PackageIds", packageIds),
                        new SqlParameter("@BagStatus", (int)BagShippingStatus.Delivery),
                        new SqlParameter("@packageStatus", (int)PackageDeliveryStatusEnum.DeliveryInProgress),
                        new SqlParameter("@orderStatus", (int)OrderStatus.Delivered),
                        new SqlParameter("@warehouseStatus", (int)WarehouseStatus.OutOfStock)
                       
                   );

                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException($"Error updating delivery status: {ex.Message}");
                }

                // update TotalWeight

                //deliveryNoteInserted.TotalWeight = totalBagWeight + totalPackagesWeight;

                //await Repository.UpdateAsync(deliveryNoteInserted);

                if (input.DeliveryFeeReason.HasValue)
                {

                    var identityCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync("GD");
                    // Lấy mã giao dịch

                    // Tạo phiếu chi
                    var transaction = new Transaction()
                    {
                        TransactionId = identityCode.Code,
                        Amount = input.ShippingFee ?? 0,
                        TransactionType = (int)TransactionTypeEnum.Payment, // Thanh toán
                        Status = (int)TransactionStatusEnum.Approved,
                        ExecutionSource = (int)TransactionSourceEnum.Manual,
                        TransactionDirection = (int)TransactionDirectionEnum.Expense, // Phiếu chi
                        TotalAmount = currentFundAccount.TotalAmount - (input.ShippingFee ?? 0),
                        Currency = "VNĐ",
                        TransactionContent = "Chi phí giao hàng",
                        ExpensePurpose = "Chi phí giao hàng",
                        FundAccountId = currentFundAccount.Id,
                        RefCode = deliveryNote.Id + " | " + deliveryNote.DeliveryNoteCode,
                    };

                    await _transactionRepository.InsertAndGetIdAsync(transaction);

                    // Trừ tiền quỹ kho
                    if (input.ShippingFee.HasValue && input.ShippingFee.Value > 0)
                    {
                        currentFundAccount.TotalAmount -= input.ShippingFee.Value;
                        await _fundAccountAppService.UpdateAsync(currentFundAccount);
                    }

                    if (input.DeliveryFeeReason == (int)DeliveryFeeType.WithFee)
                    {
                        // Trừ ví hoặc tăng công nợ của khách hàng
                        var customer = await _customerAppService.GetAsync(new EntityDto<long>(input.CustomerId));
                        if (input.ShippingFee > customer.CurrentAmount)
                        {
                            // Sử dụng công nợ
                            customer.CurrentDebt += input.ShippingFee ?? 0 - customer.CurrentAmount;
                            customer.CurrentAmount = 0;
                            await _customerAppService.UpdateAsync(customer);
                        }
                        else
                        {
                            // Trừ tiền trong ví
                            customer.CurrentAmount -= (input.ShippingFee ?? 0);

                            // Tạo giao dịch cho ví khách hàng
                            var walletTransaction = new CustomerTransaction()
                            {
                                CustomerId = customer.Id,
                                Amount = input.ShippingFee ?? 0,
                                TransactionType = (int)TransactionTypeEnum.Payment,
                                BalanceAfterTransaction = customer.CurrentAmount,
                                ReferenceCode = deliveryNote.Id + " | " + deliveryNote.DeliveryNoteCode,
                                Description = "Chi phí giao hàng",
                                TransactionDate = DateTime.Now,

                            };
                            await _walletTransactionRepository.InsertAsync(walletTransaction);
                        }
                        await _customerAppService.UpdateAsync(customer);
                        // Tạo giao dịch cho ví khách hàng

                    }

                }

                return MapToEntityDto(deliveryNote);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<ExportDto> UpdateStatusDeliveryNoteAsync(UpdateStatusDto input)
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

            var bags = (await _bagRepository.GetAllAsync()).Include(x => x.Packages).Where(x => x.DeliveryNoteId == deliveryNote.Id);
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
                        , $"Xuất kho bao"
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
                                , $"Xuất kho kiện"
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
                        , $"Xuất kho kiện"
                        , true
                    );
                }
            }

            return MapToEntityDto(deliveryNote);
        }

        public async Task<DeliveryNoteDetail> getDeliveryNoteByIdAsync(int id)
        {
            var _dto = (await Repository.GetAllAsync()).FirstOrDefault(x => x.Id == id);
            ExportDto dto = ObjectMapper.Map<ExportDto>(_dto);
            // get user login
            dto.CreatorName = _pbtAppSession.UserName;
            var model = new DeliveryNoteDetail()
            {
                Dto = dto
            };
            return model;
        }
    }
}