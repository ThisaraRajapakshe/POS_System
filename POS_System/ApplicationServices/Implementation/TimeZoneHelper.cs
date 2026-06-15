using NodaTime;

namespace POS_System.ApplicationServices.Implementation
{
    public class TimeZoneHelper
    {
        public (DateTime utcStart, DateTime utcEnd) GetUtcRange(LocalDate localDate, string timeZoneId)
        {
            var zone = DateTimeZoneProviders.Tzdb[timeZoneId];
            LocalDateTime startOfDay = localDate.AtMidnight();
            LocalDateTime endOfDay = localDate.PlusDays(1).AtMidnight();

            ZonedDateTime zdtStart = startOfDay.InZoneLeniently(zone);
            ZonedDateTime zdtEnd = endOfDay.InZoneLeniently(zone);

            return (zdtStart.ToDateTimeUtc(), zdtEnd.ToDateTimeUtc());
        }
    }
}
