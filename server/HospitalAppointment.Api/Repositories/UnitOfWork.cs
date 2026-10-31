using HospitalAppointment.Api.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalAppointment.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly HospitalAppointmentDbContext _db;

    public UnitOfWork(
        HospitalAppointmentDbContext db,
        IRoleRepository roles,
        IUserRepository users,
        IPatientRepository patients,
        IAuditLogRepository auditLogs,
        IDepartmentRepository departments,
        IDoctorRepository doctors,
        IScheduleSlotRepository scheduleSlots)
    {
        _db = db;
        Roles = roles;
        Users = users;
        Patients = patients;
        AuditLogs = auditLogs;
        Departments = departments;
        Doctors = doctors;
        ScheduleSlots = scheduleSlots;
    }

    public IRoleRepository Roles { get; }
    public IUserRepository Users { get; }
    public IPatientRepository Patients { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IDepartmentRepository Departments { get; }
    public IDoctorRepository Doctors { get; }
    public IScheduleSlotRepository ScheduleSlots { get; }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public Task<IDbContextTransaction> BeginTransactionAsync() => _db.Database.BeginTransactionAsync();
}