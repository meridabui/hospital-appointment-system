namespace HospitalAppointment.Api.Models.Entities;

public class DoctorScheduleSlot
{
    public int ScheduleSlotId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
