using System.Security.Claims;

namespace HospitalAppointment.Api.Common.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
        => int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("Token không có UserId."));
}