using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
        private Timer dailyScheduler;
        private ICollection<Timer> waterings = new List<Timer>();
        private ICollection<SprinklerZone> zones;
        private static IContainer Container { get; set; }
        public MainPage()
        {
            runningPin = Utilities.ConfigureGpioPin(18, GpioPinDriveMode.Output);
            runningPin.Write(GpioPinValue.Low);
            zones = GetZones();

            this.SetupDependencies();
            this.InitializeComponent();

            this.ConfigureTimer();

            InitializeDailyScheduler();
        }

        private void InitializeDailyScheduler()
        {
            if (dailyScheduler != null)
            {
                return;
            }
            else
            {
                runningPin.Write(GpioPinValue.High);
                this.UpdateTimer();
                var callback = new TimerCallback(async (arg) =>
                {
                    Debug.WriteLine("Running schedule, checking weather... ");
                    var weather = await this.GetWeatherAsync();

                    Debug.WriteLine($"Weather: Will it rain? {weather.PossiblePrecipitation}, temperature: {weather.Temperature} *F");
                    GetSchedule(weather);

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        runningPin.Write(GpioPinValue.Low);
                        this.UpdateTimer();
                    });
                });
                dailyScheduler = new Timer(callback, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromDays(1));
            }
        }

        private void ConfigureTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
        }

        private async Task<IWeatherResponse> GetWeatherAsync()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var weatherService = scope.Resolve<IWeatherService>();

                var weatherResponse = await weatherService.GetWeatherAsync();
                return weatherResponse;
            }
        }

        private async void RunSprinklers(IWeatherResponse weatherResponse, Schedule schedule)
        {
            if (weatherResponse != null && !weatherResponse.PossiblePrecipitation)
            {

                Debug.WriteLine($"Current Time: {DateTime.UtcNow.TimeOfDay}");
                Debug.WriteLine($"Running timer for runtime: {schedule.Time} on day: {schedule.Day}");

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    runningPin.Write(GpioPinValue.High);
                    this.UpdateTimer();
                    this.dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1000);
                });

                foreach (var sprinklerZone in zones)
                {
                    var value = Utilities.InvertGpioPinValue(sprinklerZone.Pin);
                    sprinklerZone.Pin.Write(value);


                    Debug.WriteLine($"Running zone {sprinklerZone.Number} for {schedule.ZoneDurations[sprinklerZone.Number]} seconds");
                    sprinklerZone.ActiveDuration(schedule.ZoneDurations[sprinklerZone.Number]);
                }
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                runningPin.Write(GpioPinValue.Low);
                this.UpdateTimer();
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

            Container = builder.Build();
        }

        private ICollection<SprinklerZone> GetZones()
        {
            //TODO call service for zone information
            return new List<SprinklerZone>
            {
                new SprinklerZone(1, 17),
                new SprinklerZone(2, 19)
            };
        }

        private void GetSchedule(IWeatherResponse weather)
        {
            //TODO call service for schedule for today
            var today = DateTime.UtcNow;

            var schedule = new List<Schedule>
            {
                // will happen in 5 seconds with a duration of 15 seconds
                new Schedule
                {
                    Day = today.DayOfWeek,
                    Time = today.AddSeconds(5).TimeOfDay, // will start 5 seconds from now
                    ZoneDurations = new Dictionary<int, int>
                    {
                        {1, 5},
                        {2, 10}
                    }
                },

                // will happen 5 seconds after the first and last for 10 seconds
                new Schedule
                {
                    Day = today.DayOfWeek,
                    Time = today.AddSeconds(25).TimeOfDay, // will start 5 seconds from now
                    ZoneDurations = new Dictionary<int, int>
                    {
                        {1, 5},
                        {2, 5}
                    }
                }
            };

            foreach (var scheduleItem in schedule)
            {
                var timmerCallback = new TimerCallback(arg =>
                {
                    var sprinklerAction = new Action(() => RunSprinklers(weather, scheduleItem));
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
