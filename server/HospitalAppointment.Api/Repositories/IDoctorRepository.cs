using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface IDoctorRepository
{
    Task<Doctor?> GetActiveByUserIdAsync(int userId);
}
