using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Transactions.Dto;
using System.Threading.Tasks;


namespace pbt.Transactions
{
    public interface ICustomerTransactionAppService : IAsyncCrudAppService<CustomerTransactionDto, long, PagedResultRequestDto, CustomerTransactionDto, CustomerTransactionDto>
    {
        Task<CustomerTransactionDto> GetByCustomerIdAsync(long customerId);

        public Task<PagedResultDto<CustomerTransactionDto>> GetCurrentCustomerTransactionAsync(
            PagedCustomerTransactionResultRequestDto input);
    }
     
}
