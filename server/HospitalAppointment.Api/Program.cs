using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Repositories.Implementations;
using HospitalAppointment.Api.Repositories.Interfaces;
using HospitalAppointment.Api.Services.Implementations;
using HospitalAppointment.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using HospitalAppointment.Api.Extensions;     
using HospitalAppointment.Api.Middlewares;     

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<HospitalAppointmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services Registration (DI)
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentReceptionService, AppointmentReceptionService>();

// CORS configuration for Blazor WebAssembly client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseCors(ServiceCollectionExtensions.CorsPolicy);

app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

await app.SeedDevDataAsync();

app.Run();
