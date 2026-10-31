namespace HospitalAppointment.Api.DTOs.Patients;

public class CreatePatientResponse
{
    public int UserId { get; set; }
    public int PatientId { get; set; }
    public string PatientCode { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string TemporaryPassword { get; set; } = null!;
    public string Message { get; set; } = null!;
}