using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public AppointmentRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Appointment?> GetByIdAsync(int appointmentId)
    {
        return _context.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
    }

    public Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(int doctorId, AppointmentStatus[] statuses, DateOnly slotDate)
    {
        return _context.Appointments
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.ScheduleSlot)
            .Where(x => x.DoctorId == doctorId && statuses.Contains(x.Status) && x.ScheduleSlot.SlotDate == slotDate)
            .OrderBy(x => x.ScheduleSlot.StartTime)
            .ToListAsync();
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
