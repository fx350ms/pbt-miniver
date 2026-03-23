using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.UI;
using pbt.Application.Cache;
using pbt.ConfigurationSettings.Dto;
using pbt.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ConfigurationSettings
{
    public class ConfigurationSettingAppService : AsyncCrudAppService<ConfigurationSetting, ConfigurationSettingDto, long, PagedResultRequestDto, ConfigurationSettingDto, ConfigurationSettingDto>, IConfigurationSettingAppService
    {
        private readonly ConfigAppCacheService _cacheService;

        public ConfigurationSettingAppService(IRepository<ConfigurationSetting, long> repository, ConfigAppCacheService cacheService) : base(repository)
        {
            _cacheService = cacheService;
        }
        public async Task<string> GetValueAsync(string key)
        {
            var cachedValue = _cacheService.GetCacheValue<string>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // If not in cache, get it from the repository
            var setting = await Repository.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                // Cache the value for future requests
                _cacheService.SetCacheValue(key, setting.Value);
                return setting.Value;
            }

            return null;
        }

        public async Task<T> GetValueTAsync<T>(string key)
        {
            // Kiểm tra giá trị trong cache
            var cachedValue = _cacheService.GetCacheValue<string>(key);
            if (cachedValue != null)
            {
                try
                {
                    return (T)Convert.ChangeType(cachedValue, typeof(T));
                }
                catch
                {
                    // Trả về giá trị mặc định nếu không thể chuyển đổi
                    return default(T);
                }
            }

            // Nếu không có trong cache, lấy từ repository
            var setting = await Repository.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                // Lưu giá trị vào cache
                _cacheService.SetCacheValue(key, setting.Value);

                try
                {
                    return (T)Convert.ChangeType(setting.Value, typeof(T));
                }
                catch
                {
                    // Trả về giá trị mặc định nếu không thể chuyển đổi
                    return default(T);
                }
            }

            // Trả về giá trị mặc định nếu không tìm thấy key
            return default(T);
        }

        public override async Task<ConfigurationSettingDto> UpdateAsync(ConfigurationSettingDto input)
        {
            await SetValueAsync(input.Key, input.Value);
            return await base.UpdateAsync(input);
        }

        public async Task<ConfigurationSettingDto> CreateWithCheckExistAsync(ConfigurationSettingDto input)
        {
            // Check if a configuration setting with the same key already exists
            var existingSetting = await Repository.FirstOrDefaultAsync(s => s.Key == input.Key);
            if (existingSetting != null)
            {
                throw new Exception($"A configuration setting with the key '{input.Key}' already exists.");
            }

            // If not exists, create the new configuration setting
            var result = await base.CreateAsync(input);

            // Cache the new value
            _cacheService.SetCacheValue(input.Key, input.Value);

            return result;
        }

        public async Task SetValueAsync(string key, string value)
        {
            var setting = await Repository.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                setting = new ConfigurationSetting { Key = key, Value = value };
                await Repository.InsertAsync(setting);
            }
            else
            {
                setting.Value = value;
                await Repository.UpdateAsync(setting);
            }

            // Update the cache with the new value
            _cacheService.SetCacheValue(key, value);
        }
        public async Task ResetCacheByKeyAsync(string key)
        {
            _cacheService.RemoveCacheValue(key);
        }

        public async Task ClearAllCacheAsync()
        {
            _cacheService.ClearCache();
        }



    }
}
