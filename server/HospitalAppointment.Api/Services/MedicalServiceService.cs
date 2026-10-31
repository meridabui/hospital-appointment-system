
using HospitalAppointment.Api.DTOs.MedicalService;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Repositories;

namespace HospitalAppointment.Api.Services;

public class MedicalServiceService : IMedicalServiceService
{
    private readonly IMedicalServiceRepository _medicalServiceRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public MedicalServiceService(
        IMedicalServiceRepository medicalServiceRepository,
        IDepartmentRepository departmentRepository)
    {
        _medicalServiceRepository = medicalServiceRepository;
        _departmentRepository = departmentRepository;
    }

    // 1. GET ALL / SEARCH / FILTER

    public async Task<List<MedicalServiceDto>> GetAllAsync(
        string? keyword = null,
        int? departmentId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var services = await _medicalServiceRepository.GetAllAsync(
            keyword,
            departmentId,
            isActive,
            cancellationToken);

        return services.Select(MapToDto).ToList();
    }

    // 2. GET ACTIVE SERVICES

    public async Task<List<MedicalServiceDto>> GetActiveAsync(
        int? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var services = await _medicalServiceRepository.GetActiveAsync(
            departmentId,
            cancellationToken);

        return services.Select(MapToDto).ToList();
    }

    // 3. GET BY ID
    public async Task<MedicalServiceDto> GetByIdAsync(
        int medicalServiceId,
        CancellationToken cancellationToken = default)
    {
        if (medicalServiceId <= 0)
            throw new ArgumentException("Invalid medical service ID.");

        var service = await _medicalServiceRepository.GetByIdAsync(
            medicalServiceId,
            cancellationToken);

        if (service == null)
            throw new KeyNotFoundException("Medical service not found.");

        return MapToDto(service);
    }

    // 4. CREATE MEDICAL SERVICE

    public async Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto request,
        CancellationToken cancellationToken = default)
    {
        var serviceCode = request.ServiceCode.Trim().ToUpperInvariant();
        var serviceName = request.ServiceName.Trim();

        if (string.IsNullOrWhiteSpace(serviceCode))
            throw new ArgumentException("Service code is required.");

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name is required.");

        // Kiểm tra Department
        var department = await _departmentRepository.GetByIdAsync(
            request.DepartmentId,
            cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        if (!department.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot create a service under an inactive department.");
        }

        // Kiểm tra ServiceCode trùng
        if (await _medicalServiceRepository.ExistsByCodeAsync(
            serviceCode,
            cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                "Service code already exists.");
        }

        var service = new MedicalService
        {
            DepartmentId = request.DepartmentId,
            ServiceCode = serviceCode,
            ServiceName = serviceName,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        await _medicalServiceRepository.AddAsync(
            service,
            cancellationToken);

        await _medicalServiceRepository.SaveChangesAsync(
            cancellationToken);

        // Nạp lại Department để ánh xạ DTO
        var createdService = await _medicalServiceRepository.GetByIdAsync(
            service.MedicalServiceId,
            cancellationToken);

        return MapToDto(createdService!);
    }

    // 5. UPDATE MEDICAL SERVICE

    public async Task<MedicalServiceDto> UpdateAsync(
        int medicalServiceId,
        UpdateMedicalServiceDto request,
        CancellationToken cancellationToken = default)
    {
        if (medicalServiceId <= 0)
            throw new ArgumentException("Invalid medical service ID.");

        var service = await _medicalServiceRepository.GetByIdAsync(
            medicalServiceId,
            cancellationToken);

        if (service == null)
            throw new KeyNotFoundException("Medical service not found.");

        var serviceCode = request.ServiceCode.Trim().ToUpperInvariant();
        var serviceName = request.ServiceName.Trim();

        if (string.IsNullOrWhiteSpace(serviceCode))
            throw new ArgumentException("Service code is required.");

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name is required.");

        // Kiểm tra Department mới
        var department = await _departmentRepository.GetByIdAsync(
            request.DepartmentId,
            cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        if (!department.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot assign a service to an inactive department.");
        }

        // Kiểm tra ServiceCode trùng với dịch vụ khác
        if (await _medicalServiceRepository.ExistsByCodeAsync(
            serviceCode,
            medicalServiceId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "Service code already exists.");
        }

        service.DepartmentId = request.DepartmentId;
        service.ServiceCode = serviceCode;
        service.ServiceName = serviceName;
        service.Description = request.Description?.Trim();

        await _medicalServiceRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(service);
    }

    // 6. ACTIVATE / DEACTIVATE
    public async Task<MedicalServiceDto> UpdateStatusAsync(
        int medicalServiceId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (medicalServiceId <= 0)
            throw new ArgumentException("Invalid medical service ID.");

        var service = await _medicalServiceRepository.GetByIdAsync(
            medicalServiceId,
            cancellationToken);

        if (service == null)
            throw new KeyNotFoundException("Medical service not found.");

        // Không cho kích hoạt dịch vụ thuộc khoa đã vô hiệu hóa
        if (isActive && !service.Department.IsActive)
        {
            throw new InvalidOperationException(
                "Cannot activate a service under an inactive department.");
        }

        service.IsActive = isActive;

        await _medicalServiceRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(service);
    }

    // ENTITY -> DTO
    private static MedicalServiceDto MapToDto(
        MedicalService service)
    {
        return new MedicalServiceDto
        {
            MedicalServiceId = service.MedicalServiceId,
            DepartmentId = service.DepartmentId,
            DepartmentName = service.Department.DepartmentName,
            ServiceCode = service.ServiceCode,
            ServiceName = service.ServiceName,
            Description = service.Description,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt
        };
    }
}
