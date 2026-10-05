# DragonHatchling
Project for trying out agentic workflows to create a little transparent window desktop dragon pet.

The prototype provides Hatch, Feed, Play, local saves, placement recovery, dragging, always-on-top, position reset, and Exit in a compact transparent window. Milestone 4 is complete under the user's revised acceptance criteria, with private delivery packaging, tester run notes, and resource/endurance validation. Separate-machine/VM testing was waived; PC sleep explains the sampling gap.

The source of truth is [ARCHITECTURE.md](ARCHITECTURE.md) at the repository root. See [Milestone 0 findings](docs/MILESTONE_0_FINDINGS.md), [Milestone 1 results](docs/MILESTONE_1_RESULTS.md), [Milestone 2 results](docs/MILESTONE_2_RESULTS.md), [Milestone 3 results](docs/MILESTONE_3_RESULTS.md), and [Milestone 4 results](docs/MILESTONE_4_RESULTS.md) for validation and limitations.

## Run the exported application

On Windows x64, launch `artifacts/win-x64/DragonHatchling.Desktop.exe`. Keep the entire export folder together; it includes the .NET runtime and does not require a separate runtime installation. Build outputs are ignored by Git.

- Click **Hatch** to reveal the baby dragon. Hatch is disabled during the reaction and afterward; clicks are never queued.
- Click **Feed** or **Play** on the idle baby. Both return to idle in about two seconds and can be repeated. All interaction buttons are disabled while a reaction runs; movement and window controls remain available.
- Drag the visible egg or dragon to move the window. Transparent margins are not reliable drag handles.
- Toggle **Always on top** to keep it visible over other ordinary applications.
- **Reset position** or **Ctrl+Home** centers it on the primary display's work area.
- Use the taskbar or **Alt+Tab** to find it; **Exit** or **Alt+F4** closes it.
- First launch starts with a centered egg and always-on-top off. Later launches restore the saved Egg/Baby stage, position, and topmost preference. Closing during hatch reopens at Baby/Idle when saving succeeded.
- Saves live at `%LOCALAPPDATA%/DragonHatchling/save.json`, with the previous valid save in `save.json.bak`. Recovery messages appear in the action strip; see [recovery instructions](docs/BUILD_AND_RUN.md#save-recovery).

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

Create the private delivery ZIP, bundled [tester run notes](docs/PROTOTYPE_RUN_NOTES.md), and SHA-256 checksum with:

```powershell
./scripts/Package.ps1 -Dotnet ./.tools/dotnet/dotnet.exe
```

The result is `artifacts/DragonHatchling-win-x64.zip`. See [delivery and resource validation](docs/BUILD_AND_RUN.md#package-for-private-testers) for reproduction steps.

Run the dependency-free lifecycle checks (nonzero exit on failure):

```powershell
./.tools/dotnet/dotnet.exe run --project tests/DragonHatchling.Tests -c Release -f net10.0
```

Use `dotnet` instead of the local SDK path on other machines. The tests check lifecycle, persistence/recovery, and placement rules. Add `-f net10.0-windows` instead to include WPF Exit-during-reaction and restart checks using temporary isolated saves.

Placeholder PNGs can be regenerated on Windows with `./scripts/New-PlaceholderArt.ps1`. Each uses a transparent 160 × 160 canvas and a shared bottom anchor at (80, 140).

Optional event logging:

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe --diagnostics ./artifacts/window-events.log
```

The log records activation, deactivation, dragging, position reset, DPI, topmost changes, reaction acceptance/completion/cancellation, lifecycle/activity, window-state changes, and closure. Its parent directory must exist; an unwritable log does not interrupt the app.
