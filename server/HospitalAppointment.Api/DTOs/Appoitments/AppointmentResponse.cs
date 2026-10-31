namespace HospitalAppointment.Api.DTOs.Appointments;

public class AppointmentResponse
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? BookingReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = null!;
}