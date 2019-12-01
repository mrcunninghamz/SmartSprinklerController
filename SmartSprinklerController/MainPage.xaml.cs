using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Autofac;
using Core;
using Services;
using Services.OpenWeatherService;
using SmartSprinklerController.Models;
using SmartSprinklerController.Services;
using SmartSprinklerController.Services.Configurator;

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
        private Timer dailyScheduler;
        private Timer statusTimer;
        private ICollection<Timer> waterings = new List<Timer>();
        private ICollection<SprinklerZone> zones;
        private static IContainer Container { get; set; }
        public MainPage()
        {
            runningPin = Utilities.ConfigureGpioPin(18, GpioPinDriveMode.Output);
            runningPin.Write(GpioPinValue.Low);
            
            SetupDependencies();
            InitializeComponent();

            ConfigureZones();
            ConfigureTimer();

            InitializeDailyScheduler();
        }

        private async void InitializeDailyScheduler()
        {
            if (dailyScheduler != null)
            {
                return;
            }

            await ReportStatus(Status.Started, "Checking Schedule.");

            runningPin.Write(GpioPinValue.High);
            UpdateTimer();
            var callback = new TimerCallback(async arg =>
            {
                Debug.WriteLine("Running schedule, checking weather... ");

                await ReportStatus(Status.Started, "Checking Weather.");
                var weather = await GetWeatherAsync();

                Debug.WriteLine($"Weather: Will it rain? {weather.PossiblePrecipitation}, temperature: {weather.Temperature} *F");

                if (!weather.PossiblePrecipitation)
                {
                    GetSchedule();
                }
                else
                {
                    await ReportStatus(Status.Stopped, "Rain in forecast.");
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    runningPin.Write(GpioPinValue.Low);
                    UpdateTimer();
                });
            });
            dailyScheduler = new Timer(callback, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromDays(1));
        }

        private void ConfigureTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
        }

        private async Task<ICurrentWeatherResponse> GetWeatherAsync()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var weatherService = scope.Resolve<IWeatherService>();

                var weatherResponse = await weatherService.GetWeatherNowAsync();
                return weatherResponse;
            }
        }

        private async Task ReportStatus(Status status, string message)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var statusService = scope.Resolve<IStatusService>();

                statusTimer?.Dispose();

                var callback = new TimerCallback(async arg =>
                {
                    await statusService.ReportStatus(status, message);
                });

                statusTimer = new Timer(callback, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromSeconds(5));
            }
        }

        private async void RunSprinklers()
        {
            await ReportStatus(Status.Running, "Running Sprinklers.");
            Debug.WriteLine($"Current Time: {DateTime.UtcNow.TimeOfDay}");

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                runningPin.Write(GpioPinValue.High);
                UpdateTimer();
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
            });

            foreach (var sprinklerZone in zones)
            {
                await ReportStatus(Status.Running, $"Running Sprinklers. Zone {sprinklerZone.Number}");
                Debug.WriteLine($"Running zone {sprinklerZone.Number} for {sprinklerZone.Duration} seconds");
                sprinklerZone.RunZone();
            }

            await ReportStatus(Status.Stopped, "Finished Run.");
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                runningPin.Write(GpioPinValue.Low);
                UpdateTimer();
            });
        }
        private void UpdateTimer()
        {
            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }
            else
            {
                dispatcherTimer.Start();
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
            builder.RegisterType<ConfiguratorService>().As<IConfiguratorService>();
            builder.RegisterType<StatusService>().As<IStatusService>();

            Container = builder.Build();
        }

        private async void ConfigureZones()
        {
            await ReportStatus(Status.Started, "Configuring Zones.");

            var sprinklerZones = new List<SprinklerZone>
            {
                new SprinklerZone(1, 17),
                new SprinklerZone(2, 19)
            };

            using (var scope = Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IConfiguratorService>();

                var zoneConfigurations = await service.GetZonesAsync();

                foreach (var zoneConfiguration in zoneConfigurations)
                {
                    var sprinklerZone = sprinklerZones.FirstOrDefault(x => x.Number == zoneConfiguration.Number);

                    sprinklerZone?.SetDuration(zoneConfiguration.Duration);
                }
            }

            zones = sprinklerZones;

            await ReportStatus(Status.Started, "Finished Configuring Zones.");
        }

        private async void GetSchedule()
        {
            await ReportStatus(Status.GettingSchedule, "Its Raining.");

            //TODO call service for schedule for today
            var today = DateTime.UtcNow;

            var schedule = new List<Schedule>
            {
                // will happen in 5 seconds with a duration of 10 seconds
                new Schedule
                {
                    Day = today.DayOfWeek,
                    Time = today.AddSeconds(5).TimeOfDay, // will start 5 seconds from now
                    Zones = {1, 2}
                },

                // will happen 5 seconds after the first and last for 10 seconds
                new Schedule
                {
                    Day = today.DayOfWeek,
                    Time = today.AddSeconds(20).TimeOfDay, // will start 5 seconds from now
                    Zones = {1, 2}
                }
            };

            foreach (var scheduleItem in schedule)
            {
                var timmerCallback = new TimerCallback(arg =>
                {
                    var sprinklerAction = new Action(RunSprinklers);
                    var sprinklerTask = new Task(sprinklerAction);
                    sprinklerTask.Start();
                });

                var time = DateTime.UtcNow.TimeOfDay;
                var timeTilTask = scheduleItem.Time.Subtract(time);

                Debug.WriteLine($"Current Time: {time}");
                Debug.WriteLine($"Creating timer for runtime: {scheduleItem.Time} on day: {scheduleItem.Day}");
                Debug.WriteLine($"Timer will run in {timeTilTask.Seconds}");
                var timer = new Timer(timmerCallback, null, timeTilTask, TimeSpan.FromMilliseconds(-1));
                waterings.Add(timer);
            }
        }
    }
}
