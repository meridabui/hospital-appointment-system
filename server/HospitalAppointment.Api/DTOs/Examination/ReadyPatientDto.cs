namespace HospitalAppointment.Api.DTOs.Examination;

public class ReadyPatientDto
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? BookingReason { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;
    public int PatientId { get; set; }
    public string PatientCode { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
}
