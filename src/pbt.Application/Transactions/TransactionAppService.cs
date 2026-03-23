using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Users;
using pbt.Customers;
using pbt.Entities;
using pbt.FileUploads;
using pbt.FileUploads.Dto;
using pbt.FundAccounts;
using pbt.Messages;
using pbt.OrderNumbers;
using pbt.Transactions.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pbt.Transactions
{
    [Authorize]
    [Audited]
    public class TransactionAppService : AsyncCrudAppService<Transaction, TransactionDto, long, PagedTransactionResultRequestDto, TransactionDto, TransactionDto>, ITransactionAppService
    {
        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly IRepository<Customer, long> _customerRepository;
        private readonly IRepository<CustomerTransaction, long> _customerTransactionRepository;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly string TransactionCodePrefix = "GD";
        private readonly IFileUploadAppService _fileUploadAppService;
        private readonly IRepository<User, long> _userRepository;


        private pbtAppSession _pbtAppSession;
        public TransactionAppService(IRepository<Transaction, long> repository,
            IFundAccountAppService fundAccountAppService,
            IRepository<Customer, long> customerAppService,
            IRepository<CustomerTransaction, long> customerTransactionRepository,
            IIdentityCodeAppService identityCodeAppService,
            IFileUploadAppService fileUploadAppService,
            pbtAppSession pbtAppSession,
            IRepository<User, long> userRepository
            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;
            _fundAccountAppService = fundAccountAppService;
            _customerRepository = customerAppService;
            _customerTransactionRepository = customerTransactionRepository;
            _identityCodeAppService = identityCodeAppService;
            _fileUploadAppService = fileUploadAppService;
            _userRepository = userRepository;
        }

        public async Task<PagedResultDto<TransactionDto>> GetAllDataAsync(PagedTransactionResultRequestDto input)
        {
            var query = base.CreateFilteredQuery(input);
            

            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                    x.TransactionId.Contains(input.Keyword) ||
                    x.OrderId.Contains(input.Keyword) ||
                    x.TransactionContent.Contains(input.Keyword) ||
                    x.RefCode.Contains(input.Keyword));
            }

            if (input.FundAccountId > 0)
            {
                query = query.Where(x => x.FundAccountId == input.FundAccountId);
            }
            else
            {
                if (!PermissionChecker.IsGranted(PermissionNames.Function_TransactionViewAll))
                {
                    // Lấy ra danh sách fund account được gán cho current userId

                    var fundAccountIds = await _fundAccountAppService.GetFundAccountsByCurrentUserAsync();
                    var fundAccountIdList = fundAccountIds.Select(x => x.Id).ToList();
                    query = query.Where(x => fundAccountIdList.Contains(x.FundAccountId));
                }
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime.Date >= input.StartDate.Value.Date);
            }

            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime.Date <= input.EndDate.Value.Date);
            }

            if (input.Status > 0)
            {
                query = query.Where(x => x.Status == input.Status);
            }

            if (input.TransactionType > 0)
            {
                query = query.Where(x => x.TransactionType == input.TransactionType);
            }

            if (input.ExecutionSource > 0)
            {
                query = query.Where(x => x.ExecutionSource == input.ExecutionSource);
            }

            if (input.TransactionDirection > 0)
            {
                query = query.Where(x => x.TransactionDirection == input.TransactionDirection);
            }

            if (input.MinAmount != null)
            {
                query = query.Where(x => x.Amount >= input.MinAmount);
            }

            if (input.MaxAmount != null)
            {
                query = query.Where(x => x.Amount <= input.MaxAmount);
            }

            var count = await query.CountAsync();
            query = query.OrderByDescending(x => x.Id);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);


            var data = await query.ToListAsync();

            var customerIds = data.Where(u => u.RecipientPayer.HasValue).Select(x => x.RecipientPayer).Distinct().ToList();

            var customers = await _customerRepository.GetAll()
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c);

            var result = new List<TransactionDto>();
            foreach (var item in data)
            {
                var dto = new TransactionDto()
                {
                    Id = item.Id,
                    TransactionId = item.TransactionId,
                    OrderId = item.OrderId,
                    TransactionContent = item.TransactionContent,
                    ExpensePurpose = item.ExpensePurpose,
                    ApproverUserId = item.ApproverUserId,
                    ConfirmerUserId = item.ConfirmerUserId,
                    RecipientPayer = item.RecipientPayer,
                    Amount = item.Amount,
                    TotalAmount = item.TotalAmount,
                    Currency = item.Currency,
                    Status = item.Status,
                    TransactionType = item.TransactionType,
                    ExecutionSource = item.ExecutionSource,
                    Notes = item.Notes,
                    FundAccountId = item.FundAccountId,
                    TransactionDirection = item.TransactionDirection,
                    RefCode = item.RefCode,
                    MessageId = item.MessageId,
                    CreationTime = item.CreationTime,

                    IsUpdateTransaction = item.IsUpdateTransaction,
                    CustomerTransactionUpdateType = item.CustomerTransactionUpdateType,

                };
                // Set CustomerName if RecipientPayer is set
                if (item.RecipientPayer.HasValue && customers.ContainsKey(item.RecipientPayer.Value))
                {
                    dto.CustomerName = customers[item.RecipientPayer.Value].FullName;
                }
                else
                {
                    dto.CustomerName = ""; // Hoặc để trống nếu không có khách hàng
                }

                result.Add(dto);

            }

            return new PagedResultDto<TransactionDto>
            {
                Items = result,
                TotalCount = count
            };
        }


        public async Task<bool> IsTransactionCodeDuplicateAsync(string transactionCode)
        {
            var existingTransaction = await Repository.FirstOrDefaultAsync(t => t.TransactionId == transactionCode);
            return existingTransaction != null;
        }


        public async Task<TransactionDto> CreateReceiptTransactionAsync(TransactionDto input)
        {
            try
            {
                // Log the start of the transaction creation
                Logger.Info($"Starting receipt transaction creation for type: {input.TransactionType}, amount: {input.Amount}");

                input.TransactionDirection = (int)TransactionDirectionEnum.Receipt;
                input.Status = (int)TransactionStatusEnum.PendingApprove;
                input.ExecutionSource = (int)TransactionSourceEnum.Manual;

                if (input.TransactionType == (int)TransactionTypeEnum.Deposit && !input.RecipientPayer.HasValue)
                {
                    Logger.Warn("RecipientPayer is required for deposit transaction");
                    throw new Exception("RecipientPayer is required for deposit transaction");
                }

                if (input.TransactionType == (int)TransactionTypeEnum.Payment && string.IsNullOrEmpty(input.OrderId))
                {
                    Logger.Warn("OrderId is required for payment transaction");
                    throw new Exception("OrderId is required for payment transaction");
                }

                var fundAccount = await _fundAccountAppService.GetAsync(new EntityDto<int>(input.FundAccountId));
                if (fundAccount == null || !fundAccount.IsActived)
                {
                    Logger.Warn("Fund account is not exist or not actived");
                    throw new Exception("Fund account is not exist or not actived");
                }

                var transactionCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(TransactionCodePrefix);

                // Total amount of transaction = function amount + amount fee
                input.TotalAmount = fundAccount.TotalAmount + input.Amount;
                input.TransactionId = transactionCode.Code;
                var createdTransaction = await base.CreateAsync(input);

                if (createdTransaction != null)
                {
                    // Log successful transaction creation
                    Logger.Info($"Transaction created successfully with ID: {createdTransaction.TransactionId}");

                    // Update fund account balance
                    fundAccount.TotalAmount = input.TotalAmount;
                    await _fundAccountAppService.UpdateAsync(fundAccount);
                }

                if (input.TransactionType == (int)TransactionTypeEnum.Deposit)
                {
                    // Log deposit transaction details
                    Logger.Info($"Processing deposit for customer ID: {input.RecipientPayer.Value}, amount: {input.Amount}");

                    var customerId = input.RecipientPayer.Value;
                    var customer = await _customerRepository.GetAsync(customerId);
                    if (customer == null)
                    {
                        Logger.Warn("Customer is not exist");
                        throw new Exception("Customer is not exist");
                    }
                    customer.CurrentAmount += input.Amount;


                    var customerTransaction = new CustomerTransaction
                    {
                        CustomerId = customerId,
                        TransactionType = (int)TransactionTypeEnum.Deposit,
                        Amount = input.Amount,
                        Description = "Nạp tiền vào ví",
                        ReferenceCode = input.TransactionId,
                        BalanceAfterTransaction = customer.CurrentAmount
                    };

                    var createdWalletTransaction = await _customerTransactionRepository.InsertAsync(customerTransaction);

                    await _customerRepository.UpdateAsync(customer);
                }
                else
                {
                    // Log payment transaction processing
                    Logger.Info($"Processing payment transaction for Order ID: {input.OrderId}");
                    // Implement payment processing logic here
                }
                return createdTransaction;
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while creating receipt transaction", ex);
                throw;
            }
        }

        public async Task<TransactionDto> CreatePaymentTransactionAsync(TransactionDto input)
        {
            try
            {
                // Log the start of the transaction creation
                Logger.Info($"Starting payment transaction creation for type: {input.TransactionType}, amount: {input.Amount}");


                input.TransactionDirection = (int)TransactionDirectionEnum.Expense;
                input.Status = (int)TransactionStatusEnum.PendingApprove;
                input.ExecutionSource = (int)TransactionSourceEnum.Manual;

                if (input.TransactionType == (int)TransactionTypeEnum.Deposit && !input.RecipientPayer.HasValue)
                {
                    Logger.Warn("RecipientPayer is required for deposit transaction");
                    throw new Exception("RecipientPayer is required for deposit transaction");
                }

                //if (input.TransactionType == (int)TransactionTypeEnum.Payment && string.IsNullOrEmpty(input.OrderId))
                //{
                //    Logger.Warn("OrderId is required for payment transaction");
                //    throw new Exception("OrderId is required for payment transaction");
                //}

                var fundAccount = await _fundAccountAppService.GetAsync(new EntityDto<int>(input.FundAccountId));
                if (fundAccount == null || !fundAccount.IsActived)
                {
                    Logger.Warn("Fund account is not exist or not actived");
                    throw new Exception("Fund account is not exist or not actived");
                }

                var transactionCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(TransactionCodePrefix);

                // Total amount of transaction = function amount + amount fee
                input.TotalAmount = fundAccount.TotalAmount - input.Amount;
                input.TransactionId = transactionCode.Code;
                var createdTransaction = await base.CreateAsync(input);

                if (createdTransaction != null)
                {
                    // Log successful transaction creation
                    Logger.Info($"Transaction created successfully with ID: {createdTransaction.TransactionId}");

                    // Update fund account balance
                    fundAccount.TotalAmount = input.TotalAmount;
                    await _fundAccountAppService.UpdateAsync(fundAccount);
                }

                if (input.TransactionType == (int)TransactionTypeEnum.Deposit)
                {
                    // Log deposit transaction details
                    Logger.Info($"Processing deposit for customer ID: {input.RecipientPayer.Value}, amount: {input.Amount}");

                    var customerId = input.RecipientPayer.Value;
                    var customer = await _customerRepository.GetAsync(customerId);
                    if (customer == null)
                    {
                        Logger.Warn("Customer is not exist");
                        throw new Exception("Customer is not exist");
                    }
                    customer.CurrentAmount += input.Amount;


                    var customerTransaction = new CustomerTransaction
                    {
                        CustomerId = customerId,
                        TransactionType = (int)TransactionTypeEnum.Deposit,
                        Amount = input.Amount,
                        Description = "Nạp tiền vào ví",
                        ReferenceCode = input.TransactionId,
                        BalanceAfterTransaction = customer.CurrentAmount
                    };

                    var createdWalletTransaction = await _customerTransactionRepository.InsertAsync(customerTransaction);

                    await _customerRepository.UpdateAsync(customer);
                }
                else
                if (input.TransactionType == (int)TransactionTypeEnum.Withdraw)
                {

                    // Log deposit transaction details
                    Logger.Info($"Processing deposit for customer ID: {input.RecipientPayer.Value}, amount: {input.Amount}");

                    var customerId = input.RecipientPayer.Value;
                    var customer = await _customerRepository.GetAsync(customerId);
                    if (customer == null)
                    {
                        Logger.Warn("Customer is not exist");
                        throw new Exception("Customer is not exist");
                    }
                    customer.CurrentAmount -= input.Amount;

                    var customerTransaction = new CustomerTransaction
                    {
                        CustomerId = customerId,
                        TransactionType = (int)TransactionTypeEnum.Deposit,
                        Amount = input.Amount,
                        Description = "Nạp tiền vào ví",
                        ReferenceCode = input.TransactionId,
                        BalanceAfterTransaction = customer.CurrentAmount
                    };
                    var createdWalletTransaction = await _customerTransactionRepository.InsertAsync(customerTransaction);
                    await _customerRepository.UpdateAsync(customer);
                }

                return createdTransaction;
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while creating receipt transaction", ex);
                throw;
            }
        }

        public async Task<TransactionDto> CreateAndConfirmPaymentTransactionAsync(TransactionDto input)
        {
            try
            {
                // Log the start of the transaction creation
                Logger.Info($"Starting payment transaction creation for type: {input.TransactionType}, amount: {input.Amount}");

                input.TransactionDirection = (int)TransactionDirectionEnum.Expense;
                input.Status = (int)TransactionStatusEnum.Approved;
                input.ExecutionSource = (int)TransactionSourceEnum.Manual;

                var fundAccount = await _fundAccountAppService.GetAsync(new EntityDto<int>(input.FundAccountId));
                if (fundAccount == null || !fundAccount.IsActived)
                {
                    Logger.Warn("Fund account is not exist or not actived");
                    throw new Exception("Fund account is not exist or not actived");
                }

                var transactionCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(TransactionCodePrefix);

                // Total amount of transaction = function amount + amount fee
                input.TotalAmount = fundAccount.TotalAmount - input.Amount;
                input.TransactionId = transactionCode.Code;
                if (input.TransactionType == 0)
                {
                    input.RecipientPayer = null;
                }

                var createdTransaction = await base.CreateAsync(input);

                if (createdTransaction != null)
                {
                    // Log successful transaction creation
                    Logger.Info($"Transaction created successfully with ID: {createdTransaction.TransactionId}");

                    // Update fund account balance
                    fundAccount.TotalAmount = input.TotalAmount;
                    await _fundAccountAppService.UpdateAsync(fundAccount);
                }

                if (input.TransactionType == (int)TransactionTypeEnum.Deposit)
                {
                    // Log deposit transaction details
                    Logger.Info($"Processing deposit for customer ID: {input.RecipientPayer.Value}, amount: {input.Amount}");

                    var customerId = input.RecipientPayer.Value;
                    var customer = await _customerRepository.GetAsync(customerId);
                    if (customer == null)
                    {
                        Logger.Warn("Customer is not exist");
                        throw new Exception("Customer is not exist");
                    }
                    customer.CurrentAmount += input.Amount;
                    var customerTransaction = new CustomerTransaction
                    {
                        CustomerId = customerId,
                        TransactionType = (int)TransactionTypeEnum.Deposit,
                        Amount = input.Amount,
                        Description = "Nạp tiền vào ví",
                        ReferenceCode = input.TransactionId,
                        BalanceAfterTransaction = customer.CurrentAmount
                    };

                    var createdWalletTransaction = await _customerTransactionRepository.InsertAsync(customerTransaction);

                    await _customerRepository.UpdateAsync(customer);
                }
                else
                {
                    // Log payment transaction processing
                    Logger.Info($"Processing payment transaction for Order ID: {input.OrderId}");
                    // Implement payment processing logic here

                }
                return createdTransaction;
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while creating receipt transaction", ex);
                throw;
            }
        }


        [AbpAuthorize(PermissionNames.Function_TransactionApprove)]
        public async Task<JsonResult> Approve(long id)
        {
         
            try
            {
                var transaction = await Repository.GetAsync(id);
                if(transaction == null)
                {
                    return new JsonResult(new { success = false, message = "Transaction not found" });
                }
                if(transaction.Status != (int)TransactionStatusEnum.PendingApprove)
                {
                    return new JsonResult(new { success = false, message = "Only transactions with Pending Approve status can be approved" });
                }

                var fundAccount = await _fundAccountAppService.GetAsync(new EntityDto<int>(transaction.FundAccountId));
                if (fundAccount == null || !fundAccount.IsActived)
                {
                    return new JsonResult(new { success = false, message = "Fund account is not exist or not actived" });
                }
                // Cập nhật số dư tài khoản quỹ
                if (transaction.TransactionDirection == (int)TransactionDirectionEnum.Receipt) // Thu
                {
                    fundAccount.TotalAmount += transaction.Amount;
                }
                else if (transaction.TransactionDirection == (int)TransactionDirectionEnum.Expense) // Chi
                {
                    fundAccount.TotalAmount -= transaction.Amount;
                }

                transaction.TotalAmount = fundAccount.TotalAmount;
                await _fundAccountAppService.UpdateAsync(fundAccount);
                transaction.Status = (int)TransactionStatusEnum.Approved;
                transaction.ApproverUserId = (int)_pbtAppSession.UserId;
                await Repository.UpdateAsync(transaction);

                return new JsonResult(new { success = true, message = "Transaction approved successfully" });

            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while approving transaction", ex);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<TransactionDto> CreateWithAttachmentAsync([FromForm] CreateTransactionWithAttachmentDto input)
        {
            try
            {
                if (input.TransactionDirection == 1) // Receipt
                {
                    input.ExpensePurpose = "Thu tiền khách hàng"; // Receipt purpose
                }
                else if (input.TransactionDirection == 2) // Payment
                {
                    input.ExpensePurpose = "Chi phí vận chuyển"; // Payment purpose
                }

                var isValidData = false;
                Customer customer = null;

                // Validate fund account
                var fundAccount = await _fundAccountAppService.GetAsync(new EntityDto<int>(input.FundAccountId));
                if (fundAccount == null)
                {
                    throw new Exception("Fund account is not exist or not actived");
                }

                // Cập nhật số dư tài khoản quỹ
                if (input.TransactionDirection == (int)TransactionDirectionEnum.Receipt) // Thu
                {
                    fundAccount.TotalAmount += input.Amount;
                }
                else if (input.TransactionDirection == (int)TransactionDirectionEnum.Expense) // Chi
                {
                    fundAccount.TotalAmount -= input.Amount;
                }

                // Nếu có customerId và cập nhật tài khoản
                if (input.RecipientPayer.HasValue && input.IsUpdateTransaction && input.CustomerTransactionUpdateType != (int)CustomerTransactionUpdateTypeEnum.None)
                {
                    customer = await _customerRepository.GetAsync(input.RecipientPayer.Value);
                    if (customer == null)
                    {
                        throw new Exception("Customer does not exist");
                    }
                    switch (input.CustomerTransactionUpdateType)
                    {
                        case (int)CustomerTransactionUpdateTypeEnum.AddWallet: // Cộng ví
                            customer.CurrentAmount += input.Amount;
                            break;

                        case (int)CustomerTransactionUpdateTypeEnum.SubtractWallet: // Trừ ví
                            // Vượt quá công nợ
                            if (customer.CurrentAmount < input.Amount && Math.Abs(customer.CurrentAmount - input.Amount) < customer.MaxDebt)
                            {

                                //   throw new Exception("Insufficient wallet balance");
                                throw new Exception("Debt limit exceeded");
                            }
                            customer.CurrentAmount -= input.Amount;
                            break;
 
                        default:
                            throw new Exception("Invalid CustomerTransactionUpdateType");
                    }
                    isValidData = true;
                }
                else
                {
                    isValidData = true;
                }

                if (isValidData)
                {
                    // Handle file attachments

                    var fileUploadStr = string.Empty;
                    if (input.Attachments != null && input.Attachments.Count > 0)
                    {
                        var fileUploadResult = await _fileUploadAppService.UploadFilesAsync(input.Attachments);
                        fileUploadStr = JsonConvert.SerializeObject(fileUploadResult);
                    }
                    var transactionCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(TransactionCodePrefix);
                    // Map and save transaction
                    var transaction = ObjectMapper.Map<Transaction>(input);
                    transaction.Files = fileUploadStr;
                    transaction.TransactionId = transactionCode.Code;
                    transaction.TotalAmount = fundAccount.TotalAmount + input.TotalAmount;
                    transaction.Status = (int)TransactionStatusEnum.PendingApprove;
                    transaction.TransactionDirection = input.TransactionDirection;
                    transaction.ExecutionSource = (int)TransactionSourceEnum.Manual;
                    transaction.Currency = fundAccount.Currency;
                    transaction = await Repository.InsertAsync(transaction);

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _fundAccountAppService.UpdateAsync(fundAccount);

                    if (input.RecipientPayer.HasValue)
                    {
                        var customerTransaction = new CustomerTransaction
                        {
                            CustomerId = input.RecipientPayer.Value,
                            Amount = input.Amount,
                            TransactionType = input.CustomerTransactionUpdateType switch
                            {
                                (int)CustomerTransactionUpdateTypeEnum.AddWallet or (int)CustomerTransactionUpdateTypeEnum.SubtractWallet => (int)PaymentMethod.Wallet,
                                //(int)CustomerTransactionUpdateTypeEnum.AddDebt or (int)CustomerTransactionUpdateTypeEnum.SubtractDebt => (int)PaymentMethod.Debt,
                                _ => 0
                            },
                            BalanceAfterTransaction = input.CustomerTransactionUpdateType switch
                            {
                                (int)CustomerTransactionUpdateTypeEnum.AddWallet or (int)CustomerTransactionUpdateTypeEnum.SubtractWallet => customer?.CurrentAmount ?? 0,
                                //(int)CustomerTransactionUpdateTypeEnum.AddDebt or (int)CustomerTransactionUpdateTypeEnum.SubtractDebt => customer?.CurrentDebt ?? 0,
                                _ => 0
                            },
                            Description = input.Notes,
                            ReferenceCode = input.RefCode,
                            Files = fileUploadStr
                        };
                        await _customerTransactionRepository.InsertAsync(customerTransaction);
                    }

                    var tranResult = ObjectMapper.Map<TransactionDto>(transaction);
                    if (customer != null)
                    {
                        await _customerRepository.UpdateAsync(customer);
                    }


                    return tranResult;
                }
                else
                {
                    throw new Exception("Invalid data");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while creating transaction with attachment", ex);
                throw;
            }
        }
    }
}