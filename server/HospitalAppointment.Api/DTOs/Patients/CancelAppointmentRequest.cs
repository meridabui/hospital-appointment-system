using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Patients;

public class CancelAppointmentRequest
{
    [StringLength(500, ErrorMessage = "Lý do hủy tối đa 500 ký tự.")]
    public string? Reason { get; set; }
}
