using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartSprinklerController.Services.OpenWeatherService
{
    public class OpenWeatherService : IWeatherService
    {

        public async Task<IWeatherResponse> GetWeatherAsync()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(
                "http://api.openweathermap.org/data/2.5/forecast?zip=55330,us&units=metric&appid=158598f4d558094d49e907ff979977e8");
            var weatherResponse =
                JsonConvert.DeserializeObject<OpenWeatherResponse>(await response.Content.ReadAsStringAsync());

            var weatherToday =
                weatherResponse.List.Where(x => Utilities.FromUnixTime(x.Dt).Date == DateTime.UtcNow.Date);

            return new WeatherResponse
            {
                Temperature = weatherToday.Select(x => x.Main.TempMin).Min(),
                PossiblePrecipitation =
                    weatherToday.SelectMany(x => x.Weather)

                        //Drizzle rain to Heavy shower snow. 
                        //Reference: https://openweathermap.org/weather-conditions
                        .Any(x => x.Id >= 311 && x.Id <= 622)
            };
        }
    }

    public class WeatherResponse : IWeatherResponse
    {
        public double Temperature { get; set; }
        public bool PossiblePrecipitation { get; set; }
    }
}