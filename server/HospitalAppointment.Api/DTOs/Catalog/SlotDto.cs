namespace HospitalAppointment.Api.DTOs.Catalog;

public class SlotDto
{
    public int ScheduleSlotId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}