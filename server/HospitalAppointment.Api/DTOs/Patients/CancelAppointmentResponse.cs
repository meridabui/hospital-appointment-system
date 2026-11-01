namespace HospitalAppointment.Api.DTOs.PatientPortal;

public class CancelAppointmentResponse
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CancelledAt { get; set; }
    public string Message { get; set; } = null!;
}
