using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.SignalR;

namespace Configurator.Server.Hubs
{
    public class SprinklerControllerStatusHub : Hub<ISprinklerControllerStatus>
    {
        public async Task SendMessage(Status status, string message)
        {
            await Clients.All.StatusUpdateAsync(status, message);
        }
    }

    public interface ISprinklerControllerStatus
    {
        Task StatusUpdateAsync(Status status, string message);
    }
}