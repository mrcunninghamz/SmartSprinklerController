using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace SmartSprinklerController.Models
{
    public sealed class SprinklerZone
    {
        public int Number { get; set; }

        public int Duration { get; set; }
        private GpioPin Pin { get; set; }

        public SprinklerZone(int zoneNumber, int duration, int pinNumber)
        {
            Number = zoneNumber;
            Duration = duration;
            Pin = Utilities.ConfigureGpioPin(pinNumber, GpioPinDriveMode.Output);
            Pin.Write(GpioPinValue.Low);
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
