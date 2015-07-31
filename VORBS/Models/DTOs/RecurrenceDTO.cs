using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VORBS.Models.DTOs
{
    public class RecurrenceDTO
    {
        public bool IsRecurring { get; set; }
        public bool SkipClashes { get; set; }
        public bool AutoAlternateRoom { get; set; }
        public bool AdminOverwrite { get; set; }
        public string AdminOverwriteMessage { get; set; }

        public string Frequency { get; set; }
        public DateTime EndDate { get; set; }

        public int DailyDayCount { get; set; }
        
        public int WeeklyWeekCount { get; set; }
        public int WeeklyDay { get; set; }

        public int MonthlyMonthCount { get; set; }
        public int MonthlyMonthDay { get; set; }
        public int MonthlyMonthDayCount { get; set; }
    }
}
