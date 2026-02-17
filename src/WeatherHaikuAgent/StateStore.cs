using System.Text.Json;
using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent;

public class StateStore
{
    private readonly string _statePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public StateStore(string statePath)
    {
        _statePath = statePath;
    }

    public AppState Load()
    {
        try
        {
            if (!File.Exists(_statePath))
            {
                Console.WriteLine("No state file found, starting fresh");
                return new AppState();
            }

            var json = File.ReadAllText(_statePath);
            var state = JsonSerializer.Deserialize<AppState>(json);
            
            if (state == null)
            {
                Console.WriteLine("Failed to deserialize state, starting fresh");
                return new AppState();
            }

            Console.WriteLine($"Loaded state: Last temp {state.LastLocalTempF}°F, Last sent {state.LastLocalSentAt}");
            return state;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading state: {ex.Message}, starting fresh");
            return new AppState();
        }
    }

    public void Save(AppState state)
    {
        try
        {
            var tempPath = _statePath + ".tmp";
            var json = JsonSerializer.Serialize(state, _jsonOptions);
            
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _statePath, overwrite: true);
            
            Console.WriteLine($"✓ State saved: Last temp {state.LastLocalTempF}°F, Last sent {state.LastLocalSentAt}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error saving state: {ex.Message}");
            throw;
        }
    }
}
