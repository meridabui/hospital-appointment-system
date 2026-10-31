using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Reception;

public sealed class MarkNoShowRequest
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
