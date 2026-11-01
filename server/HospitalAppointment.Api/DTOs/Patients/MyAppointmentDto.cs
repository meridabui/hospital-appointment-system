namespace HospitalAppointment.Api.DTOs.Patients;

public class MyAppointmentDto
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? BookingReason { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Server tính sẵn để client chỉ việc hiển thị nút Hủy
    public bool CanCancel { get; set; }
    public string? CancelBlockedReason { get; set; }

    // Có hồ sơ khám đã hoàn tất để xem ở "Lịch sử khám bệnh"
    public bool HasMedicalRecord { get; set; }
}
