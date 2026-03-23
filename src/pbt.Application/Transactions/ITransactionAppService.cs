using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Transactions.Dto;
using pbt.Entities;
using System.Threading.Tasks;
namespace pbt.Transactions
{
    public interface ITransactionAppService : IAsyncCrudAppService<TransactionDto, long, PagedTransactionResultRequestDto, TransactionDto, TransactionDto>
    {
        public Task<TransactionDto> CreateReceiptTransactionAsync(TransactionDto input);

        /// <summary>
        /// lấy danh sách giao dịch theo filter
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<PagedResultDto<TransactionDto>> GetAllDataAsync(PagedTransactionResultRequestDto input);
    }
}