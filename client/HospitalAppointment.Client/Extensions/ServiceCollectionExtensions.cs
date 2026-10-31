using HospitalAppointment.Client.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalAppointment.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
            throw new InvalidOperationException("Thiếu 'ApiBaseUrl' trong wwwroot/appsettings.json.");
        if (!apiBaseUrl.EndsWith('/')) apiBaseUrl += "/";

        services.AddScoped<AuthSession>();
        services.AddScoped(sp => new ApiClient(
            new HttpClient { BaseAddress = new Uri(apiBaseUrl) },
            sp.GetRequiredService<AuthSession>(),
            sp.GetRequiredService<NavigationManager>()));

        return services;
    }
}