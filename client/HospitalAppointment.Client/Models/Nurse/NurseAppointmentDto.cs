namespace HospitalAppointment.Client.Models.Nurse;

public class NurseAppointmentDto
{
    public int AppointmentId { get; set; }

    public string AppointmentCode { get; set; } = string.Empty;

    public int PatientId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public string? PatientPhone { get; set; }

    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public DateOnly SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? CheckedInAt { get; set; }

    public DateTime? ReadyAt { get; set; }

    public int? ReadyBy { get; set; }
}