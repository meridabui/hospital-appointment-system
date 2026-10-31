using Microsoft.AspNetCore.Components;

namespace HospitalAppointment.Client.Core;

public static class NavigationExtensions
{
    // Chuyển về trang login và nhớ trang hiện tại để quay lại sau khi đăng nhập
    public static void NavigateToLogin(this NavigationManager nav)
    {
        var relative = nav.ToBaseRelativePath(nav.Uri);
        if (relative.StartsWith("login", StringComparison.OrdinalIgnoreCase))
        {
            nav.NavigateTo("login");
            return;
        }

        nav.NavigateTo($"login?returnUrl={Uri.EscapeDataString("/" + relative)}");
    }
}