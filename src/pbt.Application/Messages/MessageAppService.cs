using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Customers;
using pbt.Entities;
using pbt.FundAccounts;
using pbt.Messages.Dto;
using pbt.OrderNumbers;
using pbt.Security;
using pbt.Transactions;
using pbt.Transactions.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pbt.Messages
{
    [Audited]
    public class MessageAppService : AsyncCrudAppService<Message, MessageDto, long, PagedMessageResultRequestDto, MessageDto, MessageDto>, IMessageAppService
    {
        private pbtAppSession _pbtAppSession;
        private readonly string _securityKey;
        private readonly bool _debugCheckSign;
        private readonly string TransactionCodePrefix = "GD";

        private readonly IFundAccountAppService _fundAccountAppService;
        private readonly ICustomerTransactionAppService _customerTransactionAppService;
        private readonly ITransactionAppService _transactionAppService;
        private readonly IIdentityCodeAppService _identityCodeAppService;
        private readonly ICustomerAppService _customerAppService;


        public MessageAppService(IRepository<Message, long> repository,
            IFundAccountAppService fundAccountAppService,
            ICustomerTransactionAppService customerTransactionAppService,
            ITransactionAppService transactionAppService,
            IIdentityCodeAppService identityCodeAppService,
            ICustomerAppService customerAppService,
             pbtAppSession pbtAppSession,
            IConfiguration configuration)
            : base(repository)
        {
            _securityKey = configuration["ServiceSecurity:SecurityKey"];
            _debugCheckSign = Convert.ToBoolean(configuration["ServiceSecurity:DebugCheckSign"]);

            _fundAccountAppService = fundAccountAppService;
            _customerTransactionAppService = customerTransactionAppService;
            _transactionAppService = transactionAppService;
            _identityCodeAppService = identityCodeAppService;
            _customerAppService = customerAppService;
            _pbtAppSession = pbtAppSession;

        }


        public async override Task<PagedResultDto<MessageDto>> GetAllAsync(PagedMessageResultRequestDto input)
        {
            var currentUserId = AbpSession.UserId;

            var query = base.CreateFilteredQuery(input);
            
            //if (!string.IsNullOrEmpty(input.Keyword))
            //{
            //    query = query.Where(x => x.OrderNumber.ToUpper().Contains(input.Keyword.ToUpper()));
            //}

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreatedDate >= input.StartDate);
            }
            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreatedDate <= input.EndDate);
            }
            if (input.Status > 0)
            {
                query = query.Where(x => x.Status == input.Status);
            }
            if (input.MessageType > 0)
            {
                query = query.Where(x => x.MessageType == input.MessageType);
            }

            if (input.IsCorrectSyntax >= 0)
            {
                query = query.Where(x => x.IsCorrectSyntax == (input.IsCorrectSyntax == 1));
            }

            //.WhereIf(true, x => x.CreatorUserId == currentUserId)
            //.WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.OrderNumber.ToUpper().Contains(input.Keyword.ToUpper()))
            //.WhereIf(input.StartDate != null, x => x.CreationTime >= input.StartDate)
            //.WhereIf(input.EndDate != null, x => x.CreationTime <= input.EndDate)
            //.WhereIf(input.Status == -1, x => x.OrderStatus == input.Status)
            ;
            var count = query.Count();
            query = query.OrderByDescending(x => x.Id);
            query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
            return new PagedResultDto<MessageDto>()
            {
                Items = query.ToList().MapTo<List<MessageDto>>(),
                TotalCount = count
            };
          //  return base.GetAllAsync(input);
        }


        public async Task<ReceiveMessageReponseDto> ReceiveAsync(ReceiveMessageDto input)
        {
            
            Logger.Info($"Received message with Sign: {input.Sign}, DeviceName: {input.DeviceName}, DeviceId: {input.DeviceId}, Content: {input.Content}");
            // Use _securityKey as needed
            if (input == null || string.IsNullOrEmpty(input.Sign)
                || string.IsNullOrEmpty(input.DeviceName)
                || string.IsNullOrEmpty(input.DeviceId)
                || string.IsNullOrEmpty(input.Content)
                )
            {
                return new ReceiveMessageReponseDto
                {
                    Code = (int)ReceiveMessageResult.InvalidInput,
                    Message = ReceiveMessageResult.InvalidInput.GetDescription()
                };
            }
            try
            {

                if (_debugCheckSign || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
                {
                    var inputString = input.DeviceName + input.DeviceId + input.Content + DateTime.Now.ToString("ddMMyyyy") + _securityKey;
                    string sign = StringEncode.MD5(inputString);
                    if (!input.Sign.Equals(sign, StringComparison.OrdinalIgnoreCase))
                    {
                        return new ReceiveMessageReponseDto
                        {
                            Code = (int)ReceiveMessageResult.InvalidInput,
                            Message = ReceiveMessageResult.InvalidInput.GetDescription()
                        };
                    }
                }

                var message = new Message
                {
                    DeviceName = input.DeviceName,
                    DeviceId = input.DeviceId,
                    Content = input.Content,
                    Status = (int)MessageStatus.Received,
                    MessageType = (int)MessageType.Unknown,
                    CreatedDate = DateTime.Now
                };
                //     var messageAdded = await base.CreateAsync(message);
                

                var messageId  = await Repository.InsertAndGetIdAsync(message);
                message.Id = messageId;
                var tran = DetechMessageToTransaction(input.Content);
                if (tran != null)
                {
                    if (tran.IsCorrectSyntax)
                    {
                        message.MessageType = (int)MessageType.Deposit;
                        message.IsCorrectSyntax = true;
                        message.LastUpdatedDate = DateTime.Now;
                        var fundAccount = await _fundAccountAppService.GetFundAccountByAccountNumberAsync(tran.AccountNumber);
                        if (fundAccount == null)
                        {
                            message.Status = (int)MessageStatus.Failed;
                            message.Progress = ReceiveMessageResult.NotFoundBankAccount.GetDescription();

                        
                            await Repository.UpdateAsync(message);
                            return new ReceiveMessageReponseDto
                            {
                                Code = (int)ReceiveMessageResult.NotFoundBankAccount,
                                Message = ReceiveMessageResult.NotFoundBankAccount.GetDescription(),
                                MessageId = message.Id
                            };
                        }

                        var customer = await _customerAppService.GetByUsernameOrPhone(tran.Sender);

                        var transactionCode = await _identityCodeAppService.GenerateNewSequentialNumberAsync(TransactionCodePrefix);
                        //input.TotalAmount = fundAccount.TotalAmount + input.Amount;
                        //input.TransactionId = transactionCode.Code;

                        var transaction = new TransactionDto
                        {
                            RefCode = tran.ReferenceCode,
                            Amount = tran.Amount,
                            TransactionContent = input.Content,
                            TotalAmount = fundAccount.TotalAmount + tran.Amount,
                            TransactionId = transactionCode.Code,
                            TransactionDirection = (int)TransactionDirectionEnum.Receipt,
                            Status = (int)TransactionStatusEnum.PendingApprove,
                            ExecutionSource = (int)TransactionSourceEnum.Auto,
                            RecipientPayer = customer == null ? null : customer.Id,
                            TransactionType = (int)TransactionTypeEnum.Deposit,
                            FundAccountId = fundAccount.Id,
                        };
                        var createdTransaction = await _transactionAppService.CreateAsync(transaction);
                        fundAccount.TotalAmount = fundAccount.TotalAmount + tran.Amount;
                        await _fundAccountAppService.UpdateAsync(fundAccount);

                        if (customer == null)
                        {
                            message.Status = (int)MessageStatus.Failed;
                            message.Progress = ReceiveMessageResult.NotFoundCustomer.GetDescription();
                            await Repository.UpdateAsync(message);

                            return new ReceiveMessageReponseDto
                            {
                                Code = (int)ReceiveMessageResult.NotFoundCustomer,
                                Message = ReceiveMessageResult.NotFoundCustomer.GetDescription(),
                                MessageId = message.Id
                            };
                        }

                        customer.CurrentAmount += tran.Amount;

                        var customerTransaction = new CustomerTransactionDto
                        {
                            CustomerId = customer.Id,
                            TransactionType = (int)TransactionTypeEnum.Deposit,
                            Amount = tran.Amount,
                            Description = "Nạp tiền vào ví",
                            ReferenceCode = tran.ReferenceCode,
                            BalanceAfterTransaction = customer.CurrentAmount,
                            
                        };

                        var createdCustomerTransaction = await _customerTransactionAppService.CreateAsync(customerTransaction);
                      
                        await _customerAppService.UpdateAsync(customer);
                        await Repository.UpdateAsync(message);

                        message.Status = (int)MessageStatus.Completed;
                        message.Progress = MessageStatus.Completed.GetDescription();

                        return new ReceiveMessageReponseDto
                        {
                            Code = (int)ReceiveMessageResult.Success,
                            Message = ReceiveMessageResult.Success.GetDescription(),
                            MessageId = message.Id
                        };
                    }
                    else
                    {
                        message.MessageType = tran.MessageType;
                        message.IsCorrectSyntax = false;
                        message.Status = (int)MessageStatus.Failed;
                        message.Progress = ReceiveMessageResult.ErrorSyntax.GetDescription();
                        //   await base.UpdateAsync(messageAdded);
                        await Repository.UpdateAsync(message);
                        return new ReceiveMessageReponseDto
                        {
                            Code = (int)ReceiveMessageResult.ErrorSyntax,
                            Message = ReceiveMessageResult.ErrorSyntax.GetDescription(),
                            MessageId = message.Id
                        };
                    }
                }
                else
                {
                    message.Status = (int)MessageStatus.Failed;
                    message.Progress = ReceiveMessageResult.ErrorSyntax.GetDescription();
                    message.MessageType = (int)MessageType.Unknown;
                    message.IsCorrectSyntax = false;
                    await Repository.UpdateAsync(message);

                    return new ReceiveMessageReponseDto
                    {
                        Code = (int)ReceiveMessageResult.ErrorSyntax,
                        Message = ReceiveMessageResult.ErrorSyntax.GetDescription(),
                        MessageId = message.Id
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new ReceiveMessageReponseDto
                {
                    Code = (int)ReceiveMessageResult.SystemError,
                    Message = ReceiveMessageResult.SystemError.GetDescription(),
                };
            }

        }


        private MessageDetechToTransactionDto DetechMessageToTransaction(string transactionText)
        {
            try
            {

                string pattern = @"^SD TK (\d+)\s+\+([\d,]+)VND\s+luc\s+(\d{2}-\d{2}-\d{4}\s+\d{2}:\d{2}:\d{2})\.\s+SD\s+([\d,]+)VND\.\s+Ref\s+([^\s]+)\.VC\s+(\S+)(?:\s+(.*))?$";
                Regex regex = new Regex(pattern, RegexOptions.Compiled);
                Match match = regex.Match(transactionText);

                if (match.Success)
                {
                    // Cú pháp đúng: trích xuất các nhóm thông tin
                    string account = match.Groups[1].Value;
                    string amountStr = match.Groups[2].Value;
                    string transactionTime = match.Groups[3].Value;
                    string balanceStr = match.Groups[4].Value;
                    string reference = match.Groups[5].Value;
                    string customerAccount = match.Groups[6].Value;
                    string additionalInfo = match.Groups[7].Success ? match.Groups[7].Value : "";

                    // Loại bỏ dấu phẩy
                    amountStr = amountStr.Replace(",", "");
                    balanceStr = balanceStr.Replace(",", "");

                    return new MessageDetechToTransactionDto
                    {
                        AccountNumber = account, // Số tài khoản
                        Amount = decimal.Parse(amountStr.Replace(",", "")),// Số tiền cộng
                        TransactionTime = DateTime.ParseExact(transactionTime, "dd-MM-yyyy HH:mm:ss", null),// Thời gian giao dịch
                        BalanceAfterTransaction = decimal.Parse(balanceStr.Replace(",", "")),// Số dư sau giao dịch
                        ReferenceCode = match.Groups[5].Value, // Mã tham chiếu
                        Sender = customerAccount,  // Người chuyển khoản (nếu có)
                        Content = additionalInfo,
                        IsCorrectSyntax = true,
                        MessageType = (int)MessageType.Deposit
                    };

                }
                else
                {
                    // Cú pháp sai: chỉ xác định loại giao dịch (cộng/trừ tiền)
                    // Dùng regex đơn giản để tìm phần số tiền với dấu + hoặc -
                    Regex moneyRegex = new Regex(@"([\+\-])[\d,]+VND");
                    Match moneyMatch = moneyRegex.Match(transactionText);
                    if (moneyMatch.Success)
                    {
                        string sign = moneyMatch.Groups[1].Value;
                        if (sign == "+")
                        {
                            return new MessageDetechToTransactionDto
                            {
                                IsCorrectSyntax = false,
                                MessageType = (int)MessageType.Deposit
                            };
                        }
                        else if (sign == "-")
                        {
                            return new MessageDetechToTransactionDto
                            {
                                IsCorrectSyntax = false,
                                MessageType = (int)MessageType.Transfer
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi phân tích giao dịch: " + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Tạo phiếu thu
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<TransactionDto> CreateReceiptTransactionAsync(TransactionDto input)
        {
            try
            {
                // Log the start of the transaction creation
                Logger.Info($"Starting transaction creation for type: {input.TransactionType}, amount: {input.Amount}");

                input.TransactionDirection = (int)TransactionDirectionEnum.Receipt;
             
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
                input.Status = (int)TransactionStatusEnum.Approved;
                input.ExecutionSource = (int)TransactionSourceEnum.Manual;
                input.ApproverUserId = _pbtAppSession.UserId;

                var createdTransaction = await _transactionAppService.CreateAsync(input);

                if (createdTransaction != null)
                {
                    // Log successful transaction creation
                    Logger.Info($"Transaction created successfully with ID: {createdTransaction.TransactionId}");

                    // Update fund account balance
                    fundAccount.TotalAmount = input.TotalAmount;
                    await _fundAccountAppService.UpdateAsync(fundAccount);

                    var messageDto = await base.GetAsync(new EntityDto<long>(input.MessageId.Value));
                    messageDto.TransactionId = createdTransaction.Id;
                    messageDto.Status = (int)MessageStatus.Completed;
                    messageDto.LastUpdatedDate = DateTime.Now;
                    messageDto.Progress = MessageStatus.Completed.GetDescription();
                    await base.UpdateAsync(messageDto);

                }

                if (input.TransactionType == (int)TransactionTypeEnum.Deposit)
                {
                    // Log deposit transaction details
                    Logger.Info($"Processing deposit for customer ID: {input.RecipientPayer.Value}, amount: {input.Amount}");

                    var customerId = input.RecipientPayer.Value;
                    var customer = await _customerAppService.GetAsync(new EntityDto<long>(customerId));
                    if (customer == null)
                    {
                        Logger.Warn("Customer is not exist");
                        throw new Exception("Customer is not exist");
                    }
                    customer.CurrentAmount += input.Amount;

                    var customerTransaction = new CustomerTransactionDto
                    {
                        CustomerId = customer.Id,
                        TransactionType = (int)CustomerTransactionUpdateTypeEnum.AddWallet,
                        Amount = input.Amount,
                        Description = "Nạp tiền vào ví",
                        ReferenceCode = input.TransactionId,
                        BalanceAfterTransaction = customer.CurrentAmount
                    };

                    var createdCustomerTransaction = await _customerTransactionAppService.CreateAsync(customerTransaction);

                    await _customerAppService.UpdateAsync(customer);
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
    }
}