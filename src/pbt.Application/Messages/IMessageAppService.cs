using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Messages.Dto;
using pbt.Orders.Dto;
using pbt.Transactions.Dto;
using System.Threading.Tasks;

namespace pbt.Messages
{
    public interface IMessageAppService : IAsyncCrudAppService<MessageDto, long, PagedMessageResultRequestDto, MessageDto, MessageDto>
    {
        public Task<ReceiveMessageReponseDto> ReceiveAsync(ReceiveMessageDto input);

        public Task<TransactionDto> CreateReceiptTransactionAsync(TransactionDto input);
    }
}