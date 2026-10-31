namespace HospitalAppointment.Client.Models.Nurse;

public class MarkReadyResponseDto
{
    public string Message { get; set; } = string.Empty;

    public int AppointmentId { get; set; }

    public string AppointmentCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? ReadyAt { get; set; }

    public int? ReadyBy { get; set; }
}