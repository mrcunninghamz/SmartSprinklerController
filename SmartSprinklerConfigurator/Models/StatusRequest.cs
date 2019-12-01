using Core;

namespace SmartSprinklerConfigurator.Models
{
    public class StatusRequest
    {
        public Status Status { get; set; }

        public string Message { get; set; }
    }
}