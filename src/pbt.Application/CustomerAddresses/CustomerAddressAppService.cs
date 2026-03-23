using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.CustomerAddresss.Dto;
using pbt.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.Data.SqlClient;
using System;
using Microsoft.EntityFrameworkCore;
using pbt.Authorization;
using Abp.AutoMapper;
using pbt.Orders.Dto;


namespace pbt.CustomerAddresss
{
    public class CustomerAddressAppService : AsyncCrudAppService<CustomerAddress, CustomerAddressDto, long, PagedAndSortedResultRequestDto, CustomerAddressDto, CustomerAddressDto>, ICustomerAddressAppService
    {
        private pbtAppSession _pbtAppSession;
  

        public CustomerAddressAppService(IRepository<CustomerAddress, long> repository,
            pbtAppSession pbtAppSession)
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;
        }

        public async Task<List<CustomerAddressDto>> GetByCustomerId(long customerId)
        {
            var query = Repository.GetAll();
            query = query.Where(u => u.CustomerId == customerId);
            var data = query.ToList();
            // Nếu không tìm thấy, ném ngoại lệ
            if (data == null)
            {
                throw new UserFriendlyException($"CustomerAddress not found for customerId: {customerId}");
            }

            // Ánh xạ CustomerAddress sang CustomerAddressDto
            return ObjectMapper.Map<List<CustomerAddressDto>>(data);
        }



        public async Task<PagedResultDto<CustomerAddressDto>> GetByCurrentCustomer(PagedAndSortedResultRequestDto input)
        {
           
            if (_pbtAppSession.CustomerId.HasValue)
            {
                var customerId = _pbtAppSession.CustomerId;
                var query = Repository.GetAll();
                query = query.Where(u => u.CustomerId == customerId);

                var count = query.Count();
                query = ApplySorting(query, input);
                query = ApplyPaging(query, input);

                var data = query.ToList();

                return new PagedResultDto<CustomerAddressDto>()
                {
                    Items = ObjectMapper.Map<List<CustomerAddressDto>>(data),
                    TotalCount = count
                };

            }
            return new PagedResultDto<CustomerAddressDto>
            {
                Items = new List<CustomerAddressDto>(),
                TotalCount = 0
            };
            //var customerId = _pbtAppSession.CustomerId;

            //var query = Repository.GetAll();
            //query = query.Where(u => u.CustomerId == customerId);
            //var data = query.ToList();
            //// Nếu không tìm thấy, ném ngoại lệ
            //if (data == null)
            //{
            //    throw new UserFriendlyException($"CustomerAddress not found for customerId: {customerId}");
            //}

            //// Ánh xạ CustomerAddress sang CustomerAddressDto
            //return ObjectMapper.Map<List<CustomerAddressDto>>(data);
        }


        public override async Task<CustomerAddressDto> CreateAsync(CustomerAddressDto input)
        {
            if (input.IsDefault)
            {
                await UpdateDefault(input.CustomerId);
            }
            return await base.CreateAsync(input);
        }

        public override async Task<CustomerAddressDto> UpdateAsync(CustomerAddressDto input)
        {
            if (input.IsDefault)
            {
                await UpdateDefault(input.CustomerId);
            }

            return await base.UpdateAsync(input);
        }

        public async Task<int> UpdateDefault(long customerId)
        {
            try
            {
                var result = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                   "EXEC SP_CustomerAddresses_UpdateDefault   @UserId",
                   new SqlParameter("@UserId", customerId)
                );
                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }

        }
    }
}
