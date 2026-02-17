using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent;

public class AppRunner
{
    private readonly AppConfig _config;
    private readonly WeatherClient _weatherClient;
    private readonly DecisionEngine _decisionEngine;
    private readonly HaikuGeneratorLocalTemplates _haikuGenerator;
    private readonly EmailClient _emailClient;
    private readonly StateStore _stateStore;

    public AppRunner(AppConfig config)
    {
        _config = config;
        _weatherClient = new WeatherClient();
        _decisionEngine = new DecisionEngine(config);
        _haikuGenerator = new HaikuGeneratorLocalTemplates(config.Haiku.TemplateRotationMode);
        _emailClient = new EmailClient(config.Email);
        _stateStore = new StateStore(config.State.Path);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("WeatherHaikuAgent - Run Mode");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        var state = _stateStore.Load();
        var now = DateTime.Now;

        await CheckLocalWeatherAsync(state, now);
        await CheckExtremeWeatherAsync(state, now);

        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("Run complete");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    private async Task CheckLocalWeatherAsync(AppState state, DateTime now)
    {
        Console.WriteLine("\n[Local Weather Check]");

        try
        {
            var tempF = await _weatherClient.GetCurrentTemperatureFAsync(
                _config.Local.Latitude,
                _config.Local.Longitude);

            Console.WriteLine($"Current temperature: {tempF:F1}°F at {_config.Local.LocationLabel}");

            var persona = PersonaEngine.PickPersona(tempF);
            Console.WriteLine($"Persona: {persona}");

            if (_decisionEngine.ShouldSendLocalNotification(tempF, persona, state, now))
            {
                var context = new WeatherContext
                {
                    TemperatureF = tempF,
                    Location = _config.Local.LocationLabel,
                    Timestamp = now,
                    TriggerType = "local",
                    Persona = persona
                };

                var haiku = _haikuGenerator.GenerateHaiku(context);
                
                var triggerReason = state.LastLocalTempF == null
                    ? "First notification"
                    : persona != state.LastLocalPersona
                        ? $"Persona changed from {state.LastLocalPersona}"
                        : $"Temperature changed by {Math.Abs(tempF - state.LastLocalTempF.Value):F1}°F";

                await _emailClient.SendHaikuEmailAsync(haiku, context, triggerReason);

                state.LastLocalTempF = tempF;
                state.LastLocalPersona = persona;
                state.LastLocalSentAt = now;

                _stateStore.Save(state);
            }
            else
            {
                Console.WriteLine("No notification needed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking local weather: {ex.Message}");
        }
    }

    private async Task CheckExtremeWeatherAsync(AppState state, DateTime now)
    {
        Console.WriteLine("\n[Extreme Weather Check]");

        if (!_decisionEngine.ShouldSendExtremeNotification(state, now))
        {
            return;
        }

        if (_config.Extreme.Locations.Count == 0)
        {
            Console.WriteLine("No extreme locations configured");
            return;
        }

        try
        {
            var locationTemps = new List<(string Name, double TempF, double Departure)>();

            foreach (var location in _config.Extreme.Locations)
            {
                try
                {
                    var tempF = await _weatherClient.GetCurrentTemperatureFAsync(
                        location.Latitude,
                        location.Longitude);

                    var departure = Math.Abs(tempF - _config.Extreme.ReferenceTempF);
                    locationTemps.Add((location.Name, tempF, departure));
                    
                    Console.WriteLine($"  {location.Name}: {tempF:F1}°F (departure: {departure:F1}°F)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Failed to fetch {location.Name}: {ex.Message}");
                }
            }

            if (locationTemps.Count == 0)
            {
                Console.WriteLine("No locations successfully fetched");
                return;
            }

            var selected = SelectExtremeLocation(locationTemps);
            Console.WriteLine($"Selected extreme location: {selected.Name} at {selected.TempF:F1}°F");

            var persona = PersonaEngine.PickPersona(selected.TempF);
            
            var context = new WeatherContext
            {
                TemperatureF = selected.TempF,
                Location = selected.Name,
                Timestamp = now,
                TriggerType = "extreme",
                Persona = persona
            };

            var haiku = _haikuGenerator.GenerateHaiku(context);
            var triggerReason = $"Daily extreme weather (departure: {selected.Departure:F1}°F from {_config.Extreme.ReferenceTempF}°F)";

            await _emailClient.SendHaikuEmailAsync(haiku, context, triggerReason);

            state.LastExtremeSentDate = DateOnly.FromDateTime(now);
            _stateStore.Save(state);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking extreme weather: {ex.Message}");
        }
    }

    private (string Name, double TempF, double Departure) SelectExtremeLocation(
        List<(string Name, double TempF, double Departure)> locations)
    {
        var mode = _config.Extreme.SelectionMode;
        var refTemp = _config.Extreme.ReferenceTempF;

        if (mode == "HotOnly")
        {
            return locations
                .Where(l => l.TempF > refTemp)
                .OrderByDescending(l => l.Departure)
                .First();
        }
        
        if (mode == "ColdOnly")
        {
            return locations
                .Where(l => l.TempF < refTemp)
                .OrderByDescending(l => l.Departure)
                .First();
        }

        return locations.OrderByDescending(l => l.Departure).First();
    }

    public async Task TestEmailAsync()
    {
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("WeatherHaikuAgent - Test Email");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        var context = new WeatherContext
        {
            TemperatureF = 72.0,
            Location = "Test Location",
            Timestamp = DateTime.Now,
            TriggerType = "test",
            Persona = "Porch Poet"
        };

        var haiku = _haikuGenerator.GenerateHaiku(context);

        Console.WriteLine($"\nSending test email to {_config.Email.To}...\n");
        Console.WriteLine(haiku);
        Console.WriteLine();

        try
        {
            await _emailClient.SendHaikuEmailAsync(haiku, context, "Test email");
            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("Test email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nTest email failed: {ex.Message}");
        }
    }

    public void DumpConfig()
    {
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("WeatherHaikuAgent - Configuration");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        Console.WriteLine("\n[Local Weather]");
        Console.WriteLine($"  Location: {_config.Local.LocationLabel}");
        Console.WriteLine($"  Latitude: {_config.Local.Latitude}");
        Console.WriteLine($"  Longitude: {_config.Local.Longitude}");

        Console.WriteLine("\n[Notification Rules]");
        Console.WriteLine($"  Temp Delta Threshold: {_config.Notify.TempDeltaF}°F");
        Console.WriteLine($"  Min Minutes Between: {_config.Notify.MinMinutesBetween}");
        Console.WriteLine($"  Quiet Hours: {_config.Notify.QuietHoursStart}:00 - {_config.Notify.QuietHoursEnd}:00");
        Console.WriteLine($"  Allow Quiet Hours Override: {_config.Notify.AllowQuietHoursOverride}");

        Console.WriteLine("\n[Extreme Weather]");
        Console.WriteLine($"  Enabled: {_config.Extreme.Enabled}");
        Console.WriteLine($"  Daily Send Time: {_config.Extreme.DailySendTimeLocal}");
        Console.WriteLine($"  Selection Mode: {_config.Extreme.SelectionMode}");
        Console.WriteLine($"  Reference Temp: {_config.Extreme.ReferenceTempF}°F");
        Console.WriteLine($"  Locations: {_config.Extreme.Locations.Count}");
        foreach (var loc in _config.Extreme.Locations)
        {
            Console.WriteLine($"    - {loc.Name} ({loc.Latitude}, {loc.Longitude})");
        }

        Console.WriteLine("\n[Haiku Generation]");
        Console.WriteLine($"  Mode: {_config.Haiku.Mode}");
        Console.WriteLine($"  Template Rotation: {_config.Haiku.TemplateRotationMode}");

        Console.WriteLine("\n[Email]");
        Console.WriteLine($"  SMTP Host: {_config.Email.SmtpHost}");
        Console.WriteLine($"  SMTP Port: {_config.Email.SmtpPort}");
        Console.WriteLine($"  Username: {_config.Email.Username}");
        Console.WriteLine($"  From: {_config.Email.From}");
        Console.WriteLine($"  To: {_config.Email.To}");
        Console.WriteLine($"  Subject Prefix: {_config.Email.SubjectPrefix}");

        Console.WriteLine("\n[State & Logging]");
        Console.WriteLine($"  State Path: {_config.State.Path}");
        Console.WriteLine($"  Logging Level: {_config.Logging.Level}");

        Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
}
