Perfect choice 👍
Starting with **LocalTemplates** keeps this fun, reliable, and very “agentic” without any external friction.

Below is a **complete, clean, repo-ready `SPEC.md`** that incorporates **everything we discussed**, written explicitly to work well with **GitHub Copilot–assisted development**. You can drop this in as-is.

---

# WeatherHaikuAgent

**Specification v1.0 (Local Templates First, AI-Ready)**

---

## 1. Purpose

`WeatherHaikuAgent` is a lightweight, agent-style .NET application that:

1. Monitors **local temperature changes** and sends persona-based haiku emails when meaningful changes occur.
2. Sends a **daily “Extreme Weather Somewhere”** haiku describing an unusually hot or cold location from a curated global list.
3. Operates unattended via a scheduler (macOS), using **stateful decision logic** to avoid spam.

The system is intentionally simple, deterministic, and fun—while being architected so that **AI-based haiku generation (ChatGPT/OpenAI)** can be enabled later without redesign.

---

## 2. Design Principles

* **Agentic, not chatty**
  The app observes → decides → acts, without user interaction.
* **Idempotent & stateful**
  Safe to run repeatedly on a schedule.
* **Local-first**
  No AI dependency required for v1.
* **Explicit boundaries**
  Clear separation between sensing, decision-making, and action.
* **Copilot-friendly**
  Small, testable components with clear contracts.

---

## 3. Core Behaviors

### 3.1 Local Weather Haiku (Event-Driven)

* The app polls local temperature data.
* A haiku email is sent when:

  * Temperature changes by a configurable delta (default **≥ 3°F**), OR
  * The temperature crosses into a new **persona band**
* Cooldown and quiet hours are enforced.

### 3.2 Daily Extreme Weather Haiku (Scheduled Digest)

* Once per day, the app:

  1. Fetches temperatures for a curated list of global locations
  2. Selects the most **extreme hot or cold** location (by departure from a reference temperature)
  3. Sends a themed haiku describing that location

> This is “extreme somewhere fun,” not a claim of global records.

---

## 4. Non-Goals (v1)

* No UI
* No SMS, WhatsApp, or Teams delivery
* No background daemon inside the app
* No weather alerts (wind, rain, etc.)
* No AI calls by default

---

## 5. User Stories & Acceptance Criteria

### US-1: Local temperature notifications

**As a user**, I want to receive an email haiku when the weather meaningfully changes.

**Acceptance Criteria**

* A haiku is sent if:

  * `abs(currentTemp - lastNotifiedTemp) ≥ threshold`, OR
  * Persona band changes
* Cooldown (default 60 minutes) prevents rapid repeats
* Quiet hours block messages unless overridden
* State persists across runs

---

### US-2: Daily extreme weather haiku

**As a user**, I want one daily haiku about an extreme temperature somewhere.

**Acceptance Criteria**

* Only one extreme email per calendar day
* If some locations fail to load, remaining ones are used
* If all locations fail, no email is sent
* Email includes location name and temperature

---

### US-3: Reliability & statefulness

**As a user**, I want the app to behave consistently and recover gracefully.

**Acceptance Criteria**

* Missing or corrupt state file does not crash the app
* Failed email sends do not update “last sent” state
* App is safe to run repeatedly

---

## 6. Functional Requirements

---

### FR-1 Configuration

Configuration is loaded from:

1. `appsettings.json` (optional)
2. Environment variables (override)

#### Configuration Keys

##### Local Weather

* `Local.Latitude` (double)
* `Local.Longitude` (double)
* `Local.LocationLabel` (string)

##### Notification Rules

* `Notify.TempDeltaF` (double, default `3.0`)
* `Notify.MinMinutesBetween` (int, default `60`)
* `Notify.QuietHoursStart` (int, default `7`)
* `Notify.QuietHoursEnd` (int, default `21`)
* `Notify.AllowQuietHoursOverride` (bool, default `false`)

##### Extreme Weather

* `Extreme.Enabled` (bool, default `true`)
* `Extreme.DailySendTimeLocal` (HH:mm, default `07:30`)
* `Extreme.SelectionMode`

  * `HotOrColdByDeparture` (default)
  * `HotOnly`
  * `ColdOnly`
* `Extreme.ReferenceTempF` (double, default `65`)
* `Extreme.Locations[]`

  * `Name`
  * `Latitude`
  * `Longitude`

##### Haiku Generation

* `Haiku.Mode`

  * `LocalTemplates` (default)
  * `OpenAI` (future)
* `Haiku.TemplateRotationMode`

  * `Deterministic`
  * `Random`

##### Email (Gmail SMTP)

* `Email.SmtpHost` (`smtp.gmail.com`)
* `Email.SmtpPort` (`587`)
* `Email.Username`
* `Email.Password` (Gmail App Password)
* `Email.From`
* `Email.To`
* `Email.SubjectPrefix` (default `WeatherHaiku`)

##### Runtime

* `State.Path` (default `./state.json`)
* `Logging.Level` (`Info`, `Debug`)

---

### FR-2 Weather Provider

* Default provider: **Open-Meteo**
* No API key required
* Temperature unit: Fahrenheit

**Function**

```
GetCurrentTemperatureF(latitude, longitude) → double
```

**Resilience**

* Timeout: 10–20 seconds
* One retry on transient failure

---

### FR-3 Persona Engine

Persona is selected based on temperature:

| Temp °F | Persona         |
| ------: | --------------- |
|    ≤ 20 | Frost Monk      |
|   21–40 | Snow Comedian   |
|   41–60 | Mud Philosopher |
|   61–75 | Porch Poet      |
|   76–90 | Sun Hypeman     |
|    ≥ 91 | Heat Dramatic   |

**Function**

```
PickPersona(tempF) → string
```

---

### FR-4 Haiku Generation (LocalTemplates)

* Each persona has **≥ 5 haiku templates**
* Templates are exactly 3 lines (5/7/5 not strictly enforced)
* Template selection:

  * Deterministic (seeded by date/persona/temp), OR
  * Random (with optional repeat avoidance)

**Function**

```
GenerateHaiku(persona, context) → string
```

**Context includes**

* Temperature
* Location
* Timestamp
* Trigger type (local / extreme)

---

### FR-4B (Provisioned but Disabled): OpenAI Haiku Mode

* When `Haiku.Mode=OpenAI`, the app uses the OpenAI Responses API.
* Requires `OPENAI_API_KEY`.
* ChatGPT subscriptions do **not** include API access.
* If OpenAI call fails → fallback to LocalTemplates.

> This mode is explicitly provisioned but **not required** for v1 completion.

---

### FR-5 Decision Engine

#### Local send condition

```
SendLocal =
  (abs(tempF - lastTempF) ≥ threshold OR persona changed)
  AND cooldown satisfied
  AND (within quiet hours OR override enabled)
```

#### Extreme send condition

```
SendExtreme =
  Extreme.Enabled
  AND lastExtremeSentDate != today
  AND currentTime >= configured send time
```

---

### FR-6 Email Delivery

* Plain-text email via SMTP with STARTTLS
* Gmail requires App Password
* Email subject examples:

  * `[WeatherHaiku] 68°F — Porch Poet`
  * `[WeatherHaiku] Extreme: −12°F in Yakutsk — Frost Monk`

Email body includes:

* Haiku
* Persona
* Temperature
* Location
* Timestamp
* Trigger reason

---

### FR-7 State Store

State is persisted to JSON.

**Fields**

* `LastLocalTempF`
* `LastLocalPersona`
* `LastLocalSentAt`
* `LastExtremeSentDate`
* Optional: template usage tracking

**Requirements**

* Atomic write (write temp → replace)
* Corrupt state resets safely

---

### FR-8 CLI Modes

Executable supports:

* `run` (default)
* `test-email`
* `dump-config` (no secrets)

---

## 7. System Design

### 7.1 Modules

* `ConfigLoader`
* `WeatherClient`
* `PersonaEngine`
* `DecisionEngine`
* `HaikuGeneratorLocalTemplates`
* `EmailClient`
* `StateStore`
* `AppRunner`

### 7.2 Dependency Rules

* Decision logic does **not** depend on email or weather APIs
* Haiku generation is isolated behind an interface
* External services are leaf dependencies

---

## 8. Scheduler Integration (macOS)

* Use `launchd` to run every 10–15 minutes
* App internally enforces cooldowns and daily gating

---

## 9. Default Extreme Locations (Example)

* Phoenix, AZ
* Riyadh, SA
* Fairbanks, AK
* Yakutsk, RU
* Reykjavik, IS
* Dubai, AE

User may modify freely.

---

## 10. Testing Plan

### Unit Tests

* Persona band boundaries
* Decision rules (delta, persona change, cooldown, quiet hours)
* Extreme selection logic
* State load/save behavior

### Manual Tests

* `test-email` works
* No duplicate sends within cooldown
* Extreme email sends once/day

---

## 11. Security Notes

* Gmail App Passwords only
* No secrets committed to repo
* Prefer environment variables
* Future: macOS Keychain integration

---

## 12. Repository Layout (Suggested)

```
WeatherHaikuAgent/
  src/
  tests/
  SPEC.md
  README.md
  appsettings.example.json
```

---

## 13. Definition of Done (v1)

* Runs locally on macOS via `dotnet run`
* Works under scheduled execution
* LocalTemplates haiku generation only
* Daily extreme email works
* State prevents spam
* Unit tests cover core logic
* SPEC.md + example config included

---

## 14. Future Extensions (Explicitly Out of Scope)

* OpenAI haiku generation
* Rich HTML emails
* Multiple delivery channels
* Additional weather dimensions
* Multi-user support

---

If you want next steps, I can:

* Generate a **Copilot task breakdown** (issue list)
* Generate **starter code skeletons** that match this spec exactly
* Create a **launchd plist** and Gmail App Password setup guide
* Add a **persona/haiku content pack** so you don’t have to invent poems

Just tell me how far you want to take the fun 😄
