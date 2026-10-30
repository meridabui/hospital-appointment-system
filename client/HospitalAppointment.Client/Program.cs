using HospitalAppointment.Client;
using HospitalAppointment.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API Base Address
builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7243/")
    });

// Register Services
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<NurseApiService>();

await builder.Build().RunAsync();