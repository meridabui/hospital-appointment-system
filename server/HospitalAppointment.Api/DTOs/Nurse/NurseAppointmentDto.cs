namespace HospitalAppointment.Api.DTOs.Nurse;

public class NurseAppointmentDto
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;

    public int PatientId { get; set; }
    public string PatientCode { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public string? PatientPhone { get; set; }
    public int? ReadyBy { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;

    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = null!;

    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = null!;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? ReadyAt { get; set; }
}