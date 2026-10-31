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

    Task<IReadOnlyList<Appointment>> SearchForReceptionAsync(
        string? search,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken);

    Task<Appointment?> GetForReceptionUpdateAsync(int appointmentId, CancellationToken cancellationToken);

    void Add(Appointment appointment);
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

    public Task<Appointment?> GetForReceptionUpdateAsync(int appointmentId, CancellationToken cancellationToken)
        => ReceptionQuery().SingleOrDefaultAsync(
            appointment => appointment.AppointmentId == appointmentId,
            cancellationToken);

    public void Add(Appointment appointment) => _db.Appointments.Add(appointment);

    private IQueryable<Appointment> ReceptionQuery()
        => _db.Appointments
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Doctor)
                .ThenInclude(doctor => doctor.Staff)
                    .ThenInclude(staff => staff.User)
            .Include(appointment => appointment.Department)
            .Include(appointment => appointment.ScheduleSlot);
}
