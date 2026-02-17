namespace WeatherHaikuAgent.Models;

public class WeatherContext
{
    public double TemperatureF { get; set; }
    public string Location { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string TriggerType { get; set; } = "local";
    public string Persona { get; set; } = "";
}
