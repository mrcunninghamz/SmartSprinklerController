using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Autofac;
using SmartSprinklerController.Models;
using SmartSprinklerController.Services;
using SmartSprinklerController.Services.OpenWeatherService;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartSprinklerController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GpioPin runningPin;
        private DispatcherTimer dispatcherTimer;
        private static IContainer Container { get; set; }
        public MainPage()
        {
            runningPin = Utilities.ConfigureGpioPin(18, GpioPinDriveMode.Output);
            runningPin.Write(GpioPinValue.Low);

            this.SetupDependencies();
            this.InitializeComponent();
            this.ConfigureTimer();
            this.RunSprinklers();

        }

        private void ConfigureTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
        }

        private async void RunSprinklers()
        {
            var zones = GetZones();
            using (var scope = Container.BeginLifetimeScope())
            {
                var weatherService = scope.Resolve<IWeatherService>();
                
                dispatcherTimer.Start();
                var response = await weatherService.GetWeatherAsync();

                if (!response.PossiblePrecipitation)
                {
                    foreach (var sprinklerZone in zones)
                    {
                        var value = Utilities.InvertGpioPinValue(sprinklerZone.Pin);
                        sprinklerZone.Pin.Write(value);

                        //TODO: Get schedule/duration for zone
                        var zoneDuration = 5; //5 seconds
                        sprinklerZone.ActiveDuration(zoneDuration);
                    }
                }

                runningPin.Write(GpioPinValue.Low);
                dispatcherTimer.Stop();
            }
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            GpioPinValue invertedGpioPinValue;

            var currentPinValue = runningPin.Read();

            if (currentPinValue == GpioPinValue.High)
            {
                invertedGpioPinValue = GpioPinValue.Low;
            }
            else
            {
                invertedGpioPinValue = GpioPinValue.High;
            }

            runningPin.Write(invertedGpioPinValue);
        }

        private void SetupDependencies()
        {
            var builder = new ContainerBuilder();

            //Initialize dependencies
            builder.RegisterType<OpenWeatherService>().As<IWeatherService>();

            Container = builder.Build();
        }

        private ICollection<SprinklerZone> GetZones()
        {
            return new List<SprinklerZone>
            {
                new SprinklerZone("1", 17),
                new SprinklerZone("2", 19)
            };
        }
    }
}
