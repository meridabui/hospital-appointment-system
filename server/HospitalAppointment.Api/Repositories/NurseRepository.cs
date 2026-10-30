using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class NurseRepository : INurseRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public NurseRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách bệnh nhân đã Check-in
    public async Task<List<Appointment>> GetCheckedInAppointmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.CheckedIn)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Staff)
                    .ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .OrderBy(a => a.ScheduleSlot.SlotDate)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync(cancellationToken);
    }

    // Lấy chi tiết Appointment để cập nhật
    public async Task<Appointment?> GetByIdAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Staff)
                    .ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .FirstOrDefaultAsync(
                a => a.AppointmentId == appointmentId,
                cancellationToken);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}