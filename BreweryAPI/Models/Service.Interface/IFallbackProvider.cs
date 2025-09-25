using BreweryAPI.Models.Entities;

namespace BreweryAPI.Models.Service.Interface
{
    
    public interface IFallbackProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<BrewerySource>> GetFallbackBreweriesAsync();

      
        Task SaveFallbackDataAsync(List<BrewerySource> breweries);

      
        Task<bool> HasFallbackDataAsync();
    }
}
