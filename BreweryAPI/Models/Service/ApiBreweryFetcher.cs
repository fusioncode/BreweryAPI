using BreweryAPI.Models.Entities;
using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BreweryAPI.Models.Service
{
    /// <summary>
    /// Handles fetching brewery data from external API.
    /// Follows SRP - Single responsibility for API data fetching.
    /// Follows OCP - Open for extension (can be extended for different APIs).
    /// Follows LSP - Can substitute any IBreweryFetcher implementation.
    /// </summary>
    public class ApiBreweryFetcher : IBreweryFetcher
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiBreweryFetcher> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public ApiBreweryFetcher(
            IConfiguration configuration, 
            ILogger<ApiBreweryFetcher> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _apiBaseUrl = _configuration.GetValue<string>("OpenBreweryConfig:BaseAPIUrl")
                ?? "https://api.openbrewerydb.org/v1/breweries";
        }

        public async Task<List<BrewerySource>> FetchBreweriesAsync(string filter = "")
        {
            try
            {
                _logger.LogInformation("Fetching brewery data from API with filter: {Filter}", filter);
                
                var apiUrl = string.IsNullOrWhiteSpace(filter) ? _apiBaseUrl : $"{_apiBaseUrl}/{filter}";
                
                var response = await _httpClient.GetStringAsync(apiUrl);
                
                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogWarning("Empty response received from API");
                    return new List<BrewerySource>();
                }

                var breweries = JsonConvert.DeserializeObject<List<BrewerySource>>(response);
                
                _logger.LogInformation("Successfully fetched {Count} breweries from API", breweries?.Count ?? 0);
                
                return breweries ?? new List<BrewerySource>();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error occurred while fetching brewery data from API");
                throw new InvalidOperationException("Failed to fetch data from brewery API", httpEx);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error occurred while processing brewery data");
                throw new InvalidOperationException("Failed to parse brewery data from API", jsonEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching brewery data");
                throw new InvalidOperationException("Unexpected error occurred while fetching brewery data", ex);
            }
        }
    }
}
