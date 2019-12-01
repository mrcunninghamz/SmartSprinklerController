using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Configurator.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Configurator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZonesController : ControllerBase
    {

        [HttpGet]
        public async Task<List<Zone>> GetZones()
        {
            var executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(executableLocation, "Data/zones.json");

            var json = await System.IO.File.ReadAllTextAsync(filePath);
            
            return JsonConvert.DeserializeObject<List<Zone>>(json);
        }
    }
}