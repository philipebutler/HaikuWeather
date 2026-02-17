# WeatherHaikuAgent

**An agentic .NET application that sends persona-based weather haiku emails.**

WeatherHaikuAgent monitors local temperature changes and sends creative haiku emails when meaningful weather changes occur. It also provides a daily digest email featuring the most extreme weather from a curated list of global locations.

## 🎯 Features

- **🌡️ Local Weather Monitoring**: Sends haiku emails when temperature changes significantly or crosses persona boundaries
- **🌍 Daily Extreme Weather Digest**: Once per day, highlights the hottest or coldest location from a global list
- **🎭 Six Weather Personas**: Each temperature range has its own personality (Frost Monk, Snow Comedian, Mud Philosopher, Porch Poet, Sun Hypeman, Heat Dramatic)
- **📧 Smart Email Logic**: Respects cooldown periods and quiet hours to avoid spam
- **💾 Stateful Operation**: Remembers last sent notifications across runs
- **🔧 Fully Configurable**: Customize thresholds, locations, and behavior via config file or environment variables

## 📋 Requirements

- **.NET 10.0 SDK** or later
- **Gmail account** with App Password (for email delivery)
- **Internet connection** (for weather data from Open-Meteo API)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/philipebutler/HaikuWeather.git
cd HaikuWeather
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Project

```bash
dotnet build
```

### 4. Configure the Application

Create an `appsettings.json` file in the root directory by copying the example:

```bash
cp appsettings.example.json appsettings.json
```

Edit `appsettings.json` and configure:

#### **Local Weather Settings**
```json
"Local": {
  "Latitude": 37.7749,
  "Longitude": -122.4194,
  "LocationLabel": "San Francisco"
}
```

#### **Email Settings** (Required)
```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-gmail-app-password",
  "From": "your-email@gmail.com",
  "To": "recipient@example.com",
  "SubjectPrefix": "WeatherHaiku"
}
```

> **Important**: Use a Gmail **App Password**, not your regular password. Generate one at: https://myaccount.google.com/apppasswords

#### **Notification Rules** (Optional)
```json
"Notify": {
  "TempDeltaF": 3.0,               // Minimum temp change to trigger notification
  "MinMinutesBetween": 60,         // Cooldown between notifications (minutes)
  "QuietHoursStart": 7,            // No notifications before this hour (24-hour format)
  "QuietHoursEnd": 21,             // No notifications after this hour
  "AllowQuietHoursOverride": false // Allow urgent notifications during quiet hours
}
```

#### **Extreme Weather** (Optional)
```json
"Extreme": {
  "Enabled": true,
  "DailySendTimeLocal": "07:30",
  "SelectionMode": "HotOrColdByDeparture",
  "ReferenceTempF": 65.0,
  "Locations": [
    { "Name": "Phoenix, AZ", "Latitude": 33.4484, "Longitude": -112.0740 },
    { "Name": "Fairbanks, AK", "Latitude": 64.8378, "Longitude": -147.7164 }
  ]
}
```

### 5. Test Email Configuration

Before scheduling, verify your email settings work:

```bash
cd src/WeatherHaikuAgent
dotnet run -- test-email
```

You should receive a test email within a few seconds.

## 📖 Usage

### Run Modes

#### 1. **Normal Run** (Check weather and send if needed)
```bash
cd src/WeatherHaikuAgent
dotnet run
# or
dotnet run -- run
```

#### 2. **Test Email** (Verify email configuration)
```bash
dotnet run -- test-email
```

#### 3. **Dump Configuration** (View current settings without secrets)
```bash
dotnet run -- dump-config
```

### Scheduled Execution (macOS)

Use `launchd` to run the app every 10-15 minutes:

1. Create a plist file: `~/Library/LaunchAgents/com.weatherhaiku.agent.plist`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.weatherhaiku.agent</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/share/dotnet/dotnet</string>
        <string>run</string>
        <string>--project</string>
        <string>/path/to/HaikuWeather/src/WeatherHaikuAgent</string>
    </array>
    <key>StartInterval</key>
    <integer>600</integer>
    <key>StandardOutPath</key>
    <string>/tmp/weatherhaiku.log</string>
    <key>StandardErrorPath</key>
    <string>/tmp/weatherhaiku.error.log</string>
</dict>
</plist>
```

2. Load the agent:
```bash
launchctl load ~/Library/LaunchAgents/com.weatherhaiku.agent.plist
```

3. Start the agent:
```bash
launchctl start com.weatherhaiku.agent
```

### Scheduled Execution (Linux with cron)

Add to your crontab (`crontab -e`):

```bash
*/10 * * * * cd /path/to/HaikuWeather/src/WeatherHaikuAgent && dotnet run >> /tmp/weatherhaiku.log 2>&1
```

### Scheduled Execution (Windows Task Scheduler)

1. Open Task Scheduler
2. Create Basic Task
3. Set trigger: "Daily" or "At startup"
4. Set action: "Start a program"
5. Program: `dotnet`
6. Arguments: `run --project C:\path\to\HaikuWeather\src\WeatherHaikuAgent`

## 🎭 Weather Personas

The app selects a persona based on current temperature:

| Temperature (°F) | Persona            | Style                                |
|------------------|--------------------|--------------------------------------|
| ≤ 20             | **Frost Monk**     | Zen, contemplative, ancient wisdom   |
| 21 - 40          | **Snow Comedian**  | Humorous, playful winter vibes       |
| 41 - 60          | **Mud Philosopher**| Existential, introspective           |
| 61 - 75          | **Porch Poet**     | Comfortable, grateful, observant     |
| 76 - 90          | **Sun Hypeman**    | Enthusiastic, energetic HYPE         |
| ≥ 91             | **Heat Dramatic**  | Theatrical, intense, dramatic        |

## ⚙️ Configuration Reference

### Environment Variables

All settings can be overridden with environment variables using the format `Section__Key`:

```bash
# Local weather
export Local__Latitude=37.7749
export Local__Longitude=-122.4194
export Local__LocationLabel="San Francisco"

# Email
export Email__Username="your-email@gmail.com"
export Email__Password="your-app-password"
export Email__To="recipient@example.com"

# Notification rules
export Notify__TempDeltaF=5.0
export Notify__MinMinutesBetween=90

# Extreme weather
export Extreme__Enabled=true
export Extreme__DailySendTimeLocal="08:00"
```

### Configuration Sections

- **Local**: Your location coordinates and label
- **Notify**: Notification thresholds and timing rules
- **Extreme**: Daily extreme weather digest settings
- **Haiku**: Haiku generation mode (LocalTemplates or OpenAI)
- **Email**: SMTP email delivery settings
- **State**: Path to state file (default: `./state.json`)
- **Logging**: Log level (Info or Debug)

## 🧪 Running Tests

Run the full test suite:

```bash
dotnet test
```

Run tests with verbose output:

```bash
dotnet test --verbosity normal
```

**Test Coverage**: 117 unit tests covering:
- Persona selection logic
- Decision engine rules (cooldown, quiet hours, thresholds)
- Haiku generation
- State persistence
- Configuration loading

## 📁 Project Structure

```
HaikuWeather/
├── src/
│   └── WeatherHaikuAgent/
│       ├── Models/                    # Data models
│       │   ├── AppConfig.cs
│       │   ├── AppState.cs
│       │   └── WeatherContext.cs
│       ├── AppRunner.cs               # Main orchestration
│       ├── ConfigLoader.cs            # Configuration management
│       ├── DecisionEngine.cs          # Send logic & rules
│       ├── EmailClient.cs             # SMTP email delivery
│       ├── HaikuGeneratorLocalTemplates.cs  # Haiku templates
│       ├── PersonaEngine.cs           # Temperature-based personas
│       ├── StateStore.cs              # JSON state persistence
│       ├── WeatherClient.cs           # Open-Meteo API client
│       └── Program.cs                 # CLI entry point
├── tests/
│   └── WeatherHaikuAgent.Tests/       # Unit tests
├── appsettings.example.json           # Example configuration
├── SPEC.md                            # Full specification
└── README.md                          # This file
```

## 🔒 Security Notes

- **Never commit `appsettings.json`** with real credentials
- Use **Gmail App Passwords**, not your account password
- Store secrets in environment variables or secure vaults in production
- The `.gitignore` file excludes sensitive files

## 🐛 Troubleshooting

### Email not sending

1. Verify Gmail App Password is correct
2. Check that "Less secure app access" is NOT required (App Passwords bypass this)
3. Test with `dotnet run -- test-email`
4. Check logs for SMTP errors

### No notifications received

1. Check temperature delta is sufficient (default: ≥3°F)
2. Verify you're outside quiet hours (default: 7 AM - 9 PM)
3. Check cooldown period hasn't been violated (default: 60 minutes)
4. Review state file: `cat state.json`

### Weather data not loading

1. Verify internet connection
2. Check Open-Meteo API status: https://open-meteo.com/
3. Ensure latitude/longitude are valid

### Build errors

1. Ensure .NET 10.0 SDK is installed: `dotnet --version`
2. Run `dotnet restore` to restore packages
3. Clean and rebuild: `dotnet clean && dotnet build`

## 📊 How It Works

1. **Weather Check**: Fetches current temperature from Open-Meteo API
2. **Decision Logic**: Determines if notification should be sent based on:
   - Temperature delta threshold
   - Persona change
   - Cooldown period
   - Quiet hours
3. **Haiku Generation**: Selects appropriate persona and haiku template
4. **Email Delivery**: Sends formatted email via Gmail SMTP
5. **State Update**: Saves notification timestamp and temperature to prevent duplicates
6. **Extreme Weather**: Once daily, finds most extreme location and sends special haiku

## 🎨 Example Email

```
From: your-email@gmail.com
To: recipient@example.com
Subject: [WeatherHaiku] 78°F — Sun Hypeman

Summer heat arrives
Golden rays embrace the earth
Life moves in slow waves

━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Persona: Sun Hypeman
Temperature: 78°F
Location: San Francisco
Time: 2026-02-17 14:30:00
Trigger: Temperature change (5°F)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 🛠️ Development

### Adding New Personas

Edit `PersonaEngine.cs` to add temperature bands and `HaikuGeneratorLocalTemplates.cs` to add haikus.

### Running in Debug Mode

```bash
export Logging__Level=Debug
dotnet run
```

### Future Enhancements (Not in v1)

- OpenAI-based haiku generation
- Multi-user support
- Rich HTML emails
- Additional weather dimensions (wind, rain, etc.)
- SMS/WhatsApp delivery

## 📄 License

MIT License - See repository for details

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass: `dotnet test`
5. Submit a pull request

## 📞 Support

For issues, questions, or feature requests, please open an issue on GitHub.

---

**Made with ☁️ and ❄️ by an agentic AI experiment**
