namespace HospitalAppointment.Api.DTOs.Catalog;

public class AvailableDateDto
{
    public DateOnly Date { get; set; }
    public int AvailableSlotCount { get; set; }
}