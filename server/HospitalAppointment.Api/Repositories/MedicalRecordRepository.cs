using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IMedicalRecordRepository
{
    Task<Appointment?> GetAppointmentAsync(int appointmentId);
    Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);
    Task<List<Appointment>> GetCompletedAppointmentsAsync(int doctorId, string? keyword, DateOnly? date);
    void Add(MedicalRecord record);
    void RemovePrescription(Prescription prescription);
    void RemovePrescriptionItem(PrescriptionItem item);
    void RemoveOrderedService(OrderedService orderedService);
    Task SaveChangesAsync();
    Task<List<MedicalRecord>> GetFinalByPatientAsync(int patientId);
    Task<MedicalRecord?> GetFinalDetailAsync(int appointmentId, int patientId);
}

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public MedicalRecordRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Appointment?> GetAppointmentAsync(int appointmentId)
    {
        return _context.Appointments
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.ScheduleSlot)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
    }

    public Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId)
    {
        return _context.MedicalRecords
            .Include(x => x.Prescription!).ThenInclude(x => x.Items)
            .Include(x => x.OrderedServices).ThenInclude(x => x.MedicalService)
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
    }

    public Task<List<Appointment>> GetCompletedAppointmentsAsync(int doctorId, string? keyword, DateOnly? date)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Department)
            .Include(x => x.ScheduleSlot)
            .Where(x => x.DoctorId == doctorId && x.Status == AppointmentStatus.Completed);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x => x.AppointmentCode.Contains(term)
                || x.Patient.FullName.Contains(term)
                || x.Patient.PatientCode.Contains(term));
        }

        if (date.HasValue)
        {
            query = query.Where(x => x.ScheduleSlot.SlotDate == date.Value);
        }

        return query
            .OrderByDescending(x => x.ScheduleSlot.SlotDate)
            .ThenByDescending(x => x.ScheduleSlot.StartTime)
            .ToListAsync();
    }

    public void Add(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);
    }

    public void RemovePrescription(Prescription prescription)
    {
        _context.PrescriptionItems.RemoveRange(prescription.Items);
        _context.Prescriptions.Remove(prescription);
    }

    public void RemovePrescriptionItem(PrescriptionItem item)
    {
        _context.PrescriptionItems.Remove(item);
    }

    public void RemoveOrderedService(OrderedService orderedService)
    {
        _context.OrderedServices.Remove(orderedService);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
    public Task<List<MedicalRecord>> GetFinalByPatientAsync(int patientId)
    {
        return _context.MedicalRecords
            .AsNoTracking()
            .Include(r => r.Appointment).ThenInclude(a => a.ScheduleSlot)
            .Include(r => r.Appointment).ThenInclude(a => a.Department)
            .Include(r => r.Doctor).ThenInclude(d => d.Staff).ThenInclude(s => s.User)
            .Where(r => r.PatientId == patientId && r.RecordStatus == RecordStatus.Final)
            .OrderByDescending(r => r.Appointment.ScheduleSlot.SlotDate)
            .ThenByDescending(r => r.Appointment.ScheduleSlot.StartTime)
            .ToListAsync();
    }

    public Task<MedicalRecord?> GetFinalDetailAsync(int appointmentId, int patientId)
    {
        return _context.MedicalRecords
            .AsNoTracking()
            .Include(r => r.Appointment).ThenInclude(a => a.ScheduleSlot)
            .Include(r => r.Appointment).ThenInclude(a => a.Department)
            .Include(r => r.Doctor).ThenInclude(d => d.Staff).ThenInclude(s => s.User)
            .Include(r => r.Prescription!).ThenInclude(p => p.Items)
            .Include(r => r.OrderedServices).ThenInclude(o => o.MedicalService)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId
                                   && r.PatientId == patientId
                                   && r.RecordStatus == RecordStatus.Final);
    }
}
