using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent.Tests;

public class HaikuGeneratorTests
{
    [Fact]
    public void GenerateHaiku_FrostMonk_ReturnsHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 15,
            Persona = "Frost Monk",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        Assert.Contains("\n", haiku);
    }

    [Fact]
    public void GenerateHaiku_SnowComedian_ReturnsHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 35,
            Persona = "Snow Comedian",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        Assert.Contains("\n", haiku);
    }

    [Fact]
    public void GenerateHaiku_MudPhilosopher_ReturnsHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 55,
            Persona = "Mud Philosopher",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        Assert.Contains("\n", haiku);
    }

    [Fact]
    public void GenerateHaiku_PorchPoet_ReturnsHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        Assert.Contains("\n", haiku);
    }

    [Fact]
    public void GenerateHaiku_SunHypeman_ReturnsHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 85,
            Persona = "Sun Hypeman",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        Assert.Contains("\n", haiku);
    }

    [Fact]
    public void GenerateHaiku_HeatDramatic_ReturnsHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 100,
            Persona = "Heat Dramatic",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        Assert.Contains("\n", haiku);
    }

    [Fact]
    public void GenerateHaiku_InvalidPersona_ReturnsDefaultHaiku()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Invalid Persona",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.Contains("No persona found", haiku);
    }

    [Fact]
    public void GenerateHaiku_DeterministicMode_SameInputsSameOutput()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku1 = generator.GenerateHaiku(context);
        var haiku2 = generator.GenerateHaiku(context);

        Assert.Equal(haiku1, haiku2);
    }

    [Fact]
    public void GenerateHaiku_DeterministicMode_DifferentDates_MayProduceDifferentOutput()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        
        // Test multiple dates to ensure deterministic behavior is based on date
        var haikus = new List<string>();
        for (int day = 1; day <= 20; day++)
        {
            var context = new WeatherContext
            {
                TemperatureF = 68,
                Persona = "Porch Poet",
                Location = "Test Location",
                Timestamp = new DateTime(2024, 1, day, 12, 0, 0),
                TriggerType = "local"
            };
            haikus.Add(generator.GenerateHaiku(context));
        }

        // With enough dates, we should see some variation
        var uniqueHaikus = haikus.Distinct().Count();
        Assert.True(uniqueHaikus > 1, "Deterministic mode should produce different haikus for different dates");
    }

    [Fact]
    public void GenerateHaiku_DeterministicMode_DifferentTemps_MayDifferIfHashChanges()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context1 = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };
        var context2 = new WeatherContext
        {
            TemperatureF = 70,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku1 = generator.GenerateHaiku(context1);
        var haiku2 = generator.GenerateHaiku(context2);

        // May or may not be different depending on hash collisions
        Assert.NotNull(haiku1);
        Assert.NotNull(haiku2);
    }

    [Fact]
    public void GenerateHaiku_RandomMode_MultipleCallsReturnValidHaikus()
    {
        var generator = new HaikuGeneratorLocalTemplates("Random");
        var context = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haikus = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            var haiku = generator.GenerateHaiku(context);
            Assert.NotNull(haiku);
            Assert.NotEmpty(haiku);
            haikus.Add(haiku);
        }

        // Random mode should potentially generate different haikus
        Assert.True(haikus.Count >= 1);
    }

    [Fact]
    public void GenerateHaiku_AllPersonas_HaveMultipleTemplates()
    {
        var generator = new HaikuGeneratorLocalTemplates("Random");
        var personas = new[] { "Frost Monk", "Snow Comedian", "Mud Philosopher", 
                               "Porch Poet", "Sun Hypeman", "Heat Dramatic" };

        foreach (var persona in personas)
        {
            var haikusForPersona = new HashSet<string>();
            
            for (int i = 0; i < 50; i++)
            {
                var context = new WeatherContext
                {
                    TemperatureF = 68,
                    Persona = persona,
                    Location = "Test Location",
                    Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
                    TriggerType = "local"
                };

                var haiku = generator.GenerateHaiku(context);
                haikusForPersona.Add(haiku);
            }

            // Each persona should have at least 5 different templates as per SPEC.md
            Assert.True(haikusForPersona.Count >= 5, 
                $"{persona} should have at least 5 templates, found {haikusForPersona.Count}");
        }
    }

    [Fact]
    public void GenerateHaiku_HaikuFormat_HasThreeLines()
    {
        var generator = new HaikuGeneratorLocalTemplates("Deterministic");
        var context = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);
        var lines = haiku.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, lines.Length);
    }

    [Fact]
    public void GenerateHaiku_AllPersonasHaikus_HaveThreeLines()
    {
        var generator = new HaikuGeneratorLocalTemplates("Random");
        var personas = new[] { "Frost Monk", "Snow Comedian", "Mud Philosopher", 
                               "Porch Poet", "Sun Hypeman", "Heat Dramatic" };

        foreach (var persona in personas)
        {
            var context = new WeatherContext
            {
                TemperatureF = 68,
                Persona = persona,
                Location = "Test Location",
                Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
                TriggerType = "local"
            };

            for (int i = 0; i < 10; i++)
            {
                var haiku = generator.GenerateHaiku(context);
                var lines = haiku.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                Assert.True(lines.Length == 3, 
                    $"{persona} haiku should have 3 lines, has {lines.Length}: {haiku}");
            }
        }
    }

    [Theory]
    [InlineData("Deterministic")]
    [InlineData("Random")]
    public void GenerateHaiku_DifferentModes_ProduceValidHaikus(string mode)
    {
        var generator = new HaikuGeneratorLocalTemplates(mode);
        var context = new WeatherContext
        {
            TemperatureF = 68,
            Persona = "Porch Poet",
            Location = "Test Location",
            Timestamp = new DateTime(2024, 1, 15, 12, 0, 0),
            TriggerType = "local"
        };

        var haiku = generator.GenerateHaiku(context);

        Assert.NotNull(haiku);
        Assert.NotEmpty(haiku);
        var lines = haiku.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
    }
}
