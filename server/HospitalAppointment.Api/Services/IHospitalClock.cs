namespace HospitalAppointment.Api.Services;

public interface IHospitalClock
{
    DateTime UtcNow { get; }
    DateTime HospitalNow { get; }
}
