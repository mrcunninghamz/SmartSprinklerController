using System;
using System.Collections.Generic;
using System.Text;
using Services;

namespace Configurator.Shared
{
    public class WeatherForecast : IWeatherForecastResponse
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public double TemperatureF { get; set; }
    }
}
