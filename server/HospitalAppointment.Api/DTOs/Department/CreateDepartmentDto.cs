using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Department;

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Department code is required.")]
    [StringLength(30)]
    public string DepartmentCode { get; set; } = null!;

    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(150)]
    public string DepartmentName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
}