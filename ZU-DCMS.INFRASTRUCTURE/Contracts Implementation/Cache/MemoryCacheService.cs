using Microsoft.Extensions.Caching.Memory;
using ZU_DCMS.APPLICATION.Common.Cache;
using ZU_DCMS.APPLICATION.Contracts.Cache;

namespace ZU_DCMS.INFRASTRUCTURE.Cache
{
    // __ In-memory cache implementation of ICacheService __ //
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // __ Retrieves a value from the cache based on the provided key __ //
        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
           
            return Task.FromResult(value);
        }

        // __ Stores a value in the cache with an optional expiration duration __ //
        public Task SetAsync<T>(string key, T value, TimeSpan? duration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration ?? CacheDuration.Medium
            };

            _cache.Set(key, value, options);
            
            return Task.CompletedTask;
        }

        // __ Removes a value from the cache based on the provided key __ //
        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            
            return Task.CompletedTask;
        }

        // __ Removes all cache entries that start with a specific prefix __ //
        public Task RemoveByPrefixAsync(string prefix)
        {
            _cache.Remove(prefix);
           
            return Task.CompletedTask;
        }
    }
}