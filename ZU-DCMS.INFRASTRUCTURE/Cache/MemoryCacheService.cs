using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.APPLICATION.Common;
using ZU_DCMS.APPLICATION.Contracts;

namespace ZU_DCMS.INFRASTRUCTURE.Cache
{
    // In-memory cache implementation of ICacheService
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Retrieves a value from the cache based on the provided key
        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        // Stores a value in the cache with an optional expiration duration
        public Task SetAsync<T>(string key, T value, TimeSpan? duration = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    duration ?? CacheDuration.Medium
            };
            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        // Removes a value from the cache based on the provided key
        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        // Removes all cache entries that start with the specified prefix
        // Note: This implementation does not support prefix-based removal due to limitations of IMemoryCache.
        public Task RemoveByPrefixAsync(string prefix)
        {
            return Task.CompletedTask;
        }
    }
}
