using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalAppointment.Api.Repositories;

public interface IUnitOfWork
{
    IRoleRepository Roles { get; }
    IUserRepository Users { get; }
    IPatientRepository Patients { get; }
    IAuditLogRepository AuditLogs { get; }
    IDepartmentRepository Departments { get; }
    IDoctorRepository Doctors { get; }
    IScheduleSlotRepository ScheduleSlots { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync(); // phần C dùng cho Submit Booking
}