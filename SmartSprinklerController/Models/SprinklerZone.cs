using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace SmartSprinklerController.Models
{
    public sealed class SprinklerZone
    {
        public int Number { get; }

        public int Duration { get; private set; }
        private GpioPin Pin { get; }

        public SprinklerZone(int zoneNumber, int pinNumber)
        {
            Number = zoneNumber;
            Pin = Utilities.ConfigureGpioPin(pinNumber, GpioPinDriveMode.Output);
            Pin.Write(GpioPinValue.Low);
        }

        public void SetDuration(int seconds)
        {
            Duration = seconds;
        }

        public void RunZone()
        {
            var value = Utilities.InvertGpioPinValue(Pin);
            Pin.Write(value);
            Task.Delay(TimeSpan.FromSeconds(Duration)).Wait();
            value = Utilities.InvertGpioPinValue(Pin);
            Pin.Write(value);
        }
    }
}
