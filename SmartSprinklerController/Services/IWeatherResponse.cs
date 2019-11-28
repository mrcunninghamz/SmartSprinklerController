namespace SmartSprinklerController.Services
{
    public interface IWeatherResponse
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

}
