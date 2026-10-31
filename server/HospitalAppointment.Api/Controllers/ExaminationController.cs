using HospitalAppointment.Api.DTOs.Examination;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/examination")]
public class ExaminationController : ControllerBase
{
    // TODO: remove when Auth is merged. Add [Authorize(Roles = "Doctor")] and read UserId from the JWT.
    private const int TemporaryDoctorUserId = 10;

    private readonly IExaminationService _examinationService;

    public ExaminationController(IExaminationService examinationService)
    {
        _examinationService = examinationService;
    }

    [HttpGet("ready-patients")]
    public async Task<ActionResult<List<ReadyPatientDto>>> GetReadyPatients([FromQuery] int userId = TemporaryDoctorUserId)
    {
        return Ok(await _examinationService.GetReadyPatientsAsync(userId));
    }

    [HttpPost("{appointmentId:int}/start")]
    public async Task<IActionResult> StartConsultation(int appointmentId, [FromQuery] int userId = TemporaryDoctorUserId)
    {
        await _examinationService.StartConsultationAsync(appointmentId, userId);
        return NoContent();
    }
}
