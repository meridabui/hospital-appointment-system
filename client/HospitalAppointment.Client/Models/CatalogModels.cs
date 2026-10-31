namespace HospitalAppointment.Client.Models;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DoctorListItemDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public int DepartmentId { get; set; }
    public int AvailableSlotCount { get; set; }
    public DateOnly? NextAvailableDate { get; set; }
}

public class AvailableDateDto
{
    public DateOnly Date { get; set; }
    public int AvailableSlotCount { get; set; }
}

public class SlotDto
{
    public int ScheduleSlotId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}