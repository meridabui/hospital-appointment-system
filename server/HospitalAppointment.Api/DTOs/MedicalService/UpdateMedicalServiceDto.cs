using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.MedicalService;

public class UpdateMedicalServiceDto
{
    [Required(ErrorMessage = "Department is required.")]
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Service code is required.")]
    [StringLength(40)]
    public string ServiceCode { get; set; } = null!;

    [Required(ErrorMessage = "Service name is required.")]
    [StringLength(150)]
    public string ServiceName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
}