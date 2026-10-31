using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public static class SlotQueryExtensions
{
    public static IQueryable<DoctorScheduleSlot> AvailableForBooking(
        this IQueryable<DoctorScheduleSlot> query, DateOnly today, TimeOnly now, DateOnly toDate)
        => query.Where(s => s.IsActive
                         && s.SlotDate <= toDate
                         && (s.SlotDate > today || (s.SlotDate == today && s.StartTime > now))
                         && !s.Appointments.Any(a => a.IsActiveBooking));
}