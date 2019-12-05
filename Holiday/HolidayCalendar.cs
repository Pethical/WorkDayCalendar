using System;
using System.Collections.Generic;
using System.Linq;

namespace Holiday
{
    public class HolidayCalendar
    {
        private readonly List<DateTime> ExtraWorkDays;
        private readonly List<DateTime> ExtraFreeDays;

        private readonly List<DateTime> LocalHolidays = new List<DateTime>
        {
            new DateTime(1900, 01, 01),
            new DateTime(1900, 03, 15),
            new DateTime(1900, 05, 01),
            new DateTime(1900, 08, 20),
            new DateTime(1900, 10, 23),
            new DateTime(2000, 11, 01), // Holiday since 2000
            new DateTime(1900, 12, 25),
            new DateTime(1900, 12, 26)
        };

        public HolidayCalendar(List<DateTime> extraWorkDays, List<DateTime> extraFreeDays, List<DateTime> localHolidays)
        {
            ExtraWorkDays = extraWorkDays ?? new List<DateTime>();
            ExtraFreeDays = extraFreeDays ?? new List<DateTime>();
            LocalHolidays = localHolidays ?? LocalHolidays;
        }

        public DateTime PentecostSunday(int year) => EasterSunday(year).AddDays(49);
        
        public DateTime EasterSunday(int year)
        {
            int g = year % 19;
            int c = year / 100;
            int h = (c - c / 4 - (8 * c + 13) / 25 + 19 * g + 15) % 30;
            int i = h - h / 28 * (1 - h / 28 * (29 / (h + 1)) * ((21 - g) / 11));
            int day = i - (year + year / 4 + i + 2 - c + c / 4) % 7 + 28;
            int month = 3;
            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        public bool IsWorkDay(DateTime date)
        {
            return
                ExtraWorkDays.Contains(date) ||
                (
                !ExtraFreeDays.Contains(date) &&
                date.DayOfWeek != DayOfWeek.Sunday
                && date.DayOfWeek != DayOfWeek.Saturday
                && LocalHolidays.Count(p => p.Year <= date.Year && p.Month == date.Month && p.Day == date.Day) == 0
                && EasterSunday(date.Year).AddDays(1) != date.Date
                && PentecostSunday(date.Year).AddDays(1) != date.Date
                && EasterSunday(date.Year).AddDays(-2) != date.Date
                );
        }

        public DateTime AddWorkDays(DateTime date, int workDays)
        {
            var wd = Math.Abs(workDays);
            while (wd > 0)
                wd -= IsWorkDay(date = date.Date.AddDays(Math.Sign(workDays))) ? 1 : 0;
            return date;
        }

        public int WorkDaysBetween(DateTime date1, DateTime date2)
        {
          return Enumerable.Range(0, Math.Abs((date2.Date - date1.Date).Days))
                           .Count(p => IsWorkDay(date1.Date.AddDays(p * Math.Sign((date2.Date - date1.Date).Days))));
        }        
    }
}