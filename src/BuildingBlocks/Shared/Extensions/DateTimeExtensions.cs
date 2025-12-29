namespace Shared.Extensions;

public static class DateTimeExtensions
{
    public static string ToFriendlyDate(this DateTime date)
    {
        var ts = DateTime.Now - date;
        
        if (ts.TotalMinutes < 1)
            return "just now";
        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes} minutes ago";
        if (ts.TotalHours < 24)
            return $"{(int)ts.TotalHours} hours ago";
        if (ts.TotalDays < 30)
            return $"{(int)ts.TotalDays} days ago";
        
        return date.ToString("MMMM dd, yyyy");
    }

    public static bool IsToday(this DateTime date)
    {
        return date.Date == DateTime.Today;
    }

    public static bool IsYesterday(this DateTime date)
    {
        return date.Date == DateTime.Today.AddDays(-1);
    }
}
