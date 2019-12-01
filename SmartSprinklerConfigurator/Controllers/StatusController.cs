using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartSprinklerConfigurator.Hubs;
using SmartSprinklerConfigurator.Models;

namespace SmartSprinklerConfigurator.Controllers
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