[README.md](https://github.com/user-attachments/files/25875299/README.md)
# Neuraflex — Cognitive Training Game (C# / WinForms)

**Neuraflex** is a Windows Forms application designed as a cognitive rehabilitation and training tool. It challenges players to complete timed tasks requiring memory, attention, and sequential reasoning — with a special mode tailored for Activities of Daily Living (ADL) support for elderly users.

## Game Modes

| Mode | Description |
|---|---|
| **Basic Mode** | Click colored numbered dots in the correct sequence before time runs out |
| **Context Mode (ADL)** | Complete real-life daily tasks in order — sorting trash, calling emergency services, taking medication |

## Difficulty Levels

| Level | Description |
|---|---|
| `Easy` | Fewer targets, sequence shown in order, no wave time limit |
| `Normal` | Balanced target and distractor count, randomized sequence |
| `Hard` | More targets, more distractors, tighter scoring |

## How It Works

- The player selects a game mode and difficulty from the main menu.
- In **Basic Mode**, a wave of colored circular buttons appears on screen. The player must click the correct-colored dots in the numbered sequence shown.
- In **Context Mode**, the player is shown a sequence of daily-life tasks represented by icons (trash bins, phone calls, pills). They must click the correct items in the right order.
- Each wave is timed. Correct clicks award points; wrong clicks and timeouts penalize the score.
- After gameplay ends, a **test phase** quizzes the player on what they just did — reinforcing memory retention.

## Project Structure

```
├── Program.cs              # Application entry point
├── Form1.cs                # Main UI logic, game flow, panel management
├── Form1.Designer.cs       # WinForms designer boilerplate
├── GameLogic.cs            # Core game engine: scoring, wave generation, click validation
├── CircularButton.cs       # Custom circular button control with image rendering
├── Enums.cs                # Game state, color, context item, and difficulty enums
└── App.config              # .NET 4.7.2 runtime configuration
```

## Game Elements (Context Mode)

### Trash Sorting
| Item | Icon Color | Task |
|---|---|---|
| `TrashPaper` | Blue | Throw away newspaper (paper bin) |
| `TrashPlastic` | Yellow | Throw away bottle (plastic bin) |
| `TrashGlass` | Green | Throw away jar (glass bin) |
| `TrashMixed` | Gray | Throw away general waste (distractor) |

### Emergency Calls
| Item | Task |
|---|---|
| `Phone112` | Call 112 (accident / universal SOS) |
| `Phone999` | Call ambulance (999) |
| `Phone998` | Call fire brigade (998) |
| `Phone997` | Call police (997) |
| `PhoneFamily` | Call grandson (505) |

### Medication
| Item | Task |
|---|---|
| `PillMorning` | Take morning medication |
| `PillEvening` | Take evening/sleep medication |
| `PillPain` | Take pain medication |

## Scoring

| Action | Points |
|---|---|
| Correct click (Basic Mode) | +100 |
| Correct click (Context Mode) | +150 |
| Wrong click | -50 |
| Wave timeout | -200 |

## Post-Game Test Phase

After gameplay, players are tested on what they did during the session:
- **Basic Mode test:** Players are shown color buttons and must recall the sequence of colors that appeared.
- **Context Mode test:** Players are shown multiple-choice questions asking them to identify which tasks they performed, in what order.

This reinforces short-term memory and supports cognitive assessment.

## Architecture Notes

- `GameLogic` handles all game state independently of the UI — score, level progression, wave generation, sequence validation, and history tracking.
- `CircularButton` is a custom `Button` subclass that clips itself into a circle using `GraphicsPath` and renders embedded icon images with anti-aliasing.
- All game panels (`pnlMenu`, `pnlGame`, `pnlTest`, etc.) are dynamically built in code — no Designer XML layout.
- Collision detection prevents dots from overlapping during placement (up to 200 placement attempts per dot).

## Requirements

- Windows OS
- .NET Framework 4.7.2
- Visual Studio (recommended for building)

## Running the Project

1. Open the solution in **Visual Studio**.
2. Make sure image resources (`mozg`, `papier`, `plastik`, `szklo1`, etc.) are present in `Properties/Resources`.
3. Build and run — the app launches directly into the main menu.

## Technologies Used

- **C#**
- **.NET Framework 4.7.2**
- **Windows Forms (WinForms)**
- **GDI+ / System.Drawing** (custom button rendering)
