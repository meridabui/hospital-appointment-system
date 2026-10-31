using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Extensions;
using HospitalAppointment.Api.Middlewares;
using HospitalAppointment.Api.Services.Implementations;
using HospitalAppointment.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<HospitalAppointmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services Registration (DI) - Luồng Tiếp nhận & Check-in
builder.Services.AddScoped<IAppointmentReceptionService, AppointmentReceptionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT, CORS (policy "Client"), Swagger Authorize, và DI các service/repository còn lại
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(ServiceCollectionExtensions.CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.SeedDevDataAsync();

app.Run();