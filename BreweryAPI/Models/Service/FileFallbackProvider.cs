using BreweryAPI.Models.Entities;
using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BreweryAPI.Models.Service
{

    public class FileFallbackProvider : IFallbackProvider
    {
        private readonly ILogger<FileFallbackProvider> _logger;
        private readonly string _fallbackFilePath;

        public FileFallbackProvider(ILogger<FileFallbackProvider> logger)
        {
            _logger = logger;
            _fallbackFilePath = Path.Combine(Directory.GetCurrentDirectory(), "response.json");
        }

        public async Task<List<BrewerySource>> GetFallbackBreweriesAsync()
        {
            try
            {
                if (!await HasFallbackDataAsync())
                {
                    _logger.LogWarning("No fallback data available at path: {Path}", _fallbackFilePath);
                    return new List<BrewerySource>();
                }

                _logger.LogInformation("Reading fallback brewery data from file: {Path}", _fallbackFilePath);
                
                var jsonContent = await File.ReadAllTextAsync(_fallbackFilePath);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("Fallback file is empty: {Path}", _fallbackFilePath);
                    return new List<BrewerySource>();
                }

                var breweries = JsonConvert.DeserializeObject<List<BrewerySource>>(jsonContent);
                
                _logger.LogInformation("Successfully loaded {Count} breweries from fallback file", breweries?.Count ?? 0);
                
                return breweries ?? new List<BrewerySource>();
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("Fallback file not found: {Path}", _fallbackFilePath);
                return new List<BrewerySource>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error occurred while reading fallback data");
                return new List<BrewerySource>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while reading fallback data from: {Path}", _fallbackFilePath);
                return new List<BrewerySource>();
            }
        }

        public async Task SaveFallbackDataAsync(List<BrewerySource> breweries)
        {
            try
            {
                if (breweries == null || !breweries.Any())
                {
                    _logger.LogWarning("Attempted to save null or empty brewery data to fallback file");
                    return;
                }

                _logger.LogInformation("Saving {Count} breweries to fallback file: {Path}", breweries.Count, _fallbackFilePath);
                
                var jsonContent = JsonConvert.SerializeObject(breweries, Formatting.Indented);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_fallbackFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(_fallbackFilePath, jsonContent);
                
                _logger.LogInformation("Successfully saved fallback data to file: {Path}", _fallbackFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied while saving fallback data to: {Path}", _fallbackFilePath);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "Directory not found while saving fallback data to: {Path}", _fallbackFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while saving fallback data to: {Path}", _fallbackFilePath);
            }
        }

        public async Task<bool> HasFallbackDataAsync()
        {
            try
            {
                if (!File.Exists(_fallbackFilePath))
                {
                    return false;
                }

                var fileInfo = new FileInfo(_fallbackFilePath);
                
                // Check if file exists and has content
                if (fileInfo.Length == 0)
                {
                    _logger.LogDebug("Fallback file exists but is empty: {Path}", _fallbackFilePath);
                    return false;
                }


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking fallback data availability: {Path}", _fallbackFilePath);
                return false;
            }
        }
    }
}
