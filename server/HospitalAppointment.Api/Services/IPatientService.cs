using HospitalAppointment.Api.DTOs.Patients;

namespace HospitalAppointment.Api.Services;

public interface IPatientService
{
    Task<CreatePatientResponse> CreateByReceptionistAsync(CreatePatientRequest request, int receptionistUserId);
}