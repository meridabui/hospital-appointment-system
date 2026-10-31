using HospitalAppointment.Client;
using HospitalAppointment.Client.Extensions;
using HospitalAppointment.Client.Api;
using HospitalAppointment.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddClientServices(builder.Configuration);
builder.Services.AddScoped<IAppointmentReceptionApiClient, AppointmentReceptionApiClient>();

// Register Services
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<NurseApiService>();
builder.Services.AddScoped<ExaminationApi>();
builder.Services.AddScoped<MedicalRecordApi>();

builder.Services.AddScoped<DepartmentApiService>();
builder.Services.AddScoped<MedicalServiceApiService>();
builder.Services.AddScoped<ScheduleApiService>();
await builder.Build().RunAsync();