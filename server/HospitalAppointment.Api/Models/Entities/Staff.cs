using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Models.Entities;

public class Staff
{
    public int StaffId { get; set; }
    public int UserId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string? JobTitle { get; set; }
    public DateOnly? HireDate { get; set; }
    public StaffStatus StaffStatus { get; set; } = StaffStatus.Active;

    public User User { get; set; } = null!;
    public Doctor? Doctor { get; set; }
}
