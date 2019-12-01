using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Configurator.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //BuildWebHost(args).Run();
            CreateHostBuilder(args).Build().Run();
        }

        //        public static IWebHost BuildWebHost(string[] args) =>
        //            WebHost.CreateDefaultBuilder(args)
        //                .UseConfiguration(new ConfigurationBuilder()
        //                    .AddCommandLine(args)
        //                    .Build())
        //                .UseStartup<Startup>()
        //                .Build();
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:5000/");
                });
    }
}
