namespace BreweryAPI.Models.Service.Interface
{
   
    public interface ICacheProvider
    {
        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        /// <typeparam name="T">Type of the cached value.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <returns>Cached value or default if not found.</returns>
        T? Get<T>(string key);

        /// <summary>
        /// Sets a value in cache with expiration.
        /// </summary>
        /// <typeparam name="T">Type of the value to cache.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Value to cache.</param>
        /// <param name="expiration">Cache expiration time.</param>
        void Set<T>(string key, T value, TimeSpan expiration);

        /// <summary>
        /// Removes a value from cache.
        /// </summary>
        /// <param name="key">Cache key to remove.</param>
        void Remove(string key);

        /// <summary>
        /// Checks if a key exists in cache.
        /// </summary>
        /// <param name="key">Cache key to check.</param>
        /// <returns>True if key exists, false otherwise.</returns>
        bool Exists(string key);
    }
}
