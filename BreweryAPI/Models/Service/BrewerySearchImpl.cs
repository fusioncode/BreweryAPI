using BreweryAPI.Models.Entities;
using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Logging;

namespace BreweryAPI.Models.Service
{
    /// <summary>
    /// Handles brewery search and sorting operations.
    /// Follows SRP - Single responsibility for search/sort logic.
    /// Follows OCP - Open for extension (can be extended for different search algorithms).
    /// Follows LSP - Can substitute any IBrewerySearch implementation.
    /// </summary>
    public class BrewerySearchImpl : IBrewerySearch
    {
        private readonly ILogger<BrewerySearchImpl> _logger;

        public BrewerySearchImpl(ILogger<BrewerySearchImpl> logger)
        {
            _logger = logger;
        }

        public List<BreweryDataModels> SearchBreweries(List<BrewerySource> breweries, string? searchTerm)
        {
            try
            {
                if (breweries == null || !breweries.Any())
                {
                    _logger.LogDebug("No breweries provided for search");
                    return new List<BreweryDataModels>();
                }

                // Transform first, then search
                var transformedBreweries = TransformBreweries(breweries);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _logger.LogDebug("No search term provided, returning all {Count} breweries", transformedBreweries.Count);
                    return transformedBreweries;
                }

                var loweredSearchTerm = searchTerm.ToLower().Trim();
                _logger.LogDebug("Searching breweries with term: '{SearchTerm}'", searchTerm);

                var filteredBreweries = transformedBreweries.Where(brewery =>
                    (!string.IsNullOrEmpty(brewery.name) && brewery.name.ToLower().Contains(loweredSearchTerm)) ||
                    (!string.IsNullOrEmpty(brewery.city) && brewery.city.ToLower().Contains(loweredSearchTerm)) ||
                    (!string.IsNullOrEmpty(brewery.phone) && brewery.phone.ToLower().Contains(loweredSearchTerm))
                ).ToList();

                _logger.LogDebug("Search returned {Count} breweries for term: '{SearchTerm}'", filteredBreweries.Count, searchTerm);
                
                return filteredBreweries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching breweries with term: '{SearchTerm}'", searchTerm);
                return new List<BreweryDataModels>();
            }
        }

        public List<BreweryDataModels> SortBreweries(List<BreweryDataModels> breweries, string sortBy, bool descending)
        {
            try
            {
                if (breweries == null || !breweries.Any())
                {
                    _logger.LogDebug("No breweries provided for sorting");
                    return new List<BreweryDataModels>();
                }

                _logger.LogDebug("Sorting {Count} breweries by '{SortBy}', descending: {Descending}", 
                    breweries.Count, sortBy, descending);

                var sortedBreweries = sortBy.ToLower().Trim() switch
                {
                    "name" => descending 
                        ? breweries.OrderByDescending(b => b.name ?? string.Empty).ToList()
                        : breweries.OrderBy(b => b.name ?? string.Empty).ToList(),
                    "phone" => descending 
                        ? breweries.OrderByDescending(b => b.phone ?? string.Empty).ToList()
                        : breweries.OrderBy(b => b.phone ?? string.Empty).ToList(),
                    "city" or _ => descending 
                        ? breweries.OrderByDescending(b => b.city ?? string.Empty).ToList()
                        : breweries.OrderBy(b => b.city ?? string.Empty).ToList()
                };

                _logger.LogDebug("Successfully sorted breweries by '{SortBy}'", sortBy);
                
                return sortedBreweries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sorting breweries by '{SortBy}'", sortBy);
                return breweries; // Return original list if sorting fails
            }
        }

        public List<BreweryDataModels> TransformBreweries(List<BrewerySource> breweries)
        {
            try
            {
                if (breweries == null || !breweries.Any())
                {
                    _logger.LogDebug("No breweries provided for transformation");
                    return new List<BreweryDataModels>();
                }

                _logger.LogDebug("Transforming {Count} brewery source objects to display models", breweries.Count);

                var transformedBreweries = breweries.Select(source => new BreweryDataModels
                {
                    name = source.name ?? string.Empty,
                    city = source.city ?? string.Empty,
                    phone = source.phone ?? string.Empty
                }).ToList();

                _logger.LogDebug("Successfully transformed {Count} breweries", transformedBreweries.Count);
                
                return transformedBreweries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while transforming brewery data");
                return new List<BreweryDataModels>();
            }
        }
    }
}
