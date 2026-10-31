using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
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

    public void Add(Appointment appointment) => _db.Appointments.Add(appointment);
}
