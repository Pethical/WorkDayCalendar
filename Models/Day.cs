using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayApi.Models
{
    /// <summary>
    /// A nap típusa
    /// </summary>
    public enum DayType
    {
        /// <summary>
        /// Rendkívüli munkaszüneti/ szabadnap
        /// </summary>
        ExtraFreeDay,
        /// <summary>
        /// Rendkívüli munkanap
        /// </summary>
        ExtraWorkDay,
        /// <summary>
        /// Nemzeti ünnep / országos munkaszüneti nap
        /// </summary>
        LocalHoliday
    };

    public class Day
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [EnumDataType(typeof(DayType))]
        public DayType DayType { get; set; }
    }

    public class  DayDbContext : DbContext {

        public DbSet<Day> Days { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {            
            optionsBuilder.UseInMemoryDatabase("days");
            base.OnConfiguring(optionsBuilder);
        }

    }


}
