using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore.Repositories;
using System.Linq;

namespace pbt.ShippingRates
{
    public class ShippingRateCustomerAppService : AsyncCrudAppService<ShippingRateCustomer, ShippingRateCustomerDto, long, PagedResultRequestDto, ShippingRateCustomerDto, ShippingRateCustomerDto>, IShippingRateCustomerAppService
    {
        public readonly IRepository<Customer, long> _customerRepository;

        public ShippingRateCustomerAppService(IRepository<ShippingRateCustomer, long> repository, IRepository<Customer, long> customerRepository)
            : base(repository)
        {
            _customerRepository = customerRepository;
        }

        //public async Task<ListResultDto<ShippingRateCustomerDto>> GetShippingRatesForCustomer(long customerId)
        //{
        //    var shippingRates = await Repository.GetAllListAsync(src => src.CustomerId == customerId);
        //    return new ListResultDto<ShippingRateCustomerDto>(ObjectMapper.Map<List<ShippingRateCustomerDto>>(shippingRates));
        //}

        public async Task<List<ShippingRateCustomerCheckDto>> GetCustomersForShippingRate(long groupId)
        {
            try
            {
                var allCustomers = await _customerRepository.GetAllListAsync();

                // Get ShippingRateCustomer entries for the given groupId
                var shippingRateCustomers = await Repository.GetAllListAsync(src => src.ShippingRateGroupId == groupId);

                // Map ShippingRateCustomer to a dictionary for quick lookup
                var shippingRateCustomerMap = shippingRateCustomers.ToDictionary(src => src.CustomerId, src => src.CustomerId);

                // Create the result list
                var result = allCustomers.Select(customer => new ShippingRateCustomerCheckDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    ShippingRateGroupId = groupId,
                    Selected = shippingRateCustomerMap.ContainsKey(customer.Id)
                }).ToList();

                return result;
            }
            catch (System.Exception ex)
            {

                throw;
            }

            return null;
        }


        public async Task<List<long>> GetCustomerIdsForShippingRateAsync(long groupId)
        {
            try
            {
                
                // Get ShippingRateCustomer entries for the given groupId
                var shippingRateCustomers = await Repository.GetAllListAsync(src => src.ShippingRateGroupId == groupId);
                return shippingRateCustomers.Select(u => u.CustomerId).Distinct().ToList();

            }
            catch (System.Exception ex)
            {

                throw;
            }

            return null;
        }

        public async Task AssignCustomersToShippingRate(long groupId, List<long> customerIds)
        {
            // Get existing ShippingRateCustomer entries for the given groupId
            var existingEntries = await Repository.GetAllListAsync(src => src.ShippingRateGroupId == groupId);
            
            // Remove existing customer from another ShippingRateCustomer groupId
            var existingEntriesAnotherGroupId = await Repository.GetAllListAsync(src => 
                src.ShippingRateGroupId != groupId && customerIds.Contains(src.CustomerId));
            foreach (var existingEntriesAnother in existingEntriesAnotherGroupId)
            {
                await Repository.DeleteAsync(existingEntriesAnother);
            }
            
            // Remove existing entries that are not in the new customerIds list
            foreach (var entry in existingEntries)
            {
                if (!customerIds.Contains(entry.CustomerId))
                {
                    await Repository.DeleteAsync(entry);
                }
            }
            // Add new entries for customers that are not already assigned
            foreach (var customerId in customerIds)
            {
                if (!existingEntries.Any(src => src.CustomerId == customerId))
                {
                    var newEntry = new ShippingRateCustomer
                    {
                        ShippingRateGroupId = groupId,
                        CustomerId = customerId
                    };
                    await Repository.InsertAsync(newEntry);
                }
            }
        }
    }
}
