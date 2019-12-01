using System;
using System.Threading.Tasks;
using Blazor.Extensions;
using Microsoft.AspNetCore.Components;

namespace Configurator.Client.Pages
{
    public class StatusComponent : ComponentBase
    {
        internal  string StatusMessage { get; set; }
        [Inject] private HubConnectionBuilder _hubConnectionBuilder { get; set; }
        private HubConnection connection;

        protected override async Task OnInitializedAsync()
        {

            Console.WriteLine("initializing hub connection");
            StatusMessage = "Test";
            this.connection = this._hubConnectionBuilder
                .WithUrl("/hubs/status",
                    opt =>
                    {
                        opt.LogLevel = SignalRLogLevel.None;
                        opt.Transport = HttpTransportType.WebSockets;
                    })
                .Build();

            this.connection.On<Core.Status, string>("StatusUpdateAsync", this.Handle);
            this.connection.OnClose(exc =>
            {
                Console.WriteLine("Connection was closed! " + exc.ToString());
                return Task.CompletedTask;
            });
            await this.connection.StartAsync();
        }
        private Task Handle(Core.Status status, string message)
        {
            StatusMessage = message;
            this.StateHasChanged();
            return Task.CompletedTask;
        }
    }
}