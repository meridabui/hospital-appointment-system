using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models;

namespace HospitalAppointment.Client.Services;

public class PatientPortalApiService
{
    private readonly ApiClient _api;

    public PatientPortalApiService(ApiClient api) => _api = api;

    // Lịch sử đặt lịch (mọi trạng thái, mới nhất trước)
    public Task<ApiResult<List<MyAppointmentModel>>> GetMyAppointmentsAsync()
        => _api.GetAsync<List<MyAppointmentModel>>("api/appointments/my");

    public Task<ApiResult<CancelAppointmentResult>> CancelAppointmentAsync(int appointmentId, string? reason)
        => _api.PostAsync<CancelAppointmentModel, CancelAppointmentResult>(
            $"api/appointments/{appointmentId}/cancel",
            new CancelAppointmentModel { Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim() });

    // Lịch sử khám bệnh (chỉ hồ sơ đã hoàn tất)
    public Task<ApiResult<List<MedicalHistoryItemModel>>> GetMyMedicalHistoryAsync()
        => _api.GetAsync<List<MedicalHistoryItemModel>>("api/medical-history/my");

    public Task<ApiResult<MedicalHistoryDetailModel>> GetMyMedicalHistoryDetailAsync(int appointmentId)
        => _api.GetAsync<MedicalHistoryDetailModel>($"api/medical-history/my/{appointmentId}");
}
