using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BreweryAPI.Models.Service
{
 
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheProvider> _logger;

        public MemoryCacheProvider(IMemoryCache memoryCache, ILogger<MemoryCacheProvider> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public T? Get<T>(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("Attempted to get cache value with null or empty key");
                    return default(T);
                }

                if (_memoryCache.TryGetValue(key, out T? value))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return value;
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting value from cache for key: {Key}", key);
                return default(T);
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("Attempted to set cache value with null or empty key");
                    return;
                }

                if (value == null)
                {
                    _logger.LogWarning("Attempted to set null value in cache for key: {Key}", key);
                    return;
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration,
                    SlidingExpiration = TimeSpan.FromMinutes(5), // Refresh if accessed within 5 minutes
                    Priority = CacheItemPriority.Normal
                };

                _memoryCache.Set(key, value, cacheEntryOptions);
                _logger.LogDebug("Successfully cached value for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting value in cache for key: {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("Attempted to remove cache value with null or empty key");
                    return;
                }

                _memoryCache.Remove(key);
                _logger.LogDebug("Successfully removed cache entry for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing value from cache for key: {Key}", key);
            }
        }

        public bool Exists(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogWarning("Attempted to check cache existence with null or empty key");
                    return false;
                }

                return _memoryCache.TryGetValue(key, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking cache existence for key: {Key}", key);
                return false;
            }
        }
    }
}
