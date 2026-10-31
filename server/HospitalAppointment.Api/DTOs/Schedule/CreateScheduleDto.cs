using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Schedule;

public class CreateScheduleDto
{
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required]
    public DateOnly SlotDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Range(1, 480)]
    public int SlotDurationMinutes { get; set; } = 60;
}