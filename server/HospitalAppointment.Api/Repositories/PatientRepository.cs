using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByUserIdAsync(int userId);
    void Add(Patient patient);
}

public class PatientRepository : IPatientRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public PatientRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<Patient?> GetByUserIdAsync(int userId)
        => _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

    public void Add(Patient patient) => _db.Patients.Add(patient);
}