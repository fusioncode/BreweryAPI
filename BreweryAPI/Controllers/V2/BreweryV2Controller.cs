using BreweryAPI.Models.DTOs.V2;
using BreweryAPI.Models.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

namespace BreweryAPI.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/Brewery")]
    [Authorize]
    public class BreweryV2Controller : ControllerBase
    {
        private readonly IBreweryService breweryService;
        private readonly ILogger<BreweryV2Controller> logger;

        public BreweryV2Controller(IBreweryService _breweryService, ILogger<BreweryV2Controller> _logger)
        {
            breweryService = _breweryService;
            logger = _logger;
        }

        /// <summary>
        /// Get breweries with advanced filtering, pagination, and enhanced data (V2).
        /// </summary>
        /// <param name="request">Advanced search request with filters and pagination</param>
        /// <returns>Paginated list of breweries with enhanced metadata</returns>
        [HttpPost("search")]
        public async Task<IActionResult> SearchBreweries([FromBody] BrewerySearchV2RequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Get breweries using the existing service
                var breweries = await breweryService.GetBreweries(request.SortBy, request.Descending, request.Search);
                var breweryList = ((IEnumerable<dynamic>)breweries).ToList();

                // Apply additional V2 filters
                var filteredBreweries = ApplyV2Filters(breweryList, request);

                // Apply pagination
                var totalItems = filteredBreweries.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
                var skip = (request.Page - 1) * request.PageSize;
                var pagedBreweries = filteredBreweries.Skip(skip).Take(request.PageSize).ToList();

                // Convert to V2 DTOs with enhanced data
                var breweryV2Dtos = new List<BreweryV2Dto>();
                foreach (var brewery in pagedBreweries)
                {
                    breweryV2Dtos.Add(ConvertToV2Dto(brewery, request));
                }


                stopwatch.Stop();

                var response = new BrewerySearchV2ResponseDto
                {
                    Breweries = breweryV2Dtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = request.Page,
                        PageSize = request.PageSize,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        HasNextPage = request.Page < totalPages,
                        HasPreviousPage = request.Page > 1
                    },
                    Metadata = new SearchMetadata
                    {
                        SearchTerm = request.Search,
                        SortBy = request.SortBy,
                        Descending = request.Descending,
                        ResultCount = breweryV2Dtos.Count,
                        SearchTimeMs = stopwatch.Elapsed.TotalMilliseconds,
                        AppliedFilters = GetAppliedFilters(request)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in SearchBreweries V2");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Get breweries with basic filtering (backwards compatible with V1).
        /// </summary>
        /// <param name="sortBy">Sort by 'city' or 'name'.</param>
        /// <param name="descending">Sort descending if true.</param>
        /// <param name="search">Search term for name or city.</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>List of breweries with enhanced V2 data.</returns>
        [HttpGet]
        public async Task<IActionResult> GetBreweries(
            [FromQuery] string sortBy = "city",
            [FromQuery] bool descending = false,
            [FromQuery] string search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var request = new BrewerySearchV2RequestDto
            {
                SortBy = sortBy,
                Descending = descending,
                Search = search,
                Page = page,
                PageSize = pageSize
            };

            return await SearchBreweries(request);
        }

        /// <summary>
        /// Enhanced autocomplete with additional metadata and filtering.
        /// </summary>
        /// <param name="query">Partial name or city to search for.</param>
        /// <param name="limit">Maximum number of suggestions to return.</param>
        /// <param name="includeMetadata">Include additional metadata in response.</param>
        /// <returns>List of matching names and/or cities with enhanced data.</returns>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete(
            [FromQuery] string query,
            [FromQuery] int limit = 10,
            [FromQuery] bool includeMetadata = false)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            try
            {
                var breweries = await breweryService.GetBreweries("name", false, query);
                var breweryList = ((IEnumerable<dynamic>)breweries).ToList();

                var suggestions = breweryList
                    .Select(b => new
                    {
                        name = b.name?.ToString(),
                        city = b.city?.ToString(),
                        state = b.state?.ToString(),
                        brewery_type = b.brewery_type?.ToString(),
                        // V2 enhancements
                        id = b.id?.ToString(),
                        full_address = $"{b.name}, {b.city}, {b.state}".Trim(' ', ','),
                        metadata = includeMetadata ? new
                        {
                            phone = b.phone?.ToString(),
                            website_url = b.website_url?.ToString(),
                            latitude = b.latitude?.ToString(),
                            longitude = b.longitude?.ToString()
                        } : null
                    })
                    .Where(s => !string.IsNullOrEmpty(s.name))
                    .Distinct()
                    .Take(limit)
                    .ToList();

                return Ok(new
                {
                    suggestions = suggestions,
                    total_count = suggestions.Count,
                    query = query,
                    api_version = "2.0"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in Autocomplete V2");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Get brewery by ID with enhanced V2 data.
        /// </summary>
        /// <param name="id">Brewery ID</param>
        /// <returns>Brewery details with enhanced data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBreweryById(string id)
        {
            try
            {
                var breweries = await breweryService.GetBreweries("name", false, null);
                var breweryList = ((IEnumerable<dynamic>)breweries).ToList();
                var brewery = breweryList.FirstOrDefault(b => b.id?.ToString() == id);

                if (brewery == null)
                {
                    return NotFound($"Brewery with ID '{id}' not found.");
                }

                var breweryV2Dto = ConvertToV2Dto(brewery, new BrewerySearchV2RequestDto());

                return Ok(breweryV2Dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in GetBreweryById V2");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private List<dynamic> ApplyV2Filters(List<dynamic> breweries, BrewerySearchV2RequestDto request)
        {
            var filtered = breweries.AsEnumerable();

            if (!string.IsNullOrEmpty(request.BreweryType))
            {
                filtered = filtered.Where(b => b.brewery_type?.ToString()?.Equals(request.BreweryType, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(request.City))
            {
                filtered = filtered.Where(b => b.city?.ToString()?.Contains(request.City, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrEmpty(request.State))
            {
                filtered = filtered.Where(b => b.state?.ToString()?.Equals(request.State, StringComparison.OrdinalIgnoreCase) == true);
            }

            return filtered.ToList();
        }

        private BreweryV2Dto ConvertToV2Dto(dynamic brewery, BrewerySearchV2RequestDto request)
        {
            var dto = new BreweryV2Dto
            {
                //Id = brewery.id?.ToString(),
                Name = brewery.name?.ToString(),
                //BreweryType = brewery.brewery_type?.ToString(),
                //Address1 = brewery.address_1?.ToString(),
                //Address2 = brewery.address_2?.ToString(),
                //Address3 = brewery.address_3?.ToString(),
                City = brewery.city?.ToString(),
               // StateProvince = brewery.state_province?.ToString(),
                //PostalCode = brewery.postal_code?.ToString(),
                //Country = brewery.country?.ToString(),
                //Longitude = brewery.longitude?.ToString(),
                //Latitude = brewery.latitude?.ToString(),
                Phone = brewery.phone?.ToString(),
                //WebsiteUrl = brewery.website_url?.ToString(),
                //State = brewery.state?.ToString(),
                //Street = brewery.street?.ToString(),
                
                // V2 Enhanced Properties
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365)), // Mock data
                UpdatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
                Rating = Math.Round(Random.Shared.NextDouble() * 4 + 1, 1), // 1.0 - 5.0
                ReviewCount = Random.Shared.Next(0, 500),
                IsFavorite = Random.Shared.NextDouble() > 0.8, // 20% chance
                Tags = GenerateRandomTags()
            };

            // Calculate distance if coordinates are provided
            if (request.Latitude.HasValue && request.Longitude.HasValue &&
                double.TryParse(dto.Latitude, out var lat) && double.TryParse(dto.Longitude, out var lng))
            {
                var distanceKm = CalculateDistance(request.Latitude.Value, request.Longitude.Value, lat, lng);
                dto.Distance = new DistanceInfo
                {
                    DistanceInKm = Math.Round(distanceKm, 2),
                    DistanceInMiles = Math.Round(distanceKm * 0.621371, 2),
                    FormattedDistance = distanceKm < 1 ? $"{Math.Round(distanceKm * 1000)}m" : $"{Math.Round(distanceKm, 1)}km"
                };
            }

            return dto;
        }

        private List<string> GenerateRandomTags()
        {
            var allTags = new[] { "craft", "local", "family-owned", "dog-friendly", "outdoor-seating", "live-music", "food-truck", "tours", "tasting-room", "historic" };
            var tagCount = Random.Shared.Next(0, 4);
            return allTags.OrderBy(x => Random.Shared.Next()).Take(tagCount).ToList();
        }

        private List<string> GetAppliedFilters(BrewerySearchV2RequestDto request)
        {
            var filters = new List<string>();
            
            if (!string.IsNullOrEmpty(request.Search)) filters.Add($"search:{request.Search}");
            if (!string.IsNullOrEmpty(request.BreweryType)) filters.Add($"type:{request.BreweryType}");
            if (!string.IsNullOrEmpty(request.City)) filters.Add($"city:{request.City}");
            if (!string.IsNullOrEmpty(request.State)) filters.Add($"state:{request.State}");
            if (request.MinRating.HasValue) filters.Add($"min_rating:{request.MinRating}");
            if (request.RadiusInKm.HasValue) filters.Add($"radius:{request.RadiusInKm}km");
            
            return filters;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
