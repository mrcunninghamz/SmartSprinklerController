using System.Threading.Tasks;

namespace SmartSprinklerController.Services
{
    public interface IWeatherService
    {
        Task<IWeatherResponse> GetWeatherAsync();
    }
}