using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Examination;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/examination")]
[Authorize(Roles = RoleNames.Doctor)]
public class ExaminationController : ControllerBase
{
    private readonly IExaminationService _examinationService;

    public ExaminationController(IExaminationService examinationService)
    {
        _examinationService = examinationService;
    }

    [HttpGet("ready-patients")]
    public async Task<ActionResult<List<ReadyPatientDto>>> GetReadyPatients()
    {
        return Ok(await _examinationService.GetReadyPatientsAsync(User.GetUserId()));
    }

    [HttpPost("{appointmentId:int}/start")]
    public async Task<IActionResult> StartConsultation(int appointmentId)
    {
        await _examinationService.StartConsultationAsync(appointmentId, User.GetUserId());
        return NoContent();
    }
}
