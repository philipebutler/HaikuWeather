using Microsoft.Extensions.Configuration;
using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent;

public class ConfigLoader
{
    public static AppConfig Load()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        var config = new AppConfig();
        configuration.Bind(config);

        OverrideFromEnvironmentVariables(config);

        return config;
    }

    private static void OverrideFromEnvironmentVariables(AppConfig config)
    {
        if (double.TryParse(Environment.GetEnvironmentVariable("Local__Latitude"), out var lat))
            config.Local.Latitude = lat;
        
        if (double.TryParse(Environment.GetEnvironmentVariable("Local__Longitude"), out var lon))
            config.Local.Longitude = lon;
        
        config.Local.LocationLabel = Environment.GetEnvironmentVariable("Local__LocationLabel") 
            ?? config.Local.LocationLabel;

        if (double.TryParse(Environment.GetEnvironmentVariable("Notify__TempDeltaF"), out var delta))
            config.Notify.TempDeltaF = delta;

        if (int.TryParse(Environment.GetEnvironmentVariable("Notify__MinMinutesBetween"), out var mins))
            config.Notify.MinMinutesBetween = mins;

        if (int.TryParse(Environment.GetEnvironmentVariable("Notify__QuietHoursStart"), out var qhStart))
            config.Notify.QuietHoursStart = qhStart;

        if (int.TryParse(Environment.GetEnvironmentVariable("Notify__QuietHoursEnd"), out var qhEnd))
            config.Notify.QuietHoursEnd = qhEnd;

        if (bool.TryParse(Environment.GetEnvironmentVariable("Notify__AllowQuietHoursOverride"), out var qhOverride))
            config.Notify.AllowQuietHoursOverride = qhOverride;

        if (bool.TryParse(Environment.GetEnvironmentVariable("Extreme__Enabled"), out var extremeEnabled))
            config.Extreme.Enabled = extremeEnabled;

        config.Extreme.DailySendTimeLocal = Environment.GetEnvironmentVariable("Extreme__DailySendTimeLocal") 
            ?? config.Extreme.DailySendTimeLocal;

        config.Extreme.SelectionMode = Environment.GetEnvironmentVariable("Extreme__SelectionMode") 
            ?? config.Extreme.SelectionMode;

        if (double.TryParse(Environment.GetEnvironmentVariable("Extreme__ReferenceTempF"), out var refTemp))
            config.Extreme.ReferenceTempF = refTemp;

        config.Haiku.Mode = Environment.GetEnvironmentVariable("Haiku__Mode") 
            ?? config.Haiku.Mode;

        config.Haiku.TemplateRotationMode = Environment.GetEnvironmentVariable("Haiku__TemplateRotationMode") 
            ?? config.Haiku.TemplateRotationMode;

        config.Email.SmtpHost = Environment.GetEnvironmentVariable("Email__SmtpHost") 
            ?? config.Email.SmtpHost;

        if (int.TryParse(Environment.GetEnvironmentVariable("Email__SmtpPort"), out var port))
            config.Email.SmtpPort = port;

        config.Email.Username = Environment.GetEnvironmentVariable("Email__Username") 
            ?? config.Email.Username;

        config.Email.Password = Environment.GetEnvironmentVariable("Email__Password") 
            ?? config.Email.Password;

        config.Email.From = Environment.GetEnvironmentVariable("Email__From") 
            ?? config.Email.From;

        config.Email.To = Environment.GetEnvironmentVariable("Email__To") 
            ?? config.Email.To;

        config.Email.SubjectPrefix = Environment.GetEnvironmentVariable("Email__SubjectPrefix") 
            ?? config.Email.SubjectPrefix;

        config.State.Path = Environment.GetEnvironmentVariable("State__Path") 
            ?? config.State.Path;

        config.Logging.Level = Environment.GetEnvironmentVariable("Logging__Level") 
            ?? config.Logging.Level;
    }
}
