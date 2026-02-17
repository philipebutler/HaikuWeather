using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent;

public class EmailClient
{
    private readonly EmailConfig _config;

    public EmailClient(EmailConfig config)
    {
        _config = config;
    }

    public async Task SendHaikuEmailAsync(
        string haiku,
        WeatherContext context,
        string triggerReason)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config.From));
        message.To.Add(MailboxAddress.Parse(_config.To));

        var subject = context.TriggerType == "extreme"
            ? $"[{_config.SubjectPrefix}] Extreme: {context.TemperatureF:F0}°F in {context.Location} — {context.Persona}"
            : $"[{_config.SubjectPrefix}] {context.TemperatureF:F0}°F — {context.Persona}";
        
        message.Subject = subject;

        var body = BuildEmailBody(haiku, context, triggerReason);
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        
        try
        {
            await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config.Username, _config.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            Console.WriteLine($"✓ Email sent successfully to {_config.To}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to send email: {ex.Message}");
            throw;
        }
    }

    private string BuildEmailBody(string haiku, WeatherContext context, string triggerReason)
    {
        return $@"
{haiku}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Persona: {context.Persona}
Temperature: {context.TemperatureF:F1}°F
Location: {context.Location}
Timestamp: {context.Timestamp:yyyy-MM-dd HH:mm:ss}
Trigger: {triggerReason}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Sent by WeatherHaikuAgent
";
    }
}
