using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Models.Entities;

public class Doctor
{
    public int DoctorId { get; set; }
    public int StaffId { get; set; }
    public string? Specialty { get; set; }
    public DoctorStatus DoctorStatus { get; set; } = DoctorStatus.Active;

    public Staff Staff { get; set; } = null!;
    public ICollection<DoctorDepartment> DepartmentAssignments { get; set; } = new List<DoctorDepartment>();
    public ICollection<DoctorScheduleSlot> ScheduleSlots { get; set; } = new List<DoctorScheduleSlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<OrderedService> OrderedServices { get; set; } = new List<OrderedService>();
}
