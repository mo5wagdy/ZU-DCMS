using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    public interface ICacheService
    {
        // Generic method to get a value from the cache
        Task<T?> GetAsync<T>(string key);

        // Generic method to set a value in the cache with an optional expiration duration
        Task SetAsync<T>(string key, T value, TimeSpan? duration = null);

        // Method to remove a specific key from the cache
        Task RemoveAsync(string key);

        // Method to remove all cache entries that start with a specific prefix
        Task RemoveByPrefixAsync(string prefix);
    }
}
