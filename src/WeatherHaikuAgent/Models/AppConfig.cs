namespace WeatherHaikuAgent.Models;

public class AppConfig
{
    public LocalConfig Local { get; set; } = new();
    public NotifyConfig Notify { get; set; } = new();
    public ExtremeConfig Extreme { get; set; } = new();
    public HaikuConfig Haiku { get; set; } = new();
    public EmailConfig Email { get; set; } = new();
    public StateConfig State { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
}

public class LocalConfig
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationLabel { get; set; } = "Local";
}

public class NotifyConfig
{
    public double TempDeltaF { get; set; } = 3.0;
    public int MinMinutesBetween { get; set; } = 60;
    public int QuietHoursStart { get; set; } = 7;
    public int QuietHoursEnd { get; set; } = 21;
    public bool AllowQuietHoursOverride { get; set; } = false;
}

public class ExtremeConfig
{
    public bool Enabled { get; set; } = true;
    public string DailySendTimeLocal { get; set; } = "07:30";
    public string SelectionMode { get; set; } = "HotOrColdByDeparture";
    public double ReferenceTempF { get; set; } = 65.0;
    public List<LocationConfig> Locations { get; set; } = new();
}

public class LocationConfig
{
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class HaikuConfig
{
    public string Mode { get; set; } = "LocalTemplates";
    public string TemplateRotationMode { get; set; } = "Deterministic";
}

public class EmailConfig
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string SubjectPrefix { get; set; } = "WeatherHaiku";
}

public class StateConfig
{
    public string Path { get; set; } = "./state.json";
}

public class LoggingConfig
{
    public string Level { get; set; } = "Info";
}
