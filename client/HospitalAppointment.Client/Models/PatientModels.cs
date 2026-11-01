namespace HospitalAppointment.Client.Models;

// Khớp MyAppointmentDto của server
public class MyAppointmentModel
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
    public bool CanCancel { get; set; }
    public string? CancelBlockedReason { get; set; }
    public bool HasMedicalRecord { get; set; }
}

public class CancelAppointmentModel
{
    public string? Reason { get; set; }
}

public class CancelAppointmentResult
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CancelledAt { get; set; }
    public string Message { get; set; } = null!;
}

// Khớp MedicalHistoryItemDto / MedicalHistoryDetailDto của server
public class MedicalHistoryItemModel
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public int MedicalRecordId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string? Specialty { get; set; }
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }
    public DateTime? FinalizedAt { get; set; }
}

public class MedicalHistoryDetailModel : MedicalHistoryItemModel
{
    public PrescriptionInfo? Prescription { get; set; }
    public List<OrderedServiceInfo> OrderedServices { get; set; } = new();
}

public class PrescriptionInfo
{
    public string? Notes { get; set; }
    public List<PrescriptionItemInfo> Items { get; set; } = new();
}

public class PrescriptionItemInfo
{
    public int PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = null!;
    public string? Dosage { get; set; }
    public string? Route { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public decimal? Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class OrderedServiceInfo
{
    public int OrderedServiceId { get; set; }
    public int MedicalServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? Notes { get; set; }
    public string ServiceStatus { get; set; } = null!;
}

// Nhãn tiếng Việt + màu badge cho trạng thái lịch hẹn
public static class AppointmentStatusDisplay
{
    public static readonly string[] All =
    {
        "Pending", "Confirmed", "CheckedIn", "ReadyForConsultation", "InConsultation",
        "Completed", "Rejected", "Cancelled", "NoShow"
    };

    public static string Label(string status) => status switch
    {
        "Pending" => "Chờ xác nhận",
        "Confirmed" => "Đã xác nhận",
        "CheckedIn" => "Đã check-in",
        "ReadyForConsultation" => "Sẵn sàng khám",
        "InConsultation" => "Đang khám",
        "Completed" => "Đã khám xong",
        "Rejected" => "Bị từ chối",
        "Cancelled" => "Đã hủy",
        "NoShow" => "Vắng mặt",
        _ => status
    };

    public static string Badge(string status) => status switch
    {
        "Pending" => "bg-warning text-dark",
        "Confirmed" => "bg-primary",
        "CheckedIn" or "ReadyForConsultation" or "InConsultation" => "bg-info text-dark",
        "Completed" => "bg-success",
        "Rejected" or "NoShow" => "bg-danger",
        "Cancelled" => "bg-secondary",
        _ => "bg-light text-dark"
    };
}
