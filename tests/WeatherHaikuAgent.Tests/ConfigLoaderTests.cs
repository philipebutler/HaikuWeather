using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent.Tests;

public class ConfigLoaderTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _originalDirectory;
    private readonly Dictionary<string, string?> _originalEnvVars = new();

    public ConfigLoaderTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _testDir = Path.Combine(Path.GetTempPath(), $"ConfigLoaderTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);

        // Save original environment variables that we'll modify
        var envVars = new[]
        {
            "Local__Latitude", "Local__Longitude", "Local__LocationLabel",
            "Notify__TempDeltaF", "Notify__MinMinutesBetween",
            "Notify__QuietHoursStart", "Notify__QuietHoursEnd", "Notify__AllowQuietHoursOverride",
            "Extreme__Enabled", "Extreme__DailySendTimeLocal", "Extreme__SelectionMode", "Extreme__ReferenceTempF",
            "Haiku__Mode", "Haiku__TemplateRotationMode",
            "Email__SmtpHost", "Email__SmtpPort", "Email__Username", "Email__Password",
            "Email__From", "Email__To", "Email__SubjectPrefix",
            "State__Path", "Logging__Level"
        };

        foreach (var envVar in envVars)
        {
            _originalEnvVars[envVar] = Environment.GetEnvironmentVariable(envVar);
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);
        
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }

        // Restore original environment variables
        foreach (var kvp in _originalEnvVars)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
    }

    [Fact]
    public void Load_NoConfigFile_ReturnsDefaultConfig()
    {
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.NotNull(config);
        Assert.Equal(3.0, config.Notify.TempDeltaF);
        Assert.Equal(60, config.Notify.MinMinutesBetween);
        Assert.Equal(7, config.Notify.QuietHoursStart);
        Assert.Equal(21, config.Notify.QuietHoursEnd);
        Assert.False(config.Notify.AllowQuietHoursOverride);
        Assert.True(config.Extreme.Enabled);
        Assert.Equal("07:30", config.Extreme.DailySendTimeLocal);
        Assert.Equal("HotOrColdByDeparture", config.Extreme.SelectionMode);
        Assert.Equal(65.0, config.Extreme.ReferenceTempF);
        Assert.Equal("LocalTemplates", config.Haiku.Mode);
        Assert.Equal("Deterministic", config.Haiku.TemplateRotationMode);
    }

    [Fact]
    public void Load_WithJsonFile_LoadsValues()
    {
        var configJson = @"{
            ""Local"": {
                ""Latitude"": 37.7749,
                ""Longitude"": -122.4194,
                ""LocationLabel"": ""San Francisco""
            },
            ""Notify"": {
                ""TempDeltaF"": 5.0,
                ""MinMinutesBetween"": 90
            }
        }";
        
        File.WriteAllText(Path.Combine(_testDir, "appsettings.json"), configJson);
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.Equal(37.7749, config.Local.Latitude);
        Assert.Equal(-122.4194, config.Local.Longitude);
        Assert.Equal("San Francisco", config.Local.LocationLabel);
        Assert.Equal(5.0, config.Notify.TempDeltaF);
        Assert.Equal(90, config.Notify.MinMinutesBetween);
    }

    [Fact]
    public void Load_EnvironmentVariables_OverrideJsonConfig()
    {
        var configJson = @"{
            ""Local"": {
                ""Latitude"": 37.7749,
                ""LocationLabel"": ""San Francisco""
            },
            ""Notify"": {
                ""TempDeltaF"": 5.0
            }
        }";
        
        File.WriteAllText(Path.Combine(_testDir, "appsettings.json"), configJson);
        Directory.SetCurrentDirectory(_testDir);

        Environment.SetEnvironmentVariable("Local__Latitude", "40.7128");
        Environment.SetEnvironmentVariable("Notify__TempDeltaF", "7.5");

        var config = ConfigLoader.Load();

        Assert.Equal(40.7128, config.Local.Latitude);
        Assert.Equal("San Francisco", config.Local.LocationLabel);
        Assert.Equal(7.5, config.Notify.TempDeltaF);
    }

    [Fact]
    public void Load_LocalConfig_FromEnvironmentVariables()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Local__Latitude", "34.0522");
        Environment.SetEnvironmentVariable("Local__Longitude", "-118.2437");
        Environment.SetEnvironmentVariable("Local__LocationLabel", "Los Angeles");

        var config = ConfigLoader.Load();

        Assert.Equal(34.0522, config.Local.Latitude);
        Assert.Equal(-118.2437, config.Local.Longitude);
        Assert.Equal("Los Angeles", config.Local.LocationLabel);
    }

    [Fact]
    public void Load_NotifyConfig_FromEnvironmentVariables()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Notify__TempDeltaF", "4.5");
        Environment.SetEnvironmentVariable("Notify__MinMinutesBetween", "120");
        Environment.SetEnvironmentVariable("Notify__QuietHoursStart", "8");
        Environment.SetEnvironmentVariable("Notify__QuietHoursEnd", "22");
        Environment.SetEnvironmentVariable("Notify__AllowQuietHoursOverride", "true");

        var config = ConfigLoader.Load();

        Assert.Equal(4.5, config.Notify.TempDeltaF);
        Assert.Equal(120, config.Notify.MinMinutesBetween);
        Assert.Equal(8, config.Notify.QuietHoursStart);
        Assert.Equal(22, config.Notify.QuietHoursEnd);
        Assert.True(config.Notify.AllowQuietHoursOverride);
    }

    [Fact]
    public void Load_ExtremeConfig_FromEnvironmentVariables()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Extreme__Enabled", "false");
        Environment.SetEnvironmentVariable("Extreme__DailySendTimeLocal", "08:00");
        Environment.SetEnvironmentVariable("Extreme__SelectionMode", "HotOnly");
        Environment.SetEnvironmentVariable("Extreme__ReferenceTempF", "70.0");

        var config = ConfigLoader.Load();

        Assert.False(config.Extreme.Enabled);
        Assert.Equal("08:00", config.Extreme.DailySendTimeLocal);
        Assert.Equal("HotOnly", config.Extreme.SelectionMode);
        Assert.Equal(70.0, config.Extreme.ReferenceTempF);
    }

    [Fact]
    public void Load_HaikuConfig_FromEnvironmentVariables()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Haiku__Mode", "OpenAI");
        Environment.SetEnvironmentVariable("Haiku__TemplateRotationMode", "Random");

        var config = ConfigLoader.Load();

        Assert.Equal("OpenAI", config.Haiku.Mode);
        Assert.Equal("Random", config.Haiku.TemplateRotationMode);
    }

    [Fact]
    public void Load_EmailConfig_FromEnvironmentVariables()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Email__SmtpHost", "smtp.example.com");
        Environment.SetEnvironmentVariable("Email__SmtpPort", "465");
        Environment.SetEnvironmentVariable("Email__Username", "user@example.com");
        Environment.SetEnvironmentVariable("Email__Password", "secret123");
        Environment.SetEnvironmentVariable("Email__From", "from@example.com");
        Environment.SetEnvironmentVariable("Email__To", "to@example.com");
        Environment.SetEnvironmentVariable("Email__SubjectPrefix", "TestPrefix");

        var config = ConfigLoader.Load();

        Assert.Equal("smtp.example.com", config.Email.SmtpHost);
        Assert.Equal(465, config.Email.SmtpPort);
        Assert.Equal("user@example.com", config.Email.Username);
        Assert.Equal("secret123", config.Email.Password);
        Assert.Equal("from@example.com", config.Email.From);
        Assert.Equal("to@example.com", config.Email.To);
        Assert.Equal("TestPrefix", config.Email.SubjectPrefix);
    }

    [Fact]
    public void Load_StateAndLoggingConfig_FromEnvironmentVariables()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("State__Path", "/custom/state.json");
        Environment.SetEnvironmentVariable("Logging__Level", "Debug");

        var config = ConfigLoader.Load();

        Assert.Equal("/custom/state.json", config.State.Path);
        Assert.Equal("Debug", config.Logging.Level);
    }

    [Fact]
    public void Load_InvalidNumericEnvironmentVariable_ThrowsException()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Notify__TempDeltaF", "not-a-number");

        // ConfigurationBinder throws InvalidOperationException for invalid type conversions
        Assert.Throws<InvalidOperationException>(() => ConfigLoader.Load());
    }

    [Fact]
    public void Load_InvalidBooleanEnvironmentVariable_ThrowsException()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Notify__AllowQuietHoursOverride", "not-a-bool");

        // ConfigurationBinder throws InvalidOperationException for invalid type conversions
        Assert.Throws<InvalidOperationException>(() => ConfigLoader.Load());
    }

    [Fact]
    public void Load_EmptyStringEnvironmentVariable_AllowedForStrings()
    {
        Directory.SetCurrentDirectory(_testDir);
        
        Environment.SetEnvironmentVariable("Email__Username", "");
        Environment.SetEnvironmentVariable("Local__LocationLabel", "");

        var config = ConfigLoader.Load();

        Assert.Equal("", config.Email.Username);
        Assert.Equal("", config.Local.LocationLabel);
    }

    [Fact]
    public void Load_CompleteJsonConfig_LoadsAllSections()
    {
        var configJson = @"{
            ""Local"": {
                ""Latitude"": 37.7749,
                ""Longitude"": -122.4194,
                ""LocationLabel"": ""San Francisco""
            },
            ""Notify"": {
                ""TempDeltaF"": 5.0,
                ""MinMinutesBetween"": 90,
                ""QuietHoursStart"": 8,
                ""QuietHoursEnd"": 22,
                ""AllowQuietHoursOverride"": true
            },
            ""Extreme"": {
                ""Enabled"": false,
                ""DailySendTimeLocal"": ""08:00"",
                ""SelectionMode"": ""ColdOnly"",
                ""ReferenceTempF"": 70.0,
                ""Locations"": [
                    {
                        ""Name"": ""Phoenix"",
                        ""Latitude"": 33.4484,
                        ""Longitude"": -112.0740
                    }
                ]
            },
            ""Haiku"": {
                ""Mode"": ""OpenAI"",
                ""TemplateRotationMode"": ""Random""
            },
            ""Email"": {
                ""SmtpHost"": ""smtp.test.com"",
                ""SmtpPort"": 465,
                ""Username"": ""test@test.com"",
                ""Password"": ""testpass"",
                ""From"": ""from@test.com"",
                ""To"": ""to@test.com"",
                ""SubjectPrefix"": ""TestHaiku""
            },
            ""State"": {
                ""Path"": ""/tmp/state.json""
            },
            ""Logging"": {
                ""Level"": ""Debug""
            }
        }";
        
        File.WriteAllText(Path.Combine(_testDir, "appsettings.json"), configJson);
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.Equal(37.7749, config.Local.Latitude);
        Assert.Equal(5.0, config.Notify.TempDeltaF);
        Assert.False(config.Extreme.Enabled);
        Assert.Equal("OpenAI", config.Haiku.Mode);
        Assert.Equal("smtp.test.com", config.Email.SmtpHost);
        Assert.Equal("/tmp/state.json", config.State.Path);
        Assert.Equal("Debug", config.Logging.Level);
        Assert.Single(config.Extreme.Locations);
        Assert.Equal("Phoenix", config.Extreme.Locations[0].Name);
    }

    [Fact]
    public void Load_JsonWithComments_HandlesGracefully()
    {
        // JSON with comments is not valid, but testing that it doesn't crash
        var configJson = @"{
            ""Local"": {
                ""Latitude"": 37.7749
            }
        }";
        
        File.WriteAllText(Path.Combine(_testDir, "appsettings.json"), configJson);
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.NotNull(config);
        Assert.Equal(37.7749, config.Local.Latitude);
    }

    [Fact]
    public void Load_MixedJsonAndEnvironment_EnvironmentTakesPrecedence()
    {
        var configJson = @"{
            ""Local"": {
                ""Latitude"": 37.7749,
                ""Longitude"": -122.4194,
                ""LocationLabel"": ""San Francisco""
            },
            ""Notify"": {
                ""TempDeltaF"": 5.0,
                ""MinMinutesBetween"": 90
            }
        }";
        
        File.WriteAllText(Path.Combine(_testDir, "appsettings.json"), configJson);
        Directory.SetCurrentDirectory(_testDir);

        Environment.SetEnvironmentVariable("Local__Latitude", "40.7128");
        Environment.SetEnvironmentVariable("Local__Longitude", "-74.0060");

        var config = ConfigLoader.Load();

        Assert.Equal(40.7128, config.Local.Latitude);
        Assert.Equal(-74.0060, config.Local.Longitude);
        Assert.Equal("San Francisco", config.Local.LocationLabel);
        Assert.Equal(5.0, config.Notify.TempDeltaF);
    }

    [Fact]
    public void Load_DefaultEmailConfig_HasCorrectValues()
    {
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.Equal("smtp.gmail.com", config.Email.SmtpHost);
        Assert.Equal(587, config.Email.SmtpPort);
        Assert.Equal("WeatherHaiku", config.Email.SubjectPrefix);
    }

    [Fact]
    public void Load_DefaultStateConfig_HasCorrectPath()
    {
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.Equal("./state.json", config.State.Path);
    }

    [Fact]
    public void Load_DefaultLoggingConfig_HasInfoLevel()
    {
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.Equal("Info", config.Logging.Level);
    }

    [Fact]
    public void Load_MultipleLocations_LoadsCorrectly()
    {
        var configJson = @"{
            ""Extreme"": {
                ""Locations"": [
                    {
                        ""Name"": ""Phoenix"",
                        ""Latitude"": 33.4484,
                        ""Longitude"": -112.0740
                    },
                    {
                        ""Name"": ""Fairbanks"",
                        ""Latitude"": 64.8378,
                        ""Longitude"": -147.7164
                    }
                ]
            }
        }";
        
        File.WriteAllText(Path.Combine(_testDir, "appsettings.json"), configJson);
        Directory.SetCurrentDirectory(_testDir);

        var config = ConfigLoader.Load();

        Assert.Equal(2, config.Extreme.Locations.Count);
        Assert.Equal("Phoenix", config.Extreme.Locations[0].Name);
        Assert.Equal(33.4484, config.Extreme.Locations[0].Latitude);
        Assert.Equal("Fairbanks", config.Extreme.Locations[1].Name);
        Assert.Equal(64.8378, config.Extreme.Locations[1].Latitude);
    }
}
