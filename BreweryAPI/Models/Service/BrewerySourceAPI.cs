using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BreweryAPI.Models.Service
{
    public class BrewerySourceAPI : IBrewerySourceAPI
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<BrewerySourceAPI> logger;
        private string apiBaseURl = string.Empty;
        private const string CacheKey = "BreweryApiResponse";
        private const string FilePath = "response.json";

        public BrewerySourceAPI(IConfiguration _configuration, IMemoryCache _memoryCache, ILogger<BrewerySourceAPI> _logger)
        {
            configuration = _configuration;
            memoryCache = _memoryCache;
            logger = _logger;
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            apiBaseURl = configuration.GetValue<string>("OpenBreweryConfig:BaseAPIUrl")
                ?? "https://api.openbrewerydb.org/v1/breweries";
        }

        public async Task<string> GetBrewery(string filter = "")
        {
            if (!memoryCache.TryGetValue(CacheKey, out string response))
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var apiUrl = string.IsNullOrWhiteSpace(filter) ? apiBaseURl : $"{apiBaseURl}/{filter}";
                        response = await client.GetStringAsync(apiUrl);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching from API, falling back to file.");
                    // If API call fails, read from file
                    try
                    {
                        if (File.Exists(FilePath))
                        {
                            response = File.ReadAllText(FilePath);
                        }
                        else
                        {
                            response = string.Empty;
                        }
                    }
                    catch (Exception fileEx)
                    {
                        logger.LogError(fileEx, "Error reading fallback file.");
                        response = string.Empty;
                    }
                }

                memoryCache.Set(CacheKey, response, TimeSpan.FromMinutes(10));
            }
            return response;
        }
    }
}
