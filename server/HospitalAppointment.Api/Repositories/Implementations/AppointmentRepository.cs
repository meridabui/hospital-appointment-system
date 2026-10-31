using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories.Implementations;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public AppointmentRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> SearchByCodeOrPatientNameAsync(string? keyword, DateOnly? date = null)
    {
        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Staff)
                    .ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .Include(a => a.CheckedInByUser)
            .AsNoTracking();

        if (date.HasValue)
        {
            query = query.Where(a => a.ScheduleSlot.SlotDate == date.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmed = keyword.Trim().ToLower();
            query = query.Where(a =>
                a.AppointmentCode.ToLower().Contains(trimmed) ||
                a.Patient.FullName.ToLower().Contains(trimmed) ||
                (a.Patient.Phone != null && a.Patient.Phone.Contains(trimmed)) ||
                a.Patient.PatientCode.ToLower().Contains(trimmed));
        }

        return await query
            .OrderBy(a => a.ScheduleSlot.SlotDate)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync();
    }

    public async Task<Appointment?> FindByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Staff)
                    .ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .Include(a => a.CheckedInByUser)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    public Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

