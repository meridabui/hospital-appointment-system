using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.Models.DTOs.Reception;

public class MarkNoShowRequest
{
    public int? UserId { get; set; }

    [Required(ErrorMessage = "Lý do vắng mặt không được để trống.")]
    [StringLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự.")]
    public string Reason { get; set; } = string.Empty;
}

