using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IAppointmentRepository
{
    // Slot đang được một appointment còn hiệu lực giữ chỗ
    Task<bool> IsSlotTakenAsync(int scheduleSlotId);

    // Patient đã có appointment còn hiệu lực với cùng Doctor, cùng ngày và giờ
    Task<bool> PatientHasActiveAtAsync(int patientId, int doctorId, DateOnly date, TimeOnly startTime);

    void Add(Appointment appointment);
}

// Một class hiện thực CẢ HAI interface cùng tên:
//  - Repositories.IAppointmentRepository            (luồng đặt lịch - BookingService qua UnitOfWork)
//  - Repositories.Interfaces.IAppointmentRepository (luồng lễ tân + khám bệnh)
public class AppointmentRepository : IAppointmentRepository, Interfaces.IAppointmentRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public AppointmentRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<bool> IsSlotTakenAsync(int scheduleSlotId)
        => _db.Appointments.AnyAsync(a => a.ScheduleSlotId == scheduleSlotId && a.IsActiveBooking);

    public Task<bool> PatientHasActiveAtAsync(int patientId, int doctorId, DateOnly date, TimeOnly startTime)
        => _db.Appointments.AnyAsync(a => a.PatientId == patientId
                                       && a.DoctorId == doctorId
                                       && a.IsActiveBooking
                                       && a.ScheduleSlot.SlotDate == date
                                       && a.ScheduleSlot.StartTime == startTime);

    public void Add(Appointment appointment) => _db.Appointments.Add(appointment);

    // ================= Repositories.Interfaces.IAppointmentRepository =================

    public Task<List<Appointment>> SearchByCodeOrPatientNameAsync(string? keyword, DateOnly? date = null)
    {
        var query = _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Staff).ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .Include(a => a.CheckedInByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(a => a.AppointmentCode.Contains(k)
                                  || a.Patient.FullName.Contains(k)
                                  || (a.Patient.Phone != null && a.Patient.Phone.Contains(k)));
        }

        if (date.HasValue)
        {
            var d = date.Value;
            query = query.Where(a => a.ScheduleSlot.SlotDate == d);
        }

        return query
            .OrderBy(a => a.ScheduleSlot.SlotDate)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync();
    }

    public Task<Appointment?> FindByIdAsync(int id)
        => _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

    public Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(int doctorId, AppointmentStatus[] statuses, DateOnly slotDate)
        => _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .Where(a => a.DoctorId == doctorId
                     && statuses.Contains(a.Status)
                     && a.ScheduleSlot.SlotDate == slotDate)
            .OrderBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync();

    public Task UpdateAsync(Appointment appointment)
    {
        // Entity đang được track thì không cần làm gì; chỉ attach lại khi bị detached
        if (_db.Entry(appointment).State == EntityState.Detached)
            _db.Appointments.Update(appointment);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync() => await _db.SaveChangesAsync() > 0;
}