using Microsoft.Extensions.Caching.Memory;

namespace pbt.Application.Cache
{
    public class ConfigAppCacheService
    {
        private readonly IMemoryCache _cache;

        public ConfigAppCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Method to set a cache value
        public void SetCacheValue<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(60) // Default to 60 minutes
            };

            _cache.Set(key, value, cacheEntryOptions);
        }

        // Method to get a cache value
        public T GetCacheValue<T>(string key)
        {
            _cache.TryGetValue(key, out T value);
            return value;
        }

        public bool TryGetCacheValue<T>(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        // Hàm Get hoặc Create thông minh
        public T GetOrCreate<T>(string key, Func<ICacheEntry, T> factory)
        {
            return _cache.GetOrCreate(key, factory);
        }

        // Method to remove a cache value
        public void RemoveCacheValue(string key)
        {
            _cache.Remove(key);
        }

        // Method to reset/clear all cache values
        public void ClearCache()
        {
            // Assuming _cache is of type MemoryCache
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0); // Remove all cache entries
            }
        }
    }
}
