namespace HospitalAppointment.Client.Models;

public sealed class ReceptionAppointment
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientCode { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public DateOnly? PatientDateOfBirth { get; set; }
    public string? PatientGender { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialty { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? BookingReason { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? NoShowAt { get; set; }
    public string? NoShowReason { get; set; }
}
