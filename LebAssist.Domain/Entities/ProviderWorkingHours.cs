using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ProviderWorkingHours
    {
        [Key]
        public int WorkingHourId { get; set; }

        public int ClientId { get; set; }

        public int ServiceId { get; set; }

        public DayOfWeekEnum DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual Client Provider { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}