using System.Text.Json;
using WeatherHaikuAgent.Models;

namespace WeatherHaikuAgent.Tests;

public class StateStoreTests : IDisposable
{
    private readonly string _testStateDir;
    private readonly string _testStatePath;

    public StateStoreTests()
    {
        _testStateDir = Path.Combine(Path.GetTempPath(), $"StateStoreTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testStateDir);
        _testStatePath = Path.Combine(_testStateDir, "test_state.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStateDir))
        {
            Directory.Delete(_testStateDir, recursive: true);
        }
    }

    [Fact]
    public void Load_NoStateFile_ReturnsNewState()
    {
        var store = new StateStore(_testStatePath);

        var state = store.Load();

        Assert.NotNull(state);
        Assert.Null(state.LastLocalTempF);
        Assert.Null(state.LastLocalPersona);
        Assert.Null(state.LastLocalSentAt);
        Assert.Null(state.LastExtremeSentDate);
    }

    [Fact]
    public void Save_ValidState_CreatesFile()
    {
        var store = new StateStore(_testStatePath);
        var state = new AppState
        {
            LastLocalTempF = 68.5,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 12, 0, 0),
            LastExtremeSentDate = new DateOnly(2024, 1, 15)
        };

        store.Save(state);

        Assert.True(File.Exists(_testStatePath));
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesData()
    {
        var store = new StateStore(_testStatePath);
        var originalState = new AppState
        {
            LastLocalTempF = 68.5,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 12, 0, 0),
            LastExtremeSentDate = new DateOnly(2024, 1, 15)
        };

        store.Save(originalState);
        var loadedState = store.Load();

        Assert.Equal(originalState.LastLocalTempF, loadedState.LastLocalTempF);
        Assert.Equal(originalState.LastLocalPersona, loadedState.LastLocalPersona);
        Assert.Equal(originalState.LastLocalSentAt, loadedState.LastLocalSentAt);
        Assert.Equal(originalState.LastExtremeSentDate, loadedState.LastExtremeSentDate);
    }

    [Fact]
    public void Save_OverwritesExistingFile()
    {
        var store = new StateStore(_testStatePath);
        var state1 = new AppState { LastLocalTempF = 65 };
        var state2 = new AppState { LastLocalTempF = 70 };

        store.Save(state1);
        store.Save(state2);
        var loadedState = store.Load();

        Assert.Equal(70, loadedState.LastLocalTempF);
    }

    [Fact]
    public void Load_CorruptJson_ReturnsNewState()
    {
        File.WriteAllText(_testStatePath, "{ this is not valid json }");
        var store = new StateStore(_testStatePath);

        var state = store.Load();

        Assert.NotNull(state);
        Assert.Null(state.LastLocalTempF);
    }

    [Fact]
    public void Load_EmptyFile_ReturnsNewState()
    {
        File.WriteAllText(_testStatePath, "");
        var store = new StateStore(_testStatePath);

        var state = store.Load();

        Assert.NotNull(state);
        Assert.Null(state.LastLocalTempF);
    }

    [Fact]
    public void Load_NullJson_ReturnsNewState()
    {
        File.WriteAllText(_testStatePath, "null");
        var store = new StateStore(_testStatePath);

        var state = store.Load();

        Assert.NotNull(state);
        Assert.Null(state.LastLocalTempF);
    }

    [Fact]
    public void Load_PartialState_LoadsAvailableFields()
    {
        var partialJson = "{\"LastLocalTempF\": 68.5}";
        File.WriteAllText(_testStatePath, partialJson);
        var store = new StateStore(_testStatePath);

        var state = store.Load();

        Assert.Equal(68.5, state.LastLocalTempF);
        Assert.Null(state.LastLocalPersona);
        Assert.Null(state.LastLocalSentAt);
    }

    [Fact]
    public void Save_UsesAtomicWrite_TempFileThenMove()
    {
        var store = new StateStore(_testStatePath);
        var state = new AppState { LastLocalTempF = 68.5 };

        store.Save(state);

        Assert.True(File.Exists(_testStatePath));
        Assert.False(File.Exists(_testStatePath + ".tmp"));
    }

    [Fact]
    public void Save_ProducesIndentedJson()
    {
        var store = new StateStore(_testStatePath);
        var state = new AppState
        {
            LastLocalTempF = 68.5,
            LastLocalPersona = "Porch Poet"
        };

        store.Save(state);
        var json = File.ReadAllText(_testStatePath);

        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void Save_PreservesAllFields()
    {
        var store = new StateStore(_testStatePath);
        var state = new AppState
        {
            LastLocalTempF = 68.5,
            LastLocalPersona = "Porch Poet",
            LastLocalSentAt = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            LastExtremeSentDate = new DateOnly(2024, 1, 15),
            TemplateUsageCount = new Dictionary<string, int>
            {
                ["Porch Poet_template1"] = 3,
                ["Sun Hypeman_template2"] = 5
            }
        };

        store.Save(state);
        var loadedState = store.Load();

        Assert.Equal(state.LastLocalTempF, loadedState.LastLocalTempF);
        Assert.Equal(state.LastLocalPersona, loadedState.LastLocalPersona);
        Assert.Equal(state.LastLocalSentAt, loadedState.LastLocalSentAt);
        Assert.Equal(state.LastExtremeSentDate, loadedState.LastExtremeSentDate);
        Assert.Equal(2, loadedState.TemplateUsageCount.Count);
        Assert.Equal(3, loadedState.TemplateUsageCount["Porch Poet_template1"]);
        Assert.Equal(5, loadedState.TemplateUsageCount["Sun Hypeman_template2"]);
    }

    [Fact]
    public void Load_FileWithExtraFields_IgnoresUnknownFields()
    {
        var jsonWithExtra = @"{
            ""LastLocalTempF"": 68.5,
            ""LastLocalPersona"": ""Porch Poet"",
            ""UnknownField"": ""should be ignored""
        }";
        File.WriteAllText(_testStatePath, jsonWithExtra);
        var store = new StateStore(_testStatePath);

        var state = store.Load();

        Assert.Equal(68.5, state.LastLocalTempF);
        Assert.Equal("Porch Poet", state.LastLocalPersona);
    }

    [Fact]
    public void Load_DateTimeHandling_PreservesUtcKind()
    {
        var store = new StateStore(_testStatePath);
        var originalState = new AppState
        {
            LastLocalSentAt = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc)
        };

        store.Save(originalState);
        var loadedState = store.Load();

        Assert.NotNull(loadedState.LastLocalSentAt);
        Assert.Equal(DateTimeKind.Utc, loadedState.LastLocalSentAt.Value.Kind);
    }

    [Fact]
    public void Load_DateOnlyHandling_WorksCorrectly()
    {
        var store = new StateStore(_testStatePath);
        var originalState = new AppState
        {
            LastExtremeSentDate = new DateOnly(2024, 1, 15)
        };

        store.Save(originalState);
        var loadedState = store.Load();

        Assert.NotNull(loadedState.LastExtremeSentDate);
        Assert.Equal(2024, loadedState.LastExtremeSentDate.Value.Year);
        Assert.Equal(1, loadedState.LastExtremeSentDate.Value.Month);
        Assert.Equal(15, loadedState.LastExtremeSentDate.Value.Day);
    }

    [Fact]
    public void Load_EmptyTemplateUsageCount_ReturnsEmptyDictionary()
    {
        var store = new StateStore(_testStatePath);
        var state = new AppState();

        store.Save(state);
        var loadedState = store.Load();

        Assert.NotNull(loadedState.TemplateUsageCount);
        Assert.Empty(loadedState.TemplateUsageCount);
    }

    [Fact]
    public void Save_NullValues_SerializesCorrectly()
    {
        var store = new StateStore(_testStatePath);
        var state = new AppState
        {
            LastLocalTempF = null,
            LastLocalPersona = null,
            LastLocalSentAt = null,
            LastExtremeSentDate = null
        };

        store.Save(state);
        var loadedState = store.Load();

        Assert.Null(loadedState.LastLocalTempF);
        Assert.Null(loadedState.LastLocalPersona);
        Assert.Null(loadedState.LastLocalSentAt);
        Assert.Null(loadedState.LastExtremeSentDate);
    }

    [Fact]
    public void Load_InvalidPermissions_ReturnsNewState()
    {
        // Create a read-only directory to simulate permission issues
        var readOnlyDir = Path.Combine(_testStateDir, "readonly");
        Directory.CreateDirectory(readOnlyDir);
        var readOnlyPath = Path.Combine(readOnlyDir, "state.json");
        
        // Write a file then make directory readonly (platform-specific)
        File.WriteAllText(readOnlyPath, "{\"LastLocalTempF\": 68.5}");
        
        try
        {
            // Make file readonly
            File.SetAttributes(readOnlyPath, FileAttributes.ReadOnly);
            
            // Create an invalid path to trigger an error
            var invalidPath = Path.Combine(readOnlyDir, "nonexistent", "state.json");
            var store = new StateStore(invalidPath);

            var state = store.Load();

            Assert.NotNull(state);
        }
        finally
        {
            // Clean up
            if (File.Exists(readOnlyPath))
            {
                File.SetAttributes(readOnlyPath, FileAttributes.Normal);
            }
        }
    }

    [Fact]
    public void Save_MultipleTimes_MaintainsConsistency()
    {
        var store = new StateStore(_testStatePath);

        for (int i = 0; i < 10; i++)
        {
            var state = new AppState
            {
                LastLocalTempF = 60 + i,
                LastLocalPersona = $"Persona{i}"
            };
            store.Save(state);
        }

        var finalState = store.Load();
        Assert.Equal(69, finalState.LastLocalTempF);
        Assert.Equal("Persona9", finalState.LastLocalPersona);
    }
}
