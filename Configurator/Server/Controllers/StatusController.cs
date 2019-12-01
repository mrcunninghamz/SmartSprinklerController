using System.Threading.Tasks;
using Configurator.Server.Hubs;
using Configurator.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Configurator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private IHubContext<SprinklerControllerStatusHub, ISprinklerControllerStatus> _context;

        public StatusController(IHubContext<SprinklerControllerStatusHub, ISprinklerControllerStatus> context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task Post([FromBody] StatusRequest status)
        {
            await _context.Clients.All.StatusUpdateAsync(status.Status, status.Message);
        }
    }
}