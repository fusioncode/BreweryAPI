using BreweryAPI.Models.Entities;

namespace BreweryAPI.Models.Service.Interface
{

    public interface IBreweryFetcher
    {
        /// <summary>
        /// fetch data
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<List<BrewerySource>> FetchBreweriesAsync(string filter = "");
    }
}
