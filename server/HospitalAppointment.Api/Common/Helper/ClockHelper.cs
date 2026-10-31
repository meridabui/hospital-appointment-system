namespace HospitalAppointment.Api.Common.Helpers;

public static class ClockHelper
{
    // Việt Nam không có DST nên cộng cố định 7 giờ
    public static DateTime VietnamNow => DateTime.UtcNow.AddHours(7);
}