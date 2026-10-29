namespace HospitalAppointment.Api.Models.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<DoctorDepartment> DoctorAssignments { get; set; } = new List<DoctorDepartment>();
    public ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();
    public ICollection<DoctorScheduleSlot> ScheduleSlots { get; set; } = new List<DoctorScheduleSlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
