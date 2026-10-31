namespace HospitalAppointment.Client.Core;

public static class ViVn
{
    public static string Weekday(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Thứ Hai",
        DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư",
        DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu",
        DayOfWeek.Saturday => "Thứ Bảy",
        _ => "Chủ nhật"
    };

    public static string FormatDate(DateOnly date) => $"{Weekday(date.DayOfWeek)}, {date:dd/MM/yyyy}";

    public static string FormatTime(TimeOnly time) => time.ToString("HH:mm");
}