using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Schedule;

public class UpdateScheduleDto
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
}