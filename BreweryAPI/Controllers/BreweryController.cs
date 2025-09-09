using BreweryAPI.Models.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BreweryAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BreweryController : ControllerBase
    {
        private readonly IBreweryService breweryService;
        private readonly ILogger<BreweryController> logger;

        public BreweryController(IBreweryService _breweryService, ILogger<BreweryController> _logger)
        {
            breweryService = _breweryService;
            logger = _logger;
        }

        /// <summary>
        /// Get breweries with optional sorting and search.
        /// </summary>
        /// <param name="sortBy">Sort by 'city' or 'name'.</param>
        /// <param name="descending">Sort descending if true.</param>
        /// <param name="search">Search term for name or city.</param>
        /// <returns>List of breweries.</returns>
        [HttpGet]
        public async Task<IActionResult> GetBrewery(
            [FromQuery] string sortBy = "city",
            [FromQuery] bool descending = false,
            [FromQuery] string search = null)   
        {
            try
            {
                var result = await breweryService.GetBreweries(sortBy, descending, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in GetBrewery");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Autocomplete search for brewery names or cities.
        /// </summary>
        /// <param name="query">Partial name or city to search for.</param>
        /// <param name="limit">Maximum number of suggestions to return.</param>
        /// <returns>List of matching names and/or cities.</returns>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            try
            {
                var breweries = await breweryService.GetBreweries("name", false, query);
                // Expecting breweries to be a list of objects with 'name' and 'city' properties
                var suggestions = ((IEnumerable<dynamic>)breweries)
                    .Select(b => new { name = b.name, city = b.city })
                    .Distinct()
                    .Take(limit)
                    .ToList();

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in Autocomplete");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
