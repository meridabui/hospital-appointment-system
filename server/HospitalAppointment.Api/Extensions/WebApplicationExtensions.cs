using HospitalAppointment.Api.Data;

namespace HospitalAppointment.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDevDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HospitalAppointmentDbContext>();
        await DevDataSeeder.SeedAsync(db);
    }
}