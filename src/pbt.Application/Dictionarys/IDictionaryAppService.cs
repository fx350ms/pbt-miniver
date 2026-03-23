using System.Threading.Tasks;
using Abp.Application.Services;
using pbt.Dictionary.Dto;


namespace pbt.Dictionary
{
    public interface IDictionaryAppService : IAsyncCrudAppService<DictionaryDto, int, PagedDictionaryResultRequestDto, CreateUpdateDictionaryDto, DictionaryDto>
    {
        public Task<string> GetDictionaryByCnName(string cnName);
    }
}
