using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core;
using Newtonsoft.Json;

namespace SmartSprinklerController.Services.Configurator
{
    public interface IStatusService
    {
        Task ReportStatus(Status status, string message);
    }
    public class StatusService : IStatusService
    {
        private HttpClient _httpClient;

        public StatusService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5000/api/");
        }

        public async Task ReportStatus(Status status, string message)
        {
            var json = JsonConvert.SerializeObject(new {Status = status, Message = message});
            await _httpClient.PostAsync("status", new StringContent(json, Encoding.UTF8, "application/json"));
        }
    }
}