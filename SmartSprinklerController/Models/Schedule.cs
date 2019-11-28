using System;
using System.Collections.Generic;

namespace SmartSprinklerController.Models
{
    public sealed class Schedule
    {
        public DayOfWeek Day { get; set; }

        public TimeSpan Time { get; set; }

        /// <summary>
        /// Zone duration dictionary <zonenumber, durration in seconds>
        /// </summary>
        public Dictionary<int, int> ZoneDurations { get; set; }

    }
}