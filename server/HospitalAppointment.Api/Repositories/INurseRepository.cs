using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface INurseRepository
{
    Task<List<Appointment>> GetCheckedInAppointmentsAsync(
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}