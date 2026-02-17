using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent;

public class DecisionEngine
{
    private readonly AppConfig _config;

    public DecisionEngine(AppConfig config)
    {
        _config = config;
    }

    public bool ShouldSendLocalNotification(
        double currentTempF,
        string currentPersona,
        AppState state,
        DateTime now)
    {
        if (!IsCooldownSatisfied(state, now))
        {
            Console.WriteLine("Cooldown not satisfied");
            return false;
        }

        if (!IsWithinAllowedHours(now))
        {
            Console.WriteLine("Outside allowed hours");
            return false;
        }

        if (state.LastLocalTempF == null || state.LastLocalPersona == null)
        {
            Console.WriteLine("First run - will send");
            return true;
        }

        var tempDelta = Math.Abs(currentTempF - state.LastLocalTempF.Value);
        var personaChanged = currentPersona != state.LastLocalPersona;

        if (tempDelta >= _config.Notify.TempDeltaF)
        {
            Console.WriteLine($"Temperature delta {tempDelta:F1}°F exceeds threshold {_config.Notify.TempDeltaF}°F");
            return true;
        }

        if (personaChanged)
        {
            Console.WriteLine($"Persona changed from {state.LastLocalPersona} to {currentPersona}");
            return true;
        }

        Console.WriteLine($"No significant change (delta: {tempDelta:F1}°F, persona same: {!personaChanged})");
        return false;
    }

    public bool ShouldSendExtremeNotification(AppState state, DateTime now)
    {
        if (!_config.Extreme.Enabled)
        {
            Console.WriteLine("Extreme notifications disabled");
            return false;
        }

        var today = DateOnly.FromDateTime(now);
        
        if (state.LastExtremeSentDate == today)
        {
            Console.WriteLine("Extreme notification already sent today");
            return false;
        }

        if (!TimeSpan.TryParse(_config.Extreme.DailySendTimeLocal, out var sendTime))
        {
            Console.WriteLine("Invalid DailySendTimeLocal format");
            return false;
        }

        var currentTime = now.TimeOfDay;
        if (currentTime < sendTime)
        {
            Console.WriteLine($"Current time {currentTime} is before send time {sendTime}");
            return false;
        }

        Console.WriteLine("Should send extreme notification");
        return true;
    }

    private bool IsCooldownSatisfied(AppState state, DateTime now)
    {
        if (state.LastLocalSentAt == null)
        {
            return true;
        }

        var minutesSinceLastSend = (now - state.LastLocalSentAt.Value).TotalMinutes;
        return minutesSinceLastSend >= _config.Notify.MinMinutesBetween;
    }

    private bool IsWithinAllowedHours(DateTime now)
    {
        if (_config.Notify.AllowQuietHoursOverride)
        {
            return true;
        }

        var hour = now.Hour;
        var start = _config.Notify.QuietHoursStart;
        var end = _config.Notify.QuietHoursEnd;

        return hour >= start && hour < end;
    }
}
