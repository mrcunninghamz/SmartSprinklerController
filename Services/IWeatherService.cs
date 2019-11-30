using System.Collections.Generic;
using System.Threading.Tasks;
using Services;

namespace SmartSprinklerController.Services
{
    public interface IWeatherService
    {
        Task<IReadOnlyCollection<IWeatherForecastResponse>> GetWeatherForecastAsync();
        Task<ICurrentWeatherResponse> GetWeatherNowAsync();
    }
}