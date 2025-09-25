using BreweryAPI.Models.Entities;
using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Logging;

namespace BreweryAPI.Models.Service
{
    /// <summary>
    /// Orchestrates brewery operations using SOLID principles.
    /// Follows SRP - Single responsibility for orchestration.
    /// Follows OCP - Open for extension, closed for modification.
    /// Follows LSP - Uses abstractions that can be substituted.
    /// Follows ISP - Depends only on interfaces it needs.
    /// Follows DIP - Depends on abstractions, not concretions.
    /// </summary>
    public class BreweryService : IBreweryService
    {
        private readonly IBreweryFetcher _breweryFetcher;
        private readonly ICacheProvider _cacheProvider;
        private readonly IFallbackProvider _fallbackProvider;
        private readonly IBrewerySearch _brewerySearch;
        private readonly ILogger<BreweryService> _logger;

        private const string CacheKey = "BreweryData";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

        public BreweryService(
            IBreweryFetcher breweryFetcher,
            ICacheProvider cacheProvider,
            IFallbackProvider fallbackProvider,
            IBrewerySearch brewerySearch,
            ILogger<BreweryService> logger)
        {
            _breweryFetcher = breweryFetcher;
            _cacheProvider = cacheProvider;
            _fallbackProvider = fallbackProvider;
            _brewerySearch = brewerySearch;
            _logger = logger;
        }

        public async Task<object> GetBreweries(string sortBy = "city", bool descending = false, string search = null)
        {
            try
            {
                _logger.LogInformation("Getting breweries with sortBy: {SortBy}, descending: {Descending}, search: {Search}", 
                    sortBy, descending, search);

                // Step 1: Try to get data from cache first
                var cachedBreweries = _cacheProvider.Get<List<BrewerySource>>(CacheKey);
                List<BrewerySource> breweryData;

                if (cachedBreweries != null && cachedBreweries.Any())
                {
                    _logger.LogDebug("Using cached brewery data ({Count} breweries)", cachedBreweries.Count);
                    breweryData = cachedBreweries;
                }
                else
                {
                    // Step 2: Try to fetch from API
                    try
                    {
                        _logger.LogDebug("Cache miss, fetching from API");
                        breweryData = await _breweryFetcher.FetchBreweriesAsync("breweries");
                        
                        if (breweryData.Any())
                        {
                            // Cache the successful API response
                            _cacheProvider.Set(CacheKey, breweryData, CacheExpiration);
                            
                            // Save to fallback storage for future use
                            await _fallbackProvider.SaveFallbackDataAsync(breweryData);
                            
                            _logger.LogInformation("Successfully fetched and cached {Count} breweries from API", breweryData.Count);
                        }
                    }
                    catch (Exception apiEx)
                    {
                        _logger.LogWarning(apiEx, "API fetch failed, attempting to use fallback data");
                        
                        // Step 3: Fallback to stored data if API fails
                        breweryData = await _fallbackProvider.GetFallbackBreweriesAsync();
                        
                        if (breweryData.Any())
                        {
                            _logger.LogInformation("Using fallback data ({Count} breweries)", breweryData.Count);
                        }
                        else
                        {
                            _logger.LogError("No fallback data available and API fetch failed");
                            throw new InvalidOperationException("Unable to retrieve brewery data from any source", apiEx);
                        }
                    }
                }

                // Step 4: Search and filter the data
                var searchResults = _brewerySearch.SearchBreweries(breweryData, search);
                
                // Step 5: Sort the results
                var sortedResults = _brewerySearch.SortBreweries(searchResults, sortBy, descending);

                _logger.LogInformation("Returning {Count} breweries after search and sort operations", sortedResults.Count);
                
                return sortedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetBreweries");
                throw; // Let the controller handle the response
            }
        }
    }
}
