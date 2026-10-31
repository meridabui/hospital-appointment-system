using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Client.Models.Reception;

public class MarkNoShowModel
{
    [Required(ErrorMessage = "Vui lòng nhập lý do vắng mặt.")]
    [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
    public string Reason { get; set; } = string.Empty;
}

