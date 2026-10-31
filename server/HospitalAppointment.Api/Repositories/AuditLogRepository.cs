using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface IAuditLogRepository
{
    void Add(AuditLog log);
}

public class AuditLogRepository : IAuditLogRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public AuditLogRepository(HospitalAppointmentDbContext db) => _db = db;

    public void Add(AuditLog log) => _db.AuditLogs.Add(log);
}