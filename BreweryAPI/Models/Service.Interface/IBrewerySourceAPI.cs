namespace BreweryAPI.Models.Service.Interface
{
    public interface IBrewerySourceAPI
    {
        Task<string> GetBrewery(string filter = "");
    }
}
