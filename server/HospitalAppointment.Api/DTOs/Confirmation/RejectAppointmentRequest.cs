using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Confirmation;

public sealed class RejectAppointmentRequest
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
