using System;
using Windows.Devices.Gpio;

namespace SmartSprinklerController
{
    public static class Utilities
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static GpioPin ConfigureGpioPin(int pinNumber, GpioPinDriveMode driveMode)
        {
            var gpioController = GpioController.GetDefault();

            GpioPin pin = null;
            if (gpioController != null)
            {
                pin = gpioController.OpenPin(pinNumber);
                if (pin != null)
                {
                    pin.SetDriveMode(driveMode);
                }
            }

            return pin;
        }

        public static GpioPinValue InvertGpioPinValue(GpioPin gpioPin)
        {
            var currentPinValue = gpioPin.Read();

            GpioPinValue newPinValue;

            if (currentPinValue == GpioPinValue.High)
            {
                newPinValue = GpioPinValue.Low;
            }
            else
            {
                newPinValue = GpioPinValue.High;
            }

            return newPinValue;
        }

        public static DateTimeOffset FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
    }
}
