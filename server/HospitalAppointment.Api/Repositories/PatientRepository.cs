using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByUserIdAsync(int userId);
    void Add(Patient patient);
    Task<Patient?> GetActiveByUserIdAsync(int userId);
}

public class PatientRepository : IPatientRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public PatientRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<Patient?> GetByUserIdAsync(int userId)
        => _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

    public void Add(Patient patient) => _db.Patients.Add(patient);
    public Task<Patient?> GetActiveByUserIdAsync(int userId)
    => _db.Patients.AsNoTracking()
        .FirstOrDefaultAsync(p => p.UserId == userId && p.User != null && p.User.IsActive);
}