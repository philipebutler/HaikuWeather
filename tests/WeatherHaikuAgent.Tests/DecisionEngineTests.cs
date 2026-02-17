using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent.Tests;

public class DecisionEngineTests
{
    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            Notify = new NotifyConfig
            {
                TempDeltaF = 3.0,
                MinMinutesBetween = 60,
                QuietHoursStart = 7,
                QuietHoursEnd = 21,
                AllowQuietHoursOverride = false
            },
            Extreme = new ExtremeConfig
            {
                Enabled = true,
                DailySendTimeLocal = "07:30"
            }
        };
    }

    #region Temperature Delta Tests

    [Fact]
    public void ShouldSendLocalNotification_FirstRun_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(65, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_TempDeltaExceedsThreshold_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(68.5, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_TempDeltaBelowThreshold_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(67, "Porch Poet", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_ExactThreshold_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(68, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_NegativeDelta_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(61, "Mud Philosopher", state, now);

        Assert.True(result);
    }

    #endregion

    #region Persona Change Tests

    [Fact]
    public void ShouldSendLocalNotification_PersonaChange_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 75,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(76, "Sun Hypeman", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_PersonaChangeSmallDelta_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 60,
            LastLocalPersona = "Mud Philosopher",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(61, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_NoPersonaChangeSmallDelta_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(66, "Porch Poet", state, now);

        Assert.False(result);
    }

    #endregion

    #region Cooldown Tests

    [Fact]
    public void ShouldSendLocalNotification_CooldownNotSatisfied_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 11, 30, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_CooldownExactlyMet_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 11, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_CooldownExceeded_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 30, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_CustomCooldown_RespectsConfig()
    {
        var config = CreateDefaultConfig();
        config.Notify.MinMinutesBetween = 30;
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 11, 35, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.False(result);
    }

    #endregion

    #region Quiet Hours Tests

    [Fact]
    public void ShouldSendLocalNotification_WithinQuietHours_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_BeforeQuietHoursStart_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 3, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 6, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_AtQuietHoursStart_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 5, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 7, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_AtQuietHoursEnd_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 19, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 21, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_AfterQuietHoursEnd_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 19, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 22, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_QuietHoursOverride_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        config.Notify.AllowQuietHoursOverride = true;
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 1, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 3, 0, 0);

        var result = engine.ShouldSendLocalNotification(72, "Porch Poet", state, now);

        Assert.True(result);
    }

    #endregion

    #region Extreme Weather Tests

    [Fact]
    public void ShouldSendExtremeNotification_FirstRun_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 8, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_Disabled_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        config.Extreme.Enabled = false;
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 8, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_AlreadySentToday_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastExtremeSentDate = new DateOnly(2024, 1, 15)
        };
        var now = new DateTime(2024, 1, 15, 8, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_SentYesterday_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastExtremeSentDate = new DateOnly(2024, 1, 14)
        };
        var now = new DateTime(2024, 1, 15, 8, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_BeforeSendTime_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 7, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_AtSendTime_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 7, 30, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_AfterSendTime_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.True(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_InvalidTimeFormat_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        config.Extreme.DailySendTimeLocal = "invalid";
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendExtremeNotification_CustomSendTime_RespectsConfig()
    {
        var config = CreateDefaultConfig();
        config.Extreme.DailySendTimeLocal = "14:30";
        var engine = new DecisionEngine(config);
        var state = new AppState();
        var now = new DateTime(2024, 1, 15, 14, 0, 0);

        var result = engine.ShouldSendExtremeNotification(state, now);

        Assert.False(result);
    }

    #endregion

    #region Combined Scenario Tests

    [Fact]
    public void ShouldSendLocalNotification_LargeDeltaButInCooldown_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 11, 45, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(80, "Sun Hypeman", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_LargeDeltaButOutsideQuietHours_ReturnsFalse()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 65,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 14, 12, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 2, 0, 0);

        var result = engine.ShouldSendLocalNotification(80, "Sun Hypeman", state, now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldSendLocalNotification_PersonaChangeAndLargeDelta_ReturnsTrue()
    {
        var config = CreateDefaultConfig();
        var engine = new DecisionEngine(config);
        var state = new AppState
        {
            LastLocalTempF = 60,
            LastLocalPersona = "Mud Philosopher",
            LastLocalSentAt = new DateTime(2024, 1, 15, 10, 0, 0)
        };
        var now = new DateTime(2024, 1, 15, 12, 0, 0);

        var result = engine.ShouldSendLocalNotification(76, "Sun Hypeman", state, now);

        Assert.True(result);
    }

    #endregion
}
