using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IAppointmentRepository
{
    // Đặt lịch
    Task<bool> IsSlotTakenAsync(int scheduleSlotId);
    Task<bool> PatientHasActiveAtAsync(int patientId, int doctorId, DateOnly date, TimeOnly startTime);
    void Add(Appointment appointment);

    // Lễ tân (3 lỗi của bạn nằm ở 2 hàm này)
    Task<List<Appointment>> SearchForReceptionAsync(string? keyword, DateOnly? date = null);
    Task<Appointment?> GetForReceptionUpdateAsync(int id);

    // Tên cũ, giữ để code khác không vỡ
    Task<List<Appointment>> SearchByCodeOrPatientNameAsync(string? keyword, DateOnly? date = null);
    Task<Appointment?> FindByIdAsync(int id);
    Task<List<Appointment>> SearchForReceptionAsync(
    string? search, DateOnly? date, AppointmentStatus? status, CancellationToken cancellationToken);

    Task<Appointment?> GetForReceptionUpdateAsync(int id, CancellationToken cancellationToken);

    // Khám bệnh
    Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(int doctorId, AppointmentStatus[] statuses, DateOnly slotDate);
    void AddAuditLog(AuditLog log);

    // Chung
    Task UpdateAsync(Appointment appointment);
    Task<bool> SaveChangesAsync();
}

public class AppointmentRepository : IAppointmentRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public AppointmentRepository(HospitalAppointmentDbContext db) => _db = db;

    // ---------- Đặt lịch ----------
    public Task<bool> IsSlotTakenAsync(int scheduleSlotId)
        => _db.Appointments.AnyAsync(a => a.ScheduleSlotId == scheduleSlotId && a.IsActiveBooking);

    public Task<bool> PatientHasActiveAtAsync(int patientId, int doctorId, DateOnly date, TimeOnly startTime)
        => _db.Appointments.AnyAsync(a => a.PatientId == patientId
                                       && a.DoctorId == doctorId
                                       && a.IsActiveBooking
                                       && a.ScheduleSlot.SlotDate == date
                                       && a.ScheduleSlot.StartTime == startTime);

    public void Add(Appointment appointment) => _db.Appointments.Add(appointment);

    // ---------- Lễ tân ----------
    // Chỉ đọc nên dùng AsNoTracking
    public async Task<List<Appointment>> SearchForReceptionAsync(string? keyword, DateOnly? date = null)
    {
        var query = _db.Appointments.AsNoTracking()
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
            query = query.Where(a => a.ScheduleSlot.SlotDate == date.Value);

        return await query
            .OrderBy(a => a.ScheduleSlot.SlotDate)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync();
    }

    // Có tracking vì sau đó entity bị sửa (check-in / no-show) rồi lưu
    public Task<Appointment?> GetForReceptionUpdateAsync(int id)
        => _db.Appointments
            .Include(a => a.ScheduleSlot)
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Staff).ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.CheckedInByUser)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

    // Tên cũ -> chuyển sang hàm mới
    public Task<List<Appointment>> SearchByCodeOrPatientNameAsync(string? keyword, DateOnly? date = null)
        => SearchForReceptionAsync(keyword, date);

    public Task<Appointment?> FindByIdAsync(int id)
        => GetForReceptionUpdateAsync(id);

    // ---------- Khám bệnh ----------
    public Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(int doctorId, AppointmentStatus[] statuses, DateOnly slotDate)
        => _db.Appointments.AsNoTracking()
            .Include(a => a.ScheduleSlot)
            .Include(a => a.Department)
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId
                     && statuses.Contains(a.Status)
                     && a.ScheduleSlot.SlotDate == slotDate)
            .OrderBy(a => a.ReadyAt)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync();

    public void AddAuditLog(AuditLog log) => _db.AuditLogs.Add(log);

    // ---------- Chung ----------
    public Task UpdateAsync(Appointment appointment)
    {
        _db.Appointments.Update(appointment);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync() => await _db.SaveChangesAsync() > 0;
    public async Task<List<Appointment>> SearchForReceptionAsync(
    string? search, DateOnly? date, AppointmentStatus? status, CancellationToken cancellationToken)
    {
        var query = _db.Appointments.AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Staff).ThenInclude(s => s.User)
            .Include(a => a.Department)
            .Include(a => a.ScheduleSlot)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var k = search.Trim();
            query = query.Where(a => a.AppointmentCode.Contains(k)
                                  || a.Patient.FullName.Contains(k)
                                  || (a.Patient.Phone != null && a.Patient.Phone.Contains(k)));
        }

        if (date.HasValue)
            query = query.Where(a => a.ScheduleSlot.SlotDate == date.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.ScheduleSlot.SlotDate)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync(cancellationToken);
    }

    // Có tracking vì sau đó entity bị sửa rồi lưu
    public Task<Appointment?> GetForReceptionUpdateAsync(int id, CancellationToken cancellationToken)
        => _db.Appointments
            .Include(a => a.ScheduleSlot)
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Staff).ThenInclude(s => s.User)
            .Include(a => a.Department)
            .FirstOrDefaultAsync(a => a.AppointmentId == id, cancellationToken);
}