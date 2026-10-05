# DragonHatchling
Project for trying out agentic workflows to create a little transparent window desktop dragon pet.

Milestone 1 implements the hatching slice: a placeholder egg, explicit Hatch action, short crack/reveal animation, and a baby dragon that returns to idle. The compact transparent window retains dragging, always-on-top, position reset, and Exit. Feed, Play, continuous idle motion, and persistence belong to later milestones.

The source of truth is [ARCHITECTURE.md](ARCHITECTURE.md) at the repository root. See [Milestone 0 findings](docs/MILESTONE_0_FINDINGS.md) and [Milestone 1 results](docs/MILESTONE_1_RESULTS.md) for validation and limitations.

## Run the exported application

On Windows x64, launch `artifacts/win-x64/DragonHatchling.Desktop.exe`. Keep the entire export folder together; it includes the .NET runtime and does not require a separate runtime installation. Build outputs are ignored by Git.

- Click **Hatch** to reveal the baby dragon. Hatch is disabled during the reaction and afterward; clicks are never queued.
- Drag the visible egg or dragon to move the window. Transparent margins are not reliable drag handles.
- Toggle **Always on top** to keep it visible over other ordinary applications.
- **Reset position** or **Ctrl+Home** centers it on the primary display's work area.
- Use the taskbar or **Alt+Tab** to find it; **Exit** or **Alt+F4** closes it.
- Each launch starts with an egg, centered, with always-on-top off. Lifecycle and preferences are not saved until Milestone 3.

## Build

For complete build, run, test, and troubleshooting instructions, see [Build and run](docs/BUILD_AND_RUN.md).

Requires Windows x64 and the .NET 10 SDK selected by `global.json` (10.0.401 with patch roll-forward).

```powershell
./scripts/Build.ps1
```

For this workspace, a checksum-verified SDK was extracted locally rather than installed globally:

```powershell
./scripts/Build.ps1 -Dotnet ./.tools/dotnet/dotnet.exe
```

The script publishes Release, self-contained, Windows x64 output to `artifacts/win-x64`. The application uses only WPF and the .NET libraries.

Run the dependency-free lifecycle checks (nonzero exit on failure):

```powershell
./.tools/dotnet/dotnet.exe run --project tests/DragonHatchling.Tests -c Release
```

Use `dotnet` instead of the local SDK path on other machines. The small test project compiles the same plain C# lifecycle sources without WPF, checking the one-way transition, commands while busy, and duplicate completion callbacks.

Placeholder PNGs can be regenerated on Windows with `./scripts/New-PlaceholderArt.ps1`. Each uses a transparent 160 × 160 canvas and a shared bottom anchor at (80, 140).

Optional event logging (no saved preferences):

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe --diagnostics ./artifacts/window-events.log
```

The log records activation, deactivation, dragging, position reset, DPI, topmost changes, hatch acceptance/completion, lifecycle/activity, and closure. Its parent directory must exist; an unwritable log does not interrupt the app.
