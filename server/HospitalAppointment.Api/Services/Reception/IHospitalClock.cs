namespace HospitalAppointment.Api.Services.Reception;

public interface IHospitalClock
{
    DateTime UtcNow { get; }
    DateTime HospitalNow { get; }
}
