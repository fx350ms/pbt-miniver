using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.Configuration;
using pbt.ConfigurationSettings.Dto;
using pbt.Customers;
using pbt.Customers.Dto;
using pbt.Dictionary.Dto;
using pbt.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ConfigurationSettings
{
    public interface IConfigurationSettingAppService : IAsyncCrudAppService<ConfigurationSettingDto, long, PagedResultRequestDto, ConfigurationSettingDto, ConfigurationSettingDto>
    {
        public Task<ConfigurationSettingDto> CreateWithCheckExistAsync(ConfigurationSettingDto input);

        public Task<string> GetValueAsync(string key);

        public Task<T> GetValueTAsync<T>(string key);

        public Task SetValueAsync(string key, string value);

        public Task ResetCacheByKeyAsync(string key);

        public Task ClearAllCacheAsync();
    }
}
