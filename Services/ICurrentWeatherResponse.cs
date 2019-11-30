using System;

namespace Services
{
    public interface ICurrentWeatherResponse
    {
        /// <summary>
        ///     Temperature in Fahrenheit
        /// </summary>
        double Temperature { get; set; }

        /// <summary>
        ///     Whether or not the forecast shows rain
        /// </summary>
        bool PossiblePrecipitation { get; set; }
    }

    public interface IWeatherForecastResponse
    {
        DateTime Date { get; set; }

        double TemperatureF { get; set; }

        string Summary { get; set; }
    }

}
