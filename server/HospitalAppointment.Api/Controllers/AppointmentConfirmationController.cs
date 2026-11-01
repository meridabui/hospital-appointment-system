using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Confirmation;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/appointment-confirmations")]
[Authorize(Roles = RoleNames.Receptionist)]
public sealed class AppointmentConfirmationController : ControllerBase
{
    private readonly IAppointmentConfirmationService _confirmationService;

    public AppointmentConfirmationController(IAppointmentConfirmationService confirmationService)
    {
        _confirmationService = confirmationService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<ConfirmationAppointmentDto>>> GetPending(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
        => Ok(await _confirmationService.GetPendingAsync(date, cancellationToken));

    [HttpPost("{appointmentId:int}/confirm")]
    public async Task<ActionResult<ConfirmationAppointmentDto>> Confirm(
        int appointmentId,
        CancellationToken cancellationToken)
        => ToActionResult(await _confirmationService.ConfirmAsync(
            appointmentId, User.GetUserId(), cancellationToken));

    [HttpPost("{appointmentId:int}/reject")]
    public async Task<ActionResult<ConfirmationAppointmentDto>> Reject(
        int appointmentId,
        [FromBody] RejectAppointmentRequest request,
        CancellationToken cancellationToken)
        => ToActionResult(await _confirmationService.RejectAsync(
            appointmentId, User.GetUserId(), request.Reason, cancellationToken));

    private ActionResult<ConfirmationAppointmentDto> ToActionResult(ConfirmationOperationResult result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Appointment);
        }

        var problem = new ProblemDetails { Detail = result.Error };
        return result.Failure switch
        {
            ConfirmationFailure.NotFound => NotFound(problem),
            ConfirmationFailure.InvalidState => Conflict(problem),
            _ => BadRequest(problem)
        };
    }
}
