using Configurator.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using SmartSprinklerController.Services;

namespace Configurator.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        
        private readonly ILogger<WeatherForecastController> logger;
        private readonly IWeatherService _forecastService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherService forecastService)
        {
            this.logger = logger;
            _forecastService = forecastService;
        }

        [HttpGet]
        public async Task<IEnumerable<IWeatherForecastResponse>> Get()
        {
            return await _forecastService.GetWeatherForecastAsync();
        }
    }
}
