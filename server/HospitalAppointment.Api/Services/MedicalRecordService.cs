using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.DTOs.MedicalRecords;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using Microsoft.EntityFrameworkCore;

// Có 2 IAppointmentRepository; luồng khám dùng bản Interfaces (có FindByIdAsync)
using IAppointmentRepository = HospitalAppointment.Api.Repositories.IAppointmentRepository;

namespace HospitalAppointment.Api.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordDto> GetAsync(int appointmentId, int userId);
    Task<MedicalRecordDto> SaveAsync(int appointmentId, SaveMedicalRecordRequest request, int userId);
    Task<MedicalRecordDto> CompleteConsultationAsync(int appointmentId, int userId);
    Task<List<CompletedVisitDto>> GetCompletedVisitsAsync(string? keyword, DateOnly? date, int userId);
    Task<List<ActiveMedicalServiceDto>> GetActiveServicesAsync();
}

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IMedicalServiceRepository _medicalServiceRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public MedicalRecordService(
        IMedicalRecordRepository medicalRecordRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IMedicalServiceRepository medicalServiceRepository,
        IAuditLogRepository auditLogRepository)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _medicalServiceRepository = medicalServiceRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<MedicalRecordDto> GetAsync(int appointmentId, int userId)
    {
        var doctor = await GetActiveDoctorAsync(userId);
        var appointment = await GetAssignedAppointmentAsync(appointmentId, doctor.DoctorId);
        var record = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId);

        return ToDto(appointment, record);
    }

    public async Task<MedicalRecordDto> SaveAsync(int appointmentId, SaveMedicalRecordRequest request, int userId)
    {
        var doctor = await GetActiveDoctorAsync(userId);
        var appointment = await GetAssignedAppointmentAsync(appointmentId, doctor.DoctorId);

        if (appointment.Status != AppointmentStatus.InConsultation)
        {
            throw new AppException(
                $"Chỉ được lưu hồ sơ khi đang khám. Trạng thái hiện tại: {appointment.Status}.",
                StatusCodes.Status409Conflict);
        }

        var record = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId);
        if (record is null)
        {
            record = new MedicalRecord
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                DoctorId = doctor.DoctorId,
                CreatedAt = DateTime.UtcNow
            };
            _medicalRecordRepository.Add(record);
        }
        else if (!record.CanEdit())
        {
            throw new AppException("Hồ sơ đã hoàn tất, không thể chỉnh sửa.", StatusCodes.Status409Conflict);
        }

        record.UpdateContent(request.Diagnosis, request.Result, request.TreatmentNotes);

        if (request.Prescription is not null)
        {
            SyncPrescription(record, request.Prescription);
        }

        if (request.OrderedServices is not null)
        {
            await SyncOrderedServicesAsync(record, request.OrderedServices, doctor.DoctorId);
        }

        _auditLogRepository.Add(new AuditLog
        {
            UserId = userId,
            Action = "SaveMedicalRecord",
            EntityName = "MedicalRecord",
            EntityId = appointment.AppointmentId.ToString(),
            Details = $"Medical record of appointment {appointment.AppointmentCode} saved as draft",
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            // Hồ sơ, đơn thuốc, chỉ định và audit log được lưu trong cùng một transaction
            await _medicalRecordRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new AppException(
                "Hồ sơ vừa được thay đổi ở nơi khác. Vui lòng tải lại trang.", StatusCodes.Status409Conflict);
        }

        // Nạp lại để có tên dịch vụ của các chỉ định vừa thêm
        record = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId);

        return ToDto(appointment, record);
    }

    public async Task<MedicalRecordDto> CompleteConsultationAsync(int appointmentId, int userId)
    {
        var doctor = await GetActiveDoctorAsync(userId);

        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId)
            ?? throw new AppException("Không tìm thấy lịch hẹn.", StatusCodes.Status404NotFound);

        if (appointment.DoctorId != doctor.DoctorId)
        {
            throw new AppException("Lịch hẹn này không được phân công cho bạn.", StatusCodes.Status403Forbidden);
        }

        if (!appointment.CanComplete())
        {
            throw new AppException(
                $"Không thể hoàn tất. Trạng thái hiện tại: {appointment.Status}.", StatusCodes.Status409Conflict);
        }

        var record = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId)
            ?? throw new AppException("Chưa có hồ sơ khám. Vui lòng lưu hồ sơ trước.", StatusCodes.Status404NotFound);

        if (!record.IsComplete())
        {
            throw new AppException("Hồ sơ thiếu chẩn đoán hoặc ghi chú điều trị. Vui lòng bổ sung và lưu lại.");
        }

        record.FinalizeRecord(userId);
        appointment.Complete(userId);

        _auditLogRepository.Add(new AuditLog
        {
            UserId = userId,
            Action = "CompleteConsultation",
            EntityName = "Appointment",
            EntityId = appointment.AppointmentId.ToString(),
            Details = $"Appointment {appointment.AppointmentCode} completed",
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            // Cùng DbContext nên một lần lưu là một transaction: hồ sơ, lịch hẹn và audit log
            await _medicalRecordRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new AppException(
                "Lịch hẹn vừa được thay đổi ở nơi khác. Vui lòng tải lại trang.", StatusCodes.Status409Conflict);
        }

        return ToDto(appointment, record);
    }

    public async Task<List<CompletedVisitDto>> GetCompletedVisitsAsync(string? keyword, DateOnly? date, int userId)
    {
        var doctor = await GetActiveDoctorAsync(userId);
        var appointments = await _medicalRecordRepository.GetCompletedAppointmentsAsync(doctor.DoctorId, keyword, date);

        return appointments.Select(x => new CompletedVisitDto
        {
            AppointmentId = x.AppointmentId,
            AppointmentCode = x.AppointmentCode,
            SlotDate = x.ScheduleSlot.SlotDate,
            StartTime = x.ScheduleSlot.StartTime,
            EndTime = x.ScheduleSlot.EndTime,
            DepartmentName = x.Department.DepartmentName,
            PatientCode = x.Patient.PatientCode,
            PatientName = x.Patient.FullName
        }).ToList();
    }

    public async Task<List<ActiveMedicalServiceDto>> GetActiveServicesAsync()
    {
        var services = await _medicalServiceRepository.GetActiveAsync();

        return services.Select(x => new ActiveMedicalServiceDto
        {
            MedicalServiceId = x.MedicalServiceId,
            ServiceCode = x.ServiceCode,
            ServiceName = x.ServiceName,
            DepartmentName = x.Department.DepartmentName
        }).ToList();
    }

    private void SyncPrescription(MedicalRecord record, PrescriptionRequest request)
    {
        if (request.Items.Count == 0)
        {
            if (record.Prescription is not null)
            {
                _medicalRecordRepository.RemovePrescription(record.Prescription);
                record.Prescription = null;
            }

            return;
        }

        record.Prescription ??= new Prescription { PrescribedAt = DateTime.UtcNow };
        record.Prescription.Notes = request.Notes;

        var existingItems = record.Prescription.Items.ToList();

        foreach (var item in existingItems.Where(x => !request.Items.Any(r => r.PrescriptionItemId == x.PrescriptionItemId)))
        {
            _medicalRecordRepository.RemovePrescriptionItem(item);
            record.Prescription.Items.Remove(item);
        }

        foreach (var itemRequest in request.Items)
        {
            var item = new PrescriptionItem();
            if (itemRequest.PrescriptionItemId is { } itemId)
            {
                item = existingItems.FirstOrDefault(x => x.PrescriptionItemId == itemId)
                    ?? throw new AppException("Dòng thuốc không thuộc hồ sơ này.");
            }
            else
            {
                record.Prescription.Items.Add(item);
            }

            item.MedicationName = itemRequest.MedicationName.Trim();
            item.Dosage = itemRequest.Dosage;
            item.Route = itemRequest.Route;
            item.Frequency = itemRequest.Frequency;
            item.Duration = itemRequest.Duration;
            item.Quantity = itemRequest.Quantity;
            item.Instructions = itemRequest.Instructions;
        }
    }

    private async Task SyncOrderedServicesAsync(MedicalRecord record, List<OrderedServiceRequest> requests, int doctorId)
    {
        if (requests.Select(x => x.MedicalServiceId).Distinct().Count() != requests.Count)
        {
            throw new AppException("Một dịch vụ chỉ được chỉ định một lần.");
        }

        var activeServices = await _medicalServiceRepository.GetActiveAsync();
        var existingOrders = record.OrderedServices.ToList();

        // Chỉ bỏ được chỉ định chưa thực hiện; InProgress và Completed luôn được giữ lại
        foreach (var order in existingOrders.Where(x => x.ServiceStatus == ServiceStatus.Ordered
            && !requests.Any(r => r.OrderedServiceId == x.OrderedServiceId)))
        {
            _medicalRecordRepository.RemoveOrderedService(order);
            record.OrderedServices.Remove(order);
        }

        foreach (var orderRequest in requests)
        {
            if (orderRequest.OrderedServiceId is { } orderId)
            {
                var order = existingOrders.FirstOrDefault(x => x.OrderedServiceId == orderId)
                    ?? throw new AppException("Chỉ định không thuộc hồ sơ này.");

                if (order.MedicalServiceId != orderRequest.MedicalServiceId)
                {
                    throw new AppException("Không thể đổi dịch vụ của chỉ định đã có.");
                }

                if (order.ServiceStatus == ServiceStatus.Ordered)
                {
                    order.Notes = orderRequest.Notes;
                }

                continue;
            }

            var service = activeServices.FirstOrDefault(x => x.MedicalServiceId == orderRequest.MedicalServiceId)
                ?? throw new AppException("Dịch vụ không tồn tại hoặc đã ngừng hoạt động.");

            record.OrderedServices.Add(new OrderedService
            {
                MedicalServiceId = service.MedicalServiceId,
                OrderedByDoctorId = doctorId,
                OrderedAt = DateTime.UtcNow,
                Notes = orderRequest.Notes,
                ServiceStatus = ServiceStatus.Ordered
            });
        }
    }

    private async Task<Doctor> GetActiveDoctorAsync(int userId)
    {
        return await _doctorRepository.GetActiveByUserIdAsync(userId)
            ?? throw new AppException("Tài khoản của bạn không phải bác sĩ đang hoạt động.", StatusCodes.Status403Forbidden);
    }

    private async Task<Appointment> GetAssignedAppointmentAsync(int appointmentId, int doctorId)
    {
        var appointment = await _medicalRecordRepository.GetAppointmentAsync(appointmentId)
            ?? throw new AppException("Không tìm thấy lịch hẹn.", StatusCodes.Status404NotFound);

        if (appointment.DoctorId != doctorId)
        {
            throw new AppException("Lịch hẹn này không được phân công cho bạn.", StatusCodes.Status403Forbidden);
        }

        return appointment;
    }

    private static MedicalRecordDto ToDto(Appointment appointment, MedicalRecord? record)
    {
        return new MedicalRecordDto
        {
            AppointmentId = appointment.AppointmentId,
            AppointmentCode = appointment.AppointmentCode,
            AppointmentStatus = appointment.Status.ToString(),
            BookingReason = appointment.BookingReason,
            SlotDate = appointment.ScheduleSlot.SlotDate,
            StartTime = appointment.ScheduleSlot.StartTime,
            EndTime = appointment.ScheduleSlot.EndTime,
            DepartmentName = appointment.Department.DepartmentName,
            PatientCode = appointment.Patient.PatientCode,
            PatientName = appointment.Patient.FullName,
            DateOfBirth = appointment.Patient.DateOfBirth,
            Gender = appointment.Patient.Gender,
            MedicalRecordId = record?.MedicalRecordId,
            RecordStatus = record?.RecordStatus.ToString(),
            Diagnosis = record?.Diagnosis,
            Result = record?.Result,
            TreatmentNotes = record?.TreatmentNotes,
            UpdatedAt = record?.UpdatedAt,
            Prescription = record?.Prescription is null ? null : new PrescriptionDto
            {
                Notes = record.Prescription.Notes,
                Items = record.Prescription.Items.OrderBy(x => x.PrescriptionItemId).Select(x => new PrescriptionItemDto
                {
                    PrescriptionItemId = x.PrescriptionItemId,
                    MedicationName = x.MedicationName,
                    Dosage = x.Dosage,
                    Route = x.Route,
                    Frequency = x.Frequency,
                    Duration = x.Duration,
                    Quantity = x.Quantity,
                    Instructions = x.Instructions
                }).ToList()
            },
            OrderedServices = record?.OrderedServices.OrderBy(x => x.OrderedServiceId).Select(x => new OrderedServiceDto
            {
                OrderedServiceId = x.OrderedServiceId,
                MedicalServiceId = x.MedicalServiceId,
                ServiceName = x.MedicalService.ServiceName,
                Notes = x.Notes,
                ServiceStatus = x.ServiceStatus.ToString()
            }).ToList() ?? new()
        };
    }
}
