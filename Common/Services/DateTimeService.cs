using Common.Interfaces;

namespace Common.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime GetUtcDateTime() => DateTime.UtcNow;

}