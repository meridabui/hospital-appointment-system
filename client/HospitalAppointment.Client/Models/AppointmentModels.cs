namespace HospitalAppointment.Client.Models;

public class CreateAppointmentRequest
{
    public int DepartmentId { get; set; }
    public int DoctorId { get; set; }
    public int ScheduleSlotId { get; set; }
    public string? BookingReason { get; set; }
}

public class AppointmentResponse
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? BookingReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}