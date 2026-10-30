namespace HospitalAppointment.Api.Models.Entities;

public class DoctorDepartment
{
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime AssignedAt { get; set; }
    public int? AssignedBy { get; set; }
    public bool IsPrimary { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public User? AssignedByUser { get; set; }
}
