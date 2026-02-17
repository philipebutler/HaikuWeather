# WeatherHaikuAgent Test Suite

## Overview

This test suite provides comprehensive unit test coverage for the WeatherHaikuAgent core business logic as specified in SPEC.md Section 10 (Testing Plan). All tests are deterministic, self-contained, and follow xUnit patterns.

## Test Files

### 1. PersonaEngineTests.cs (22 tests)
Tests all persona band boundaries from SPEC.md FR-3.

**Coverage:**
- ✅ Frost Monk (≤20°F) boundary tests
- ✅ Snow Comedian (21-40°F) boundary tests  
- ✅ Mud Philosopher (41-60°F) boundary tests
- ✅ Porch Poet (61-75°F) boundary tests
- ✅ Sun Hypeman (76-90°F) boundary tests
- ✅ Heat Dramatic (≥91°F) boundary tests
- ✅ Exact boundary condition tests (20, 40, 60, 75, 90, 91°F)
- ✅ Consistency tests (same input produces same output)
- ✅ Persona change across boundaries

**Key Tests:**
- `PickPersona_FrostMonk_BoundaryTests`: Tests -40°F, 0°F, 20°F
- `PickPersona_ExactBoundary_*`: Tests exact boundary values
- `PickPersona_PersonaChangesAcrossBoundary`: Verifies transitions

### 2. DecisionEngineTests.cs (43 tests)
Tests all decision rules from SPEC.md FR-5.

**Coverage:**

#### Temperature Delta Rules (6 tests)
- ✅ First run returns true
- ✅ Delta ≥ threshold triggers send
- ✅ Delta < threshold blocks send
- ✅ Exact threshold value (≥ 3°F)
- ✅ Negative delta (temperature drops)

#### Persona Change Rules (3 tests)
- ✅ Persona change triggers send (even with small delta)
- ✅ No persona change + small delta blocks send
- ✅ Persona change across any boundary

#### Cooldown Rules (4 tests)
- ✅ Cooldown not satisfied blocks send
- ✅ Cooldown exactly met (60 minutes) allows send
- ✅ Cooldown exceeded allows send
- ✅ Custom cooldown configuration respected

#### Quiet Hours Rules (7 tests)
- ✅ Within quiet hours (7-21) allows send
- ✅ Before quiet hours start blocks send
- ✅ At quiet hours start (7:00) allows send
- ✅ At quiet hours end (21:00) blocks send
- ✅ After quiet hours end blocks send
- ✅ Quiet hours override flag bypasses restrictions

#### Extreme Weather Rules (10 tests)
- ✅ First run returns true
- ✅ Disabled feature blocks send
- ✅ Already sent today blocks send
- ✅ Sent yesterday allows send today
- ✅ Before send time blocks send
- ✅ At send time (07:30) allows send
- ✅ After send time allows send
- ✅ Invalid time format blocks send
- ✅ Custom send time respected

#### Combined Scenarios (3 tests)
- ✅ Large delta but in cooldown blocks send
- ✅ Large delta but outside quiet hours blocks send
- ✅ Persona change + large delta allows send

**Key Tests:**
- `ShouldSendLocalNotification_*`: Tests local weather notification logic
- `ShouldSendExtremeNotification_*`: Tests daily extreme weather logic

### 3. HaikuGeneratorTests.cs (17 tests)
Tests haiku generation for all personas.

**Coverage:**
- ✅ Each persona has valid haiku templates (6 personas)
- ✅ Invalid persona returns default haiku
- ✅ Deterministic mode consistency (same input → same output)
- ✅ Deterministic mode variation (different dates → different outputs)
- ✅ Random mode produces valid haikus
- ✅ All personas have ≥5 different templates (per SPEC.md FR-4)
- ✅ All haikus have exactly 3 lines
- ✅ Both rotation modes (Deterministic/Random) work

**Key Tests:**
- `GenerateHaiku_*Persona_ReturnsHaiku`: Tests each of 6 personas
- `GenerateHaiku_AllPersonas_HaveMultipleTemplates`: Verifies ≥5 templates
- `GenerateHaiku_HaikuFormat_HasThreeLines`: Format validation
- `GenerateHaiku_DeterministicMode_SameInputsSameOutput`: Determinism test

### 4. StateStoreTests.cs (21 tests)
Tests state persistence and error handling.

**Coverage:**

#### Basic Operations (5 tests)
- ✅ Load with no file returns new state
- ✅ Save creates file
- ✅ Save and load round-trip preserves data
- ✅ Save overwrites existing file
- ✅ Multiple saves maintain consistency

#### Error Handling (4 tests)
- ✅ Corrupt JSON returns new state (no crash)
- ✅ Empty file returns new state
- ✅ Null JSON returns new state
- ✅ Invalid permissions returns new state

#### Data Integrity (7 tests)
- ✅ Partial state loads available fields
- ✅ Atomic write uses temp file + move
- ✅ Produces indented JSON
- ✅ Preserves all fields (temp, persona, timestamp, date, usage counts)
- ✅ File with extra fields ignores unknown fields
- ✅ Null values serialize correctly
- ✅ Empty template usage count handled

#### Type Handling (2 tests)
- ✅ DateTime preserves UTC kind
- ✅ DateOnly handles correctly

**Key Tests:**
- `Load_CorruptJson_ReturnsNewState`: Resilience to bad data
- `Save_UsesAtomicWrite_TempFileThenMove`: Atomic write verification
- `SaveAndLoad_RoundTrip_PreservesData`: Data integrity check

### 5. ConfigLoaderTests.cs (24 tests)
Tests configuration loading from JSON and environment variables.

**Coverage:**

#### Basic Loading (3 tests)
- ✅ No config file returns defaults
- ✅ JSON file loads values
- ✅ Environment variables override JSON

#### Configuration Sections (7 tests)
- ✅ Local config (latitude, longitude, label)
- ✅ Notify config (delta, cooldown, quiet hours)
- ✅ Extreme config (enabled, send time, mode, reference temp)
- ✅ Haiku config (mode, rotation)
- ✅ Email config (SMTP settings)
- ✅ State config (path)
- ✅ Logging config (level)

#### Environment Variables (5 tests)
- ✅ Invalid numeric env var throws exception
- ✅ Invalid boolean env var throws exception
- ✅ Empty string allowed for string fields
- ✅ Mixed JSON + env vars (env takes precedence)
- ✅ All config sections loadable from env vars

#### Complete Configuration (3 tests)
- ✅ Complete JSON loads all sections
- ✅ Multiple locations load correctly
- ✅ Default values for all sections

**Key Tests:**
- `Load_EnvironmentVariables_OverrideJsonConfig`: Precedence test
- `Load_CompleteJsonConfig_LoadsAllSections`: Full config test
- `Load_InvalidNumericEnvironmentVariable_ThrowsException`: Validation test

## Test Statistics

- **Total Tests:** 117
- **Passed:** 117 ✅
- **Failed:** 0
- **Skipped:** 0
- **Duration:** ~150ms

## Running Tests

```bash
# Run all tests
dotnet test tests/WeatherHaikuAgent.Tests/WeatherHaikuAgent.Tests.csproj

# Run with detailed output
dotnet test tests/WeatherHaikuAgent.Tests/WeatherHaikuAgent.Tests.csproj --verbosity normal

# Run specific test class
dotnet test --filter PersonaEngineTests
dotnet test --filter DecisionEngineTests
dotnet test --filter HaikuGeneratorTests
dotnet test --filter StateStoreTests
dotnet test --filter ConfigLoaderTests

# Run with code coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

## Test Design Principles

### Determinism
- No random failures
- Fixed timestamps and temperatures
- Isolated test environment (temp directories for file tests)
- Proper cleanup in Dispose methods

### Self-Contained
- No external dependencies (APIs, databases)
- No shared state between tests
- Each test can run independently
- Proper setup/teardown using IDisposable

### Clear Assertions
- One logical assertion per test
- Descriptive test names
- Theory tests for parameterized scenarios
- Clear error messages

### Coverage Alignment
All tests directly map to SPEC.md requirements:
- FR-3: Persona boundaries → PersonaEngineTests
- FR-5: Decision rules → DecisionEngineTests
- FR-4: Haiku generation → HaikuGeneratorTests
- FR-7: State store → StateStoreTests
- FR-1: Configuration → ConfigLoaderTests

## Test Patterns Used

### xUnit Patterns
- `[Fact]`: Single test case
- `[Theory]` + `[InlineData]`: Parameterized tests
- `IDisposable`: Cleanup resources
- `Assert.*`: Rich assertion library

### Test Organization
- Arrange-Act-Assert structure
- Helper methods for common setup
- Region grouping for related tests
- Descriptive test method names

## Future Test Additions

Potential areas for expansion (not required for v1):
- Integration tests with real weather API
- Email sending tests (mock SMTP)
- End-to-end workflow tests
- Performance/load tests
- Extreme weather selection algorithm tests
- Template rotation exhaustiveness tests

## Notes

- Tests use temporary directories that are cleaned up automatically
- Environment variables are saved and restored to avoid test pollution
- StateStore tests verify atomic writes (temp file + move pattern)
- ConfigLoader tests verify environment variable precedence over JSON
- All persona boundaries from SPEC.md Table (FR-3) are tested
- All decision rules from SPEC.md FR-5 are covered
