namespace WeatherHaikuAgent.Tests;

public class PersonaEngineTests
{
    [Theory]
    [InlineData(-40, "Frost Monk")]
    [InlineData(0, "Frost Monk")]
    [InlineData(20, "Frost Monk")]
    public void PickPersona_FrostMonk_BoundaryTests(double tempF, string expected)
    {
        var result = PersonaEngine.PickPersona(tempF);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(21, "Snow Comedian")]
    [InlineData(30, "Snow Comedian")]
    [InlineData(40, "Snow Comedian")]
    public void PickPersona_SnowComedian_BoundaryTests(double tempF, string expected)
    {
        var result = PersonaEngine.PickPersona(tempF);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(41, "Mud Philosopher")]
    [InlineData(50, "Mud Philosopher")]
    [InlineData(60, "Mud Philosopher")]
    public void PickPersona_MudPhilosopher_BoundaryTests(double tempF, string expected)
    {
        var result = PersonaEngine.PickPersona(tempF);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(61, "Porch Poet")]
    [InlineData(68, "Porch Poet")]
    [InlineData(75, "Porch Poet")]
    public void PickPersona_PorchPoet_BoundaryTests(double tempF, string expected)
    {
        var result = PersonaEngine.PickPersona(tempF);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(76, "Sun Hypeman")]
    [InlineData(83, "Sun Hypeman")]
    [InlineData(90, "Sun Hypeman")]
    public void PickPersona_SunHypeman_BoundaryTests(double tempF, string expected)
    {
        var result = PersonaEngine.PickPersona(tempF);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(91, "Heat Dramatic")]
    [InlineData(100, "Heat Dramatic")]
    [InlineData(120, "Heat Dramatic")]
    public void PickPersona_HeatDramatic_BoundaryTests(double tempF, string expected)
    {
        var result = PersonaEngine.PickPersona(tempF);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PickPersona_ExactBoundary_20Degrees_ReturnsFrostMonk()
    {
        var result = PersonaEngine.PickPersona(20);
        Assert.Equal("Frost Monk", result);
    }

    [Fact]
    public void PickPersona_ExactBoundary_40Degrees_ReturnsSnowComedian()
    {
        var result = PersonaEngine.PickPersona(40);
        Assert.Equal("Snow Comedian", result);
    }

    [Fact]
    public void PickPersona_ExactBoundary_60Degrees_ReturnsMudPhilosopher()
    {
        var result = PersonaEngine.PickPersona(60);
        Assert.Equal("Mud Philosopher", result);
    }

    [Fact]
    public void PickPersona_ExactBoundary_75Degrees_ReturnsPorchPoet()
    {
        var result = PersonaEngine.PickPersona(75);
        Assert.Equal("Porch Poet", result);
    }

    [Fact]
    public void PickPersona_ExactBoundary_90Degrees_ReturnsSunHypeman()
    {
        var result = PersonaEngine.PickPersona(90);
        Assert.Equal("Sun Hypeman", result);
    }

    [Fact]
    public void PickPersona_ExactBoundary_91Degrees_ReturnsHeatDramatic()
    {
        var result = PersonaEngine.PickPersona(91);
        Assert.Equal("Heat Dramatic", result);
    }

    [Theory]
    [InlineData(20.0)]
    [InlineData(40.0)]
    [InlineData(60.0)]
    [InlineData(75.0)]
    [InlineData(90.0)]
    public void PickPersona_AtBoundaries_IsConsistent(double tempF)
    {
        var result1 = PersonaEngine.PickPersona(tempF);
        var result2 = PersonaEngine.PickPersona(tempF);
        Assert.Equal(result1, result2);
    }

    [Theory]
    [InlineData(20, 21)]
    [InlineData(40, 41)]
    [InlineData(60, 61)]
    [InlineData(75, 76)]
    [InlineData(90, 91)]
    public void PickPersona_PersonaChangesAcrossBoundary(double temp1, double temp2)
    {
        var persona1 = PersonaEngine.PickPersona(temp1);
        var persona2 = PersonaEngine.PickPersona(temp2);
        Assert.NotEqual(persona1, persona2);
    }
}
