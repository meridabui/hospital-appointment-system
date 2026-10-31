using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Appointments;

public class CreateAppointmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khoa.")]
    public int DepartmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn bác sĩ.")]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn ngày và giờ khám.")]
    public int ScheduleSlotId { get; set; }

    [StringLength(500, ErrorMessage = "Lý do khám tối đa 500 ký tự.")]
    public string? BookingReason { get; set; }
}