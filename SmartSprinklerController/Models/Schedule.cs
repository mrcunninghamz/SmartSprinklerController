using System;
using System.Collections.Generic;

namespace SmartSprinklerController.Models
{
    public sealed class Schedule
    {
        public DayOfWeek Day { get; set; }

        public TimeSpan Time { get; set; }

        /// <summary>
        /// Zones to run
        /// </summary>
        public ICollection<int> Zones { get; set; } = new List<int>();

    }
}