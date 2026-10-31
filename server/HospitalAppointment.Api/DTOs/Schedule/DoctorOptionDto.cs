namespace HospitalAppointment.Api.DTOs.Schedule;

public class DoctorOptionDto
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string? Specialty { get; set; }
}