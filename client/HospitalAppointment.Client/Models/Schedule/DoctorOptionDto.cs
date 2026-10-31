namespace HospitalAppointment.Client.Models.Schedule;

public class DoctorOptionDto
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string? Specialty { get; set; }
}