namespace HospitalAppointment.Client.Models.Schedule;

public class CreateScheduleDto
{
    public int DoctorId { get; set; }

    public int DepartmentId { get; set; }

    public DateOnly SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotDurationMinutes { get; set; } = 60;
}