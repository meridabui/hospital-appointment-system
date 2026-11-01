
using HospitalAppointment.Client.Models.Nurse;
using HospitalAppointment.Client.Core;

namespace HospitalAppointment.Client.Services;

public class NurseApiService
{
    private readonly ApiClient _api;

    public NurseApiService(ApiClient api)
    {
        _api = api;
    }

    // 1. Lấy danh sách bệnh nhân đã Check-in
    public Task<ApiResult<List<NurseAppointmentDto>>> GetCheckedInAppointmentsAsync()
        => _api.GetAsync<List<NurseAppointmentDto>>("api/Nurse/appointments/checked-in");

    // 2. Lấy chi tiết Appointment
    public Task<ApiResult<NurseAppointmentDto>> GetAppointmentDetailAsync(int appointmentId)
        => _api.GetAsync<NurseAppointmentDto>($"api/Nurse/appointments/{appointmentId}");

    // 3. Xác nhận bệnh nhân sẵn sàng khám
    public Task<ApiResult<MarkReadyResponseDto>> MarkReadyAsync(int appointmentId)
        => _api.PostAsync<MarkReadyResponseDto>($"api/Nurse/appointments/{appointmentId}/mark-ready");
}
