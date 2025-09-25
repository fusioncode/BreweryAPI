using BreweryAPI.Models.Entities;

namespace BreweryAPI.Models.Service.Interface
{
 
    public interface IBrewerySearch
    {
        /// <summary>
        /// Searches breweries based on query criteria.
        /// </summary>
        /// <param name="breweries">List of breweries to search through.</param>
        /// <param name="searchTerm">Search term to filter by.</param>
        /// <returns>Filtered list of breweries.</returns>
        List<BreweryDataModels> SearchBreweries(List<BrewerySource> breweries, string? searchTerm);

        /// <summary>
        /// Sorts breweries based on specified criteria.
        /// </summary>
        /// <param name="breweries">List of breweries to sort.</param>
        /// <param name="sortBy">Field to sort by (name, city, etc.).</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        /// <returns>Sorted list of breweries.</returns>
        List<BreweryDataModels> SortBreweries(List<BreweryDataModels> breweries, string sortBy, bool descending);

        /// <summary>
        /// Transforms brewery source data to display models.
        /// </summary>
        /// <param name="breweries">Source brewery data.</param>
        /// <returns>Transformed brewery display models.</returns>
        List<BreweryDataModels> TransformBreweries(List<BrewerySource> breweries);
    }
}
