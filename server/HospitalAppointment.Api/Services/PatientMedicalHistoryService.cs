using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.DTOs.Patients;
using HospitalAppointment.Api.DTOs.MedicalRecords; 
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Repositories;

namespace HospitalAppointment.Api.Services;

public interface IPatientMedicalHistoryService
{
    Task<List<MedicalHistoryItemDto>> GetMyHistoryAsync(int userId);
    Task<MedicalHistoryDetailDto> GetMyDetailAsync(int userId, int appointmentId);
}

public class PatientMedicalHistoryService : IPatientMedicalHistoryService
{
    private readonly IPatientRepository _patients;
    private readonly IMedicalRecordRepository _records;

    public PatientMedicalHistoryService(IPatientRepository patients, IMedicalRecordRepository records)
    {
        _patients = patients;
        _records = records;
    }

    public async Task<List<MedicalHistoryItemDto>> GetMyHistoryAsync(int userId)
    {
        var patient = await GetPatientAsync(userId);
        var records = await _records.GetFinalByPatientAsync(patient.PatientId);

        return records.Select(r =>
        {
            var dto = new MedicalHistoryItemDto();
            Fill(dto, r);
            return dto;
        }).ToList();
    }

    public async Task<MedicalHistoryDetailDto> GetMyDetailAsync(int userId, int appointmentId)
    {
        var patient = await GetPatientAsync(userId);

        // Lọc theo PatientId trong truy vấn: hồ sơ của người khác trả về "không tìm thấy"
        var record = await _records.GetFinalDetailAsync(appointmentId, patient.PatientId)
                     ?? throw new AppException("Không tìm thấy hồ sơ khám bệnh.", StatusCodes.Status404NotFound);

        var dto = new MedicalHistoryDetailDto();
        Fill(dto, record);

        if (record.Prescription != null)
        {
            dto.Prescription = new PrescriptionDto
            {
                Notes = record.Prescription.Notes,
                Items = record.Prescription.Items.Select(i => new PrescriptionItemDto
                {
                    PrescriptionItemId = i.PrescriptionItemId,
                    MedicationName = i.MedicationName,
                    Dosage = i.Dosage,
                    Route = i.Route,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Quantity = i.Quantity,
                    Instructions = i.Instructions
                }).ToList()
            };
        }

        dto.OrderedServices = record.OrderedServices.Select(o => new OrderedServiceDto
        {
            OrderedServiceId = o.OrderedServiceId,
            MedicalServiceId = o.MedicalServiceId,
            ServiceName = o.MedicalService.ServiceName,
            Notes = o.Notes,
            ServiceStatus = o.ServiceStatus.ToString()
        }).ToList();

        return dto;
    }

    private async Task<Patient> GetPatientAsync(int userId)
        => await _patients.GetActiveByUserIdAsync(userId)
           ?? throw new AppException("Tài khoản không có quyền thực hiện thao tác này.", StatusCodes.Status403Forbidden);

    private static void Fill(MedicalHistoryItemDto dto, MedicalRecord r)
    {
        dto.AppointmentId = r.AppointmentId;
        dto.AppointmentCode = r.Appointment.AppointmentCode;
        dto.MedicalRecordId = r.MedicalRecordId;
        dto.Date = r.Appointment.ScheduleSlot.SlotDate;
        dto.StartTime = r.Appointment.ScheduleSlot.StartTime;
        dto.EndTime = r.Appointment.ScheduleSlot.EndTime;
        dto.DepartmentName = r.Appointment.Department.DepartmentName;
        dto.DoctorName = r.Doctor.Staff.User.FullName;
        dto.Specialty = r.Doctor.Specialty;
        dto.Diagnosis = r.Diagnosis;
        dto.Result = r.Result;
        dto.TreatmentNotes = r.TreatmentNotes;
        dto.FinalizedAt = r.FinalizedAt;
    }
}
