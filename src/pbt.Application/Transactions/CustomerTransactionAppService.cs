using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Entities;
using pbt.FileUploads;
using pbt.FileUploads.Dto;
using pbt.Transactions.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace pbt.Transactions
{
    [Authorize]
    public class CustomerTransactionAppService : AsyncCrudAppService<CustomerTransaction, CustomerTransactionDto, long, PagedResultRequestDto, CustomerTransactionDto, CustomerTransactionDto>, ICustomerTransactionAppService
    {
        private readonly pbtAppSession _pbtAppSession;
        private readonly IRepository<Customer, long> _customerRepository;
        private readonly IFileUploadAppService _fileUploadAppService;

        public CustomerTransactionAppService(
            IRepository<CustomerTransaction, long> repository,
            IRepository<Customer, long> customerRepository,
            IFileUploadAppService fileUploadAppService,
                pbtAppSession pbtAppSession
            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;
            _customerRepository = customerRepository;
            _fileUploadAppService = fileUploadAppService;
        }

        public async Task<CustomerTransactionDto> GetByCustomerIdAsync(long customerId)
        {
            var CustomerTransaction = (await Repository.GetAllAsync())
                .Where(u => u.CustomerId == customerId)
                .FirstOrDefault();

            return ObjectMapper.Map<CustomerTransactionDto>(CustomerTransaction);
        }

        protected override IQueryable<CustomerTransaction> CreateFilteredQuery(PagedResultRequestDto input)
        {
            var userId = AbpSession.UserId; /// Will change to customerId
            var query = Repository.GetAll()
                .Where(u => u.CustomerId == userId.Value);
            return query;
        }



        // get current customer transaction
        /// <summary>
        /// Lấy danh sách giao dịch của khách hàng hiện tại
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<CustomerTransactionDto>> GetTransactionAsync(PagedCustomerTransactionRequestDto input)
        {

            var query = (await Repository.GetAllAsync())
                .Where(u => u.CustomerId == input.CustomerId);
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                    x.Description.Contains(input.Keyword) ||
                    x.ReferenceCode.Contains(input.Keyword) ||
                    x.Amount.ToString().Contains(input.Keyword));
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime.Date >= input.StartDate.Value.Date);
            }

            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime.Date <= input.EndDate.Value.Date);
            }


            if (input.TransactionType > 0)
            {
                query = query.Where(x => x.TransactionType == input.TransactionType);
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

            var data = ObjectMapper.Map<List<CustomerTransactionDto>>(query.ToList());
            return new PagedResultDto<CustomerTransactionDto>()
            {
                Items = data,
                TotalCount = count
            };
        }

        // get current customer transaction
        /// <summary>
        /// Lấy danh sách giao dịch của khách hàng hiện tại
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PagedResultDto<CustomerTransactionDto>> GetCurrentCustomerTransactionAsync(PagedCustomerTransactionResultRequestDto input)
        {
            var userId = _pbtAppSession.CustomerId;
            var query = (await Repository.GetAllAsync())
                .Where(u => u.CustomerId == userId.Value);
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x =>
                    x.Description.Contains(input.Keyword) ||
                    x.ReferenceCode.Contains(input.Keyword) ||
                    x.Amount.ToString().Contains(input.Keyword));
            }

            if (input.StartDate != null)
            {
                query = query.Where(x => x.CreationTime.Date >= input.StartDate.Value.Date);
            }

            if (input.EndDate != null)
            {
                query = query.Where(x => x.CreationTime.Date <= input.EndDate.Value.Date);
            }


            if (input.TransactionType > 0)
            {
                query = query.Where(x => x.TransactionType == input.TransactionType);
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

            var data = ObjectMapper.Map<List<CustomerTransactionDto>>(query.ToList());
            return new PagedResultDto<CustomerTransactionDto>()
            {
                Items = data,
                TotalCount = count
            };
        }


        [HttpPost]
        public async Task<CustomerTransactionDto> CreateWithAttachmentAsync([FromForm] CreateCustomerTransactionWithAttachmentDto input)
        {
            try
            {
                var customer = await _customerRepository.GetAsync(input.CustomerId);
                if (customer == null)
                {
                    Logger.Warn("Customer not found with ID: " + input.CustomerId);
                    throw new Exception("Customer does not exist");
                }

                if (input.CustomerTransactionUpdateType == (int)CustomerTransactionUpdateTypeEnum.AddWallet) // Cộng ví
                {
                    customer.CurrentAmount += input.Amount;
                }

                else
                {
                    customer.CurrentAmount -= input.Amount;
                }

                var fileUploadStr = string.Empty;
                if (input.Attachments != null && input.Attachments.Count > 0)
                {
                    var fileUploadResult = await _fileUploadAppService.UploadFilesAsync(input.Attachments);
                    fileUploadStr = JsonConvert.SerializeObject(fileUploadResult);
                }

                var customerTransaction = new CustomerTransaction
                {
                    CustomerId = input.CustomerId,
                    Amount = input.Amount,
                    TransactionType = input.CustomerTransactionUpdateType  ,
                    BalanceAfterTransaction = customer.CurrentAmount,
                    Description = input.Notes,
                    ReferenceCode = input.RefCode,
                    Files = fileUploadStr
                };
                await Repository.InsertAsync(customerTransaction);
                await _customerRepository.UpdateAsync(customer);

                return ObjectMapper.Map<CustomerTransactionDto>(customerTransaction);

            }
            catch (Exception ex)
            {
                Logger.Error("Error in CreateWithAttachmentAsync: " + ex.Message);
                throw ex;
            }
        }
    }
}
