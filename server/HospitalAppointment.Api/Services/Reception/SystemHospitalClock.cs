namespace HospitalAppointment.Api.Services.Reception;

public sealed class SystemHospitalClock : IHospitalClock
{
    private readonly TimeZoneInfo _hospitalTimeZone;

    public SystemHospitalClock(IConfiguration configuration)
    {
        var configuredId = configuration["Hospital:TimeZoneId"];
        _hospitalTimeZone = ResolveTimeZone(configuredId);
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime HospitalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _hospitalTimeZone);

    private static TimeZoneInfo ResolveTimeZone(string? configuredId)
    {
        var candidates = new[] { configuredId, "SE Asia Standard Time", "Asia/Ho_Chi_Minh" };

        foreach (var candidate in candidates.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidate!);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException("The hospital time zone could not be resolved.");
    }
}
