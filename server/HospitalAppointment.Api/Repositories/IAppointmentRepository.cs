using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int appointmentId);
    Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(int doctorId, AppointmentStatus[] statuses, DateOnly slotDate);
    void AddAuditLog(AuditLog auditLog);
    Task SaveChangesAsync();
}
