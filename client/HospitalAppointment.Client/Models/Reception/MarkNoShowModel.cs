using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Client.Models.Reception;

public class MarkNoShowModel
{
    public int? UserId { get; set; } = 4;

    [Required(ErrorMessage = "Vui lòng nhập lý do vắng mặt.")]
    [StringLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự.")]
    public string Reason { get; set; } = string.Empty;
}

