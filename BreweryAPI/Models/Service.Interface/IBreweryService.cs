namespace BreweryAPI.Models.Service.Interface
{
    public interface IBreweryService
    {
        Task<object> GetBreweries(string sortBy = "city", bool descending = false, string search = null);
    }
}
