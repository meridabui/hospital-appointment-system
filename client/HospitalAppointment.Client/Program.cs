using HospitalAppointment.Client;
using HospitalAppointment.Client.Services.Implementations;
using HospitalAppointment.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HospitalAppointment.Client.Extensions;                          // + 1

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddClientServices(builder.Configuration);            // + 2

// Đăng ký Api Client cho Luồng 4: Tiếp nhận & Check-in
builder.Services.AddScoped<IAppointmentReceptionApiClient, AppointmentReceptionApiClient>();

await builder.Build().RunAsync();
