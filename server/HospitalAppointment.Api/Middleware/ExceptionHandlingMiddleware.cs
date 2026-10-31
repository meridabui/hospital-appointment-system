using HospitalAppointment.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteErrorAsync(context, StatusCodes.Status409Conflict,
                "This appointment was changed by someone else. Please refresh and try again.");
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
