using System;
using System.Collections.Generic;

namespace Holiday
{
    class Program
    {
        private static readonly List<DateTime> ExtraWorkDays = new List<DateTime>
        {
            new DateTime(2019, 08, 10),
            new DateTime(2019, 12, 07),
            new DateTime(2019, 12, 14),
        };

        private static readonly List<DateTime> ExtraFreeDays = new List<DateTime>
        {
            new DateTime(2019, 08, 19),
            new DateTime(2019, 12, 24),
            new DateTime(2019, 12, 27),
        };

        private static void Main(string[] args)
        {
            var calendar = new HolidayCalendar(ExtraWorkDays, ExtraFreeDays, null);
            Console.WriteLine(calendar.AddWorkDays(new DateTime(2019, 12, 05), -3));
            Console.WriteLine(calendar.WorkDaysBetween(new DateTime(2019, 12, 31), new DateTime(2018, 12, 31)));
            Console.WriteLine(calendar.WorkDaysBetween(new DateTime(2018, 12, 31), new DateTime(2019, 12, 31)));
            Console.WriteLine(calendar.WorkDaysBetween(DateTime.Now.Date, new DateTime(2019, 12, 31)));
            Console.WriteLine(calendar.WorkDaysBetween(new DateTime(2019, 12, 31), DateTime.Now.Date));
        }
    }
}
