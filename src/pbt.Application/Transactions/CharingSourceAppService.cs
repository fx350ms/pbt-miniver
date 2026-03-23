using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Transactions.Dto;
using System.Threading.Tasks;
using pbt.Entities;

namespace pbt.Transactions
{
    public class CharingSourceAppService : AsyncCrudAppService<CharingSource, CharingSourceDto, int, PagedResultRequestDto, CharingSourceDto, CharingSourceDto>, ICharingSourceAppService
    {
        public CharingSourceAppService(IRepository<CharingSource, int> repository)
            : base(repository)
        {
        }

        public override Task<CharingSourceDto> CreateAsync(CharingSourceDto input)
        {

            return base.CreateAsync(input);
        }
    }
}