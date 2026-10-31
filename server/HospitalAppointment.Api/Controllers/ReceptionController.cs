using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Reception;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Services.Reception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/reception/appointments")]
[Authorize(Roles = RoleNames.Receptionist)]
public sealed class ReceptionController : ControllerBase
{
    private readonly IReceptionService _receptionService;

    public ReceptionController(IReceptionService receptionService)
    {
        _receptionService = receptionService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ReceptionAppointmentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReceptionAppointmentDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] DateOnly? date,
        [FromQuery] AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var appointments = await _receptionService.SearchAsync(search, date, status, cancellationToken);
        return Ok(appointments);
    }

    [HttpPost("{appointmentId:int}/check-in")]
    public async Task<ActionResult<ReceptionAppointmentDto>> CheckIn(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _receptionService.CheckInAsync(appointmentId, userId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{appointmentId:int}/no-show")]
    public async Task<ActionResult<ReceptionAppointmentDto>> MarkNoShow(
        int appointmentId,
        [FromBody] MarkNoShowRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _receptionService.MarkNoShowAsync(
            appointmentId,
            userId,
            request.Reason,
            cancellationToken);

        return ToActionResult(result);
    }

    private ActionResult<ReceptionAppointmentDto> ToActionResult(ReceptionOperationResult result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Appointment);
        }

        var problem = new ProblemDetails { Detail = result.Error };
        return result.Failure switch
        {
            ReceptionFailure.NotFound => NotFound(problem),
            ReceptionFailure.ConcurrencyConflict => Conflict(problem),
            ReceptionFailure.InvalidState => Conflict(problem),
            ReceptionFailure.TooEarlyForNoShow => BadRequest(problem),
            _ => BadRequest(problem)
        };
    }
}
