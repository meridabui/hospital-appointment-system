namespace HospitalAppointment.Api.DTOs.Schedule;

public class DoctorScheduleDto
{
    public int ScheduleSlotId { get; set; }

    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = null!;

    public string? Specialty { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public DateOnly SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}