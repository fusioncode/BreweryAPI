using BreweryAPI.Models.Entities;
using BreweryAPI.Models.Service.Interface;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BreweryAPI.Models.Service
{
    public class BreweryService : IBreweryService
    {
        private readonly IBrewerySourceAPI sourceAPI;
        private readonly ILogger<BreweryService> logger;

        public BreweryService(IBrewerySourceAPI _sourceAPI, ILogger<BreweryService> _logger)
        {
            sourceAPI = _sourceAPI;
            logger = _logger;
        }

        public async Task<object> GetBreweries(string sortBy = "city", bool descending = false, string search = null)
        {
            try
            {
                var filter = "breweries";
                var breweries = await sourceAPI.GetBrewery(filter);

                var breweryList = JsonConvert.DeserializeObject<List<BrewerySource>>(breweries);

                var result = breweryList
                    .Select(b => new BreweryDataModels
                    {
                        name = b.name,
                        city = b.city,
                        phone = b.phone
                    })
                    .AsQueryable();

                // Search logic
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var lowered = search.ToLower();
                    result = result.Where(b =>
                        (!string.IsNullOrEmpty(b.name) && b.name.ToLower().Contains(lowered)) ||
                        (!string.IsNullOrEmpty(b.city) && b.city.ToLower().Contains(lowered))
                    );
                }

                // Sorting logic
                result = sortBy.ToLower() switch
                {
                    "name" => descending ? result.OrderByDescending(b => b.name) : result.OrderBy(b => b.name),
                    _ => descending ? result.OrderByDescending(b => b.city) : result.OrderBy(b => b.city)
                };

                return result.ToList();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error occurred in GetBreweries");
                throw; // Let the controller handle the response
            }
        }
    }
}
