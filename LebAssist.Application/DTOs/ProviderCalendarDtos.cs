namespace LebAssist.Application.DTOs
{
    public class ProviderCalendarDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<CalendarDayDto> Days { get; set; } = new();
        public List<AppointmentDto> Appointments { get; set; } = new();
    }

    public class CalendarDayDto
    {
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
        public int AppointmentCount { get; set; }
        public List<AppointmentDto> DayAppointments { get; set; } = new();
    }

    public class AppointmentDto
    {
        public int BookingId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhoto { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime ScheduledDateTime { get; set; }
        public TimeSpan ScheduledTime => ScheduledDateTime.TimeOfDay;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? ClientPhone { get; set; }
    }
}
