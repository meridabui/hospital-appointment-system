namespace HospitalAppointment.Api.Models.Entities;

public class User
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Phone { get; set; }
    public string FullName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Role Role { get; set; } = null!;
    public Staff? Staff { get; set; }
    public Patient? Patient { get; set; }
    public ICollection<DoctorDepartment> DepartmentAssignments { get; set; } = new List<DoctorDepartment>();
    public ICollection<DoctorScheduleSlot> CreatedScheduleSlots { get; set; } = new List<DoctorScheduleSlot>();
    public ICollection<Appointment> ConfirmedAppointments { get; set; } = new List<Appointment>();
    public ICollection<Appointment> CheckedInAppointments { get; set; } = new List<Appointment>();
    public ICollection<Appointment> CompletedAppointments { get; set; } = new List<Appointment>();
    public ICollection<Appointment> CancelledAppointments { get; set; } = new List<Appointment>();
    public ICollection<Appointment> NoShowAppointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> FinalizedRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
