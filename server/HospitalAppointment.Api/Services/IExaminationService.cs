using HospitalAppointment.Api.DTOs.Examination;

namespace HospitalAppointment.Api.Services;

public interface IExaminationService
{
    Task<List<ReadyPatientDto>> GetReadyPatientsAsync(int userId);
    Task StartConsultationAsync(int appointmentId, int userId);
}
