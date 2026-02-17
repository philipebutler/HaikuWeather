using System.Text.Json;

namespace WeatherHaikuAgent;

public class WeatherClient
{
    private readonly HttpClient _httpClient;

    public WeatherClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    public async Task<double> GetCurrentTemperatureFAsync(double latitude, double longitude)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true&temperature_unit=fahrenheit";

        try
        {
            var response = await _httpClient.GetStringAsync(url);
            return ParseTemperatureFromResponse(response);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error fetching weather: {ex.Message}");
            await Task.Delay(2000);
            
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                return ParseTemperatureFromResponse(response);
            }
            catch (Exception retryEx)
            {
                throw new Exception($"Failed to fetch weather after retry: {retryEx.Message}");
            }
        }
    }

    private double ParseTemperatureFromResponse(string response)
    {
        var doc = JsonDocument.Parse(response);
        
        if (doc.RootElement.TryGetProperty("current_weather", out var currentWeather) &&
            currentWeather.TryGetProperty("temperature", out var temp))
        {
            return temp.GetDouble();
        }

        throw new Exception("Temperature data not found in API response. Expected JSON structure: {\"current_weather\": {\"temperature\": <number>}}");
    }
}
