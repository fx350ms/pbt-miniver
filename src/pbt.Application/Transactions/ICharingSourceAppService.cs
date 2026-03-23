using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Transactions.Dto;

namespace pbt.Transactions
{
    public interface ICharingSourceAppService : IAsyncCrudAppService<CharingSourceDto, int, PagedResultRequestDto, CharingSourceDto, CharingSourceDto>
    {
    }
}