namespace WeatherHaikuAgent.Models;

public class AppState
{
    public double? LastLocalTempF { get; set; }
    public string? LastLocalPersona { get; set; }
    public DateTime? LastLocalSentAt { get; set; }
    public DateOnly? LastExtremeSentDate { get; set; }
    public Dictionary<string, int> TemplateUsageCount { get; set; } = new();
}
