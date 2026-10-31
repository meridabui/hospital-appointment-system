using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IScheduleSlotRepository
{
    Task<List<DoctorScheduleSlot>> GetAvailableAsync(
        int doctorId, int departmentId, DateOnly from, DateOnly to, DateOnly today, TimeOnly now);

    Task<List<(int DoctorId, DateOnly SlotDate)>> GetAvailableSlotDatesAsync(
        IReadOnlyCollection<int> doctorIds, int departmentId, DateOnly today, TimeOnly now, DateOnly toDate);

    Task<DoctorScheduleSlot?> GetByIdAsync(int scheduleSlotId);
}

public class ScheduleSlotRepository : IScheduleSlotRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public ScheduleSlotRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<List<DoctorScheduleSlot>> GetAvailableAsync(
        int doctorId, int departmentId, DateOnly from, DateOnly to, DateOnly today, TimeOnly now)
        => _db.DoctorScheduleSlots.AsNoTracking()
            .Where(s => s.DoctorId == doctorId && s.DepartmentId == departmentId && s.SlotDate >= from)
            .AvailableForBooking(today, now, to)
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .ToListAsync();

    public async Task<List<(int DoctorId, DateOnly SlotDate)>> GetAvailableSlotDatesAsync(
        IReadOnlyCollection<int> doctorIds, int departmentId, DateOnly today, TimeOnly now, DateOnly toDate)
    {
        var rows = await _db.DoctorScheduleSlots.AsNoTracking()
            .Where(s => s.DepartmentId == departmentId && doctorIds.Contains(s.DoctorId))
            .AvailableForBooking(today, now, toDate)
            .Select(s => new { s.DoctorId, s.SlotDate })
            .ToListAsync();

        return rows.Select(r => (r.DoctorId, r.SlotDate)).ToList();
    }
    public Task<DoctorScheduleSlot?> GetByIdAsync(int scheduleSlotId)
    => _db.DoctorScheduleSlots.AsNoTracking().FirstOrDefaultAsync(s => s.ScheduleSlotId == scheduleSlotId);
}