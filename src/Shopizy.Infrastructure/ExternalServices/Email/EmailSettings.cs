namespace Shopizy.Infrastructure.ExternalServices.Email;

public class EmailSettings
{
    public const string Section = "EmailSettings";

    public bool EnableRealEmail { get; set; }
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public string SenderEmail { get; set; } = "noreply@shopizy.com";
    public string SenderName { get; set; } = "Shopizy Store";
    public bool EnableSsl { get; set; }
}
