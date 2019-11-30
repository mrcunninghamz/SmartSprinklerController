using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartSprinklerController.Services.Configurator
{
    public interface IConfiguratorService
    {
        Task<ICollection<Zone>> GetZonesAsync();
    }
    public class ConfiguratorService : IConfiguratorService, IDisposable
    {
        private HttpClient _httpClient;

        public ConfiguratorService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task<ICollection<Zone>> GetZonesAsync()
        {
            var response = await _httpClient.GetAsync("zones");
            var zones = JsonConvert.DeserializeObject<List<Zone>>(await response.Content.ReadAsStringAsync());

            return zones;
        }
    }

    public class Zone
    {
        public int Number { get; set; }

        public int Duration { get; set; }
    }
}
