using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace SmartSprinklerController.Models
{
    public sealed class SprinklerZone
    {
        public string Number { get; set; }
        public GpioPin Pin { get; set; }

        public SprinklerZone(string zoneNumber, int pinNumber)
        {
            Number = zoneNumber;
            Pin = Utilities.ConfigureGpioPin(pinNumber, GpioPinDriveMode.Output);
            Pin.Write(GpioPinValue.Low);
        }

        public void ActiveDuration(int seconds)
        {
            Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();
            var value = Utilities.InvertGpioPinValue(Pin);
            Pin.Write(value);
        }
    }
}
