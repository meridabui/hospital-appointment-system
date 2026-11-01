using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Patients;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

// Dùng chung tiền tố api/appointments với AppointmentsController (đặt lịch)
[ApiController]
[Route("api/appointments")]
[Authorize(Roles = RoleNames.Patient)]
public class PatientAppointmentsController : ControllerBase
{
    private readonly IPatientAppointmentService _service;

    public PatientAppointmentsController(IPatientAppointmentService service) => _service = service;

    // Lịch sử đặt lịch: mọi trạng thái, mới nhất trước
    [HttpGet("my")]
    public async Task<ActionResult<List<MyAppointmentDto>>> GetMy()
        => Ok(await _service.GetMyAppointmentsAsync(User.GetUserId()));

    [HttpPost("{appointmentId:int}/cancel")]
    public async Task<ActionResult<CancelAppointmentResponse>> Cancel(int appointmentId, CancelAppointmentRequest request)
        => Ok(await _service.CancelAsync(User.GetUserId(), appointmentId, request));
}
