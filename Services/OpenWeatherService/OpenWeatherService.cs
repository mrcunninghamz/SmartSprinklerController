using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartSprinklerController.Services;
using SmartSprinklerController.Services.OpenWeatherService;

namespace Services.OpenWeatherService
{
    public class OpenWeatherService : IWeatherService, IDisposable
    {
        private HttpClient _httpClient;
        private string _appid = "158598f4d558094d49e907ff979977e8";

        public OpenWeatherService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<IReadOnlyCollection<IWeatherForecastResponse>> GetWeatherForecastAsync()
        {
            var url = $"http://api.openweathermap.org/data/2.5/forecast?zip=55330,us&units=metric&appid={_appid}";
            var response = await _httpClient.GetAsync(url);
            var weatherResponse = JsonConvert.DeserializeObject<OpenWeatherResponse>(await response.Content.ReadAsStringAsync());

            return weatherResponse.List.Select(x => new WeatherForecastResponse
            {
                Date = Convert.ToDateTime(x.DtTxt),
                TemperatureF = x.Main.Temp,
                Summary = string.Join(", ", x.Weather.Select(s => s.Description))
            }).ToList();
        }

        public async Task<ICurrentWeatherResponse> GetWeatherNowAsync()
        {
            var url = $"http://api.openweathermap.org/data/2.5/forecast?zip=55330,us&units=metric&appid={_appid}";
            var response = await _httpClient.GetAsync(url);
            var weatherResponse =
                JsonConvert.DeserializeObject<OpenWeatherResponse>(await response.Content.ReadAsStringAsync());

            var weatherToday =
                weatherResponse.List.Where(x => Convert.ToDateTime(x.DtTxt).Date == DateTime.UtcNow.Date);

            return new CurrentWeatherResponse
            {
                Temperature = weatherToday.Select(x => x.Main.TempMin).Min(),
                PossiblePrecipitation =
                    weatherToday.SelectMany(x => x.Weather)

                        //Drizzle rain to Heavy shower snow. 
                        //Reference: https://openweathermap.org/weather-conditions
                        .Any(x => x.Id >= 311 && x.Id <= 622)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
    public class WeatherForecastResponse : IWeatherForecastResponse
    {
        public DateTime Date { get; set; }

        public double TemperatureF { get; set; }

        public string Summary { get; set; }
    }
    public class CurrentWeatherResponse : ICurrentWeatherResponse
    {
        public double Temperature { get; set; }
        public bool PossiblePrecipitation { get; set; }
    }
}