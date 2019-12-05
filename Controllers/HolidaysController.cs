using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Holiday;
using HolidayApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HolidayApi.Controllers
{
    /// <summary>
    /// Különleges nap
    /// </summary>
    public class DayViewModel
    {
        /// <summary>
        /// A dátum, yyyy-MM-dd formátumban
        /// </summary>
        public string date { get; set; }
        /// <summary>
        /// A dátum típusa
        /// </summary>
        public DayType dayType { get; set; }
    }

    /// <summary>
    /// Holiday Controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HolidaysController : ControllerBase
    {
        private static readonly List<DateTime> ExtraWorkDays = new List<DateTime>
        {
            new DateTime(2019, 08, 10),
            new DateTime(2019, 12, 07),
            new DateTime(2019, 12, 14),
            new DateTime(2020, 08, 29),
            new DateTime(2020, 12, 12)
        };

        private static readonly List<DateTime> ExtraFreeDays = new List<DateTime>
        {
            new DateTime(2019, 08, 19),
            new DateTime(2019, 12, 24),
            new DateTime(2019, 12, 27),
            new DateTime(2020, 08, 21),
            new DateTime(2020, 12, 24)
        };

        private static readonly List<DateTime> LocalHolidays = new List<DateTime>
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


        private readonly ILogger<HolidaysController> _logger;
        private readonly DayDbContext dbContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        public HolidaysController(ILogger<HolidaysController> logger, DayDbContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
            if (dbContext.Days.Count() == 0)
            {
                ExtraFreeDays.ForEach(d => dbContext.Days.Add(new Day { Date = d, DayType = DayType.ExtraFreeDay }));
                ExtraWorkDays.ForEach(d => dbContext.Days.Add(new Day { Date = d, DayType = DayType.ExtraFreeDay }));
                LocalHolidays.ForEach(d => dbContext.Days.Add(new Day { Date = d, DayType = DayType.LocalHoliday }));
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Nap típusának beállítása
        /// </summary>
        /// <param name="dayViewModel">A dátum és a típusa</param>
        /// <returns>bool</returns>
        [HttpPost("setDay")]
        [Produces("application/json")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<bool>))]
        public async Task<ApiResponse<bool>> AddDay([FromBody] DayViewModel dayViewModel)
        {
            var d = DateTime.ParseExact(dayViewModel.date, "yyyy-MM-dd", null);
            var dayModel = await dbContext.Days.FirstOrDefaultAsync(p => p.Date.Date == d.Date);
            if (dayModel == null)
            {
                dayModel = new Day();
                await dbContext.Days.AddAsync(dayModel);
            }
            dayModel.Date = d.Date;
            dayModel.DayType = dayViewModel.dayType;
            await dbContext.SaveChangesAsync();
            return new ApiResponse<bool> { Data = true, Success = true };
        }

        /// <summary>
        /// Különleges nap törlése
        /// </summary>
        /// <param name="date">A dátum yyyy-MM-dd formátumban</param>
        /// <returns>bool</returns>
        [HttpDelete("removeDay")]
        [Produces("application/json")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<bool>))]
        public async Task<ApiResponse<bool>> RemoveDay(string date)
        {
            var d = DateTime.ParseExact(date, "yyyy-MM-dd", null);
            var dayModel = await dbContext.Days.FirstOrDefaultAsync(p => p.Date.Date == d.Date);
            if (dayModel == null)
            {
                return new ApiResponse<bool>() { Data = true, Success = true };
            }
            dbContext.Days.Remove(dayModel);
            await dbContext.SaveChangesAsync();
            return new ApiResponse<bool> { Data = true, Success = true };
        }

        /// <summary>
        /// Visszaadja húsvét vasárnap napját a megadott évben
        /// </summary>
        /// <param name="year">Az év amiben a húsvétet keressük</param>
        /// <returns>DateTime</returns>
        [HttpGet("EasterSunday/{year}")]
        [Produces("application/json")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<DateTime>))]
        public ApiResponse<DateTime> EasterSunday(int year)
        {
            return new ApiResponse<DateTime>()
            {
                Data = new HolidayCalendar(
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraWorkDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraFreeDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.LocalHoliday).Select(p => p.Date.Date).ToList()
            ).EasterSunday(year)
            };
        }

        /// <summary>
        /// Megmondja, hogy az adott dátum munkanap-e
        /// </summary>
        /// <param name="date">A dátum</param>
        /// <returns>bool</returns>
        [HttpGet("IsWorkDay/{date}")]
        [Produces("application/json")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<bool>))]
        public ApiResponse<bool> IsWorkDay(DateTime date)
        {
            return new ApiResponse<bool>()
            {
                Data = new HolidayCalendar(
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraWorkDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraFreeDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.LocalHoliday).Select(p => p.Date.Date).ToList()
                    ).IsWorkDay(date)
            };
        }

        /// <summary>
        /// Kiszámítja, hogy két dátum között hány munkanap van
        /// </summary>
        /// <param name="from">A kezdő dátum</param>
        /// <param name="to">A végdátum</param>
        /// <returns>int</returns>
        [HttpGet("WorkDaysBeetween/{from}/{to}")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<int>))]
        [Produces("application/json")]
        public ApiResponse<int> WorkDaysBetween(DateTime from, DateTime to)
        {
            return new ApiResponse<int>() { Data = new HolidayCalendar(
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraWorkDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraFreeDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.LocalHoliday).Select(p => p.Date.Date).ToList()
                ).WorkDaysBetween(from, to) };
        }

        /// <summary>
        /// Hozzáadja a megadott számú munkanapot a megadott dátumhoz és visszaadja az így kapott dátumot
        /// </summary>
        /// <param name="date">A kezdő dátum</param>
        /// <param name="days">A munkanapok száma</param>
        /// <returns>DateTime</returns>
        [ProducesResponseType(200, Type = typeof(ApiResponse<DateTime>))]
        [Produces("application/json")]
        [HttpGet("AddWorkdDays/{date}/{days}")]
        public ApiResponse<DateTime> AddWorkdDays(DateTime date, int days)
        {
            return new ApiResponse<DateTime>() { Data = new HolidayCalendar(
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraWorkDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.ExtraFreeDay).Select(p => p.Date.Date).ToList(),
                    dbContext.Days.Where(p => p.DayType == DayType.LocalHoliday).Select(p => p.Date.Date).ToList()
                ).AddWorkDays(date, days) };
        }
    }
}
