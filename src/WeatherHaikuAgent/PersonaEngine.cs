namespace WeatherHaikuAgent;

public class PersonaEngine
{
    public static string PickPersona(double tempF)
    {
        return tempF switch
        {
            <= 20 => "Frost Monk",
            <= 40 => "Snow Comedian",
            <= 60 => "Mud Philosopher",
            <= 75 => "Porch Poet",
            <= 90 => "Sun Hypeman",
            _ => "Heat Dramatic"
        };
    }
}
