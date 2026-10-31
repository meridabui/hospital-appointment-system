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

    // Lấy appointment theo Id (có tracking để cập nhật trạng thái)
    Task<Appointment?> GetByIdAsync(int appointmentId);

    Task<IReadOnlyList<Appointment>> SearchForReceptionAsync(
        string? search,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken);

    Task<Appointment?> GetForReceptionUpdateAsync(
        int appointmentId,
        CancellationToken cancellationToken);

    // Appointment của bác sĩ theo các trạng thái trong một ngày (dùng cho danh sách chờ khám)
    Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(
        int doctorId, IReadOnlyCollection<AppointmentStatus> statuses, DateOnly date);

    void Add(Appointment appointment);
    void AddAuditLog(AuditLog log);
    Task<int> SaveChangesAsync();
}

public class AppointmentRepository : IAppointmentRepository
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

    public Task<Appointment?> GetByIdAsync(int appointmentId)
        => _db.Appointments
            .Include(a => a.ScheduleSlot)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

    public async Task<IReadOnlyList<Appointment>> SearchForReceptionAsync(
        string? search,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var query = ReceptionQuery().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(appointment =>
                appointment.AppointmentCode.Contains(term) ||
                appointment.Patient.PatientCode.Contains(term) ||
                appointment.Patient.FullName.Contains(term) ||
                (appointment.Patient.Phone != null && appointment.Patient.Phone.Contains(term)));
        }

        if (date.HasValue)
        {
            query = query.Where(appointment => appointment.ScheduleSlot.SlotDate == date.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(appointment => appointment.Status == status.Value);
        }

        return await query
            .OrderBy(appointment => appointment.ScheduleSlot.SlotDate)
            .ThenBy(appointment => appointment.ScheduleSlot.StartTime)
            .Take(200)
            .ToListAsync(cancellationToken);
    }

    public Task<Appointment?> GetForReceptionUpdateAsync(
        int appointmentId,
        CancellationToken cancellationToken)
        => ReceptionQuery().SingleOrDefaultAsync(
            appointment => appointment.AppointmentId == appointmentId,
            cancellationToken);

    public Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(
        int doctorId, IReadOnlyCollection<AppointmentStatus> statuses, DateOnly date)
        => _db.Appointments.AsNoTracking()
            .Include(a => a.ScheduleSlot)
            .Include(a => a.Department)
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId
                     && statuses.Contains(a.Status)
                     && a.ScheduleSlot.SlotDate == date)
            .OrderBy(a => a.ReadyAt)
            .ThenBy(a => a.ScheduleSlot.StartTime)
            .ToListAsync();

    public void Add(Appointment appointment) => _db.Appointments.Add(appointment);

    public void AddAuditLog(AuditLog log) => _db.AuditLogs.Add(log);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    private IQueryable<Appointment> ReceptionQuery()
        => _db.Appointments
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Doctor)
                .ThenInclude(doctor => doctor.Staff)
                    .ThenInclude(staff => staff.User)
            .Include(appointment => appointment.Department)
            .Include(appointment => appointment.ScheduleSlot);
}
