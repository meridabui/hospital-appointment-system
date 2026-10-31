using HospitalAppointment.Client;
using HospitalAppointment.Client.Services.Implementations;
using HospitalAppointment.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Đăng ký Api Client cho Luồng 4: Tiếp nhận & Check-in
builder.Services.AddScoped<IAppointmentReceptionApiClient, AppointmentReceptionApiClient>();

await builder.Build().RunAsync();
