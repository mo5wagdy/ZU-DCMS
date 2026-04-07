using System;
using System.Collections.Generic;
using System.Text;

namespace ZU_DCMS.APPLICATION.Contracts
{
    /*
     * Cache service interface for managing cache operations in the application.
     * This interface defines methods for retrieving, setting, and removing cache entries.
     * It allows for generic types to be used, making it flexible for various caching scenarios.
     * The SetAsync method includes an optional duration parameter to specify how long the cache entry should be valid.
     */
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key); // => Generic method to get a value from the cache
        Task SetAsync<T>(string key, T value, TimeSpan? duration = null); // => Generic method to set a value in the cache with an optional expiration duration
        Task RemoveAsync(string key); // => Method to remove a specific key from the cache
        Task RemoveByPrefixAsync(string prefix); // => Method to remove all cache entries that start with a specific prefix, useful for bulk invalidation
    }
}
