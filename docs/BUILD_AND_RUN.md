# Build and run DragonHatchling

These instructions describe the Milestone 4 prototype delivery. Run PowerShell commands from the repository root, the directory containing `global.json` and `scripts`.

## Requirements

- Windows 11 x64, the current application target. The UI uses WPF and runs on Windows only.
- To build: .NET SDK **10.0.401**, or a newer patch in the same SDK feature band allowed by `global.json` (`latestPatch`). A runtime installation alone is insufficient.
- PowerShell to run the build script.
- Internet access for the first restore if required framework/runtime packages are not already cached.

The application has no third-party package dependencies. Placeholder PNGs are included in the repository; generating new artwork is not required to build.

## Build with an installed SDK

Check that the SDK is available:

```powershell
dotnet --list-sdks
dotnet --version
```

Run the build script:

```powershell
./scripts/Build.ps1
```

The script restores dependencies and publishes a **Release, self-contained Windows x64** application to `artifacts/win-x64`. A successful publish returns without a `Publish failed` error.

## Build with this workspace's local SDK

This development workspace has a checksum-verified SDK extracted into `.tools/dotnet`. That directory is ignored by Git and is not included in a fresh clone. If it exists, use:

```powershell
./scripts/Build.ps1 -Dotnet ./.tools/dotnet/dotnet.exe
```

If the local SDK is absent, use an installed matching SDK and the previous section. The build script accepts another SDK executable path through `-Dotnet`:

```powershell
./scripts/Build.ps1 -Dotnet 'C:/path/to/dotnet.exe'
```

## Run the published application

After building, run:

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe
```

Alternatively, open `artifacts/win-x64` in File Explorer and double-click `DragonHatchling.Desktop.exe`.

Keep **the entire publish folder together** when copying the application. The EXE depends on neighboring files, including its bundled .NET runtime. A separate runtime installation is not required to run this export. Build outputs are ignored by Git, so build first on a fresh clone. Separate-machine/VM testing was waived for Milestone 4 by the user.

Close any running copy before publishing again so its files can be replaced.

## Package for private testers

```powershell
./scripts/Package.ps1 -Dotnet ./.tools/dotnet/dotnet.exe
```

Use `-Dotnet dotnet` with an installed matching SDK. Packaging publishes to a
fresh directory under ignored `artifacts/package-*/win-x64`, includes
[prototype run notes](PROTOTYPE_RUN_NOTES.md) beside the EXE, and creates
`artifacts/DragonHatchling-win-x64.zip` plus a `.zip.sha256` checksum. The ZIP
contains one `win-x64` folder, with the complete self-contained export. It
contains no SDK, development saves, diagnostic logs, or test runner. Later
packaging runs replace the ZIP and checksum. Extract to a new folder for each
delivery check; do not overlay a previous export.

The run notes include optional broader desktop and delivery checks.
Milestone 4 is complete under the user's revised acceptance criteria;
separate-machine/VM testing is not required for this prototype. See
[Milestone 4 results](MILESTONE_4_RESULTS.md) for the executed checks and limits.

## Measure resource use

Launch the extracted EXE, use Task Manager to find its process ID, then run:

```powershell
./scripts/Measure-Resources.ps1 -ProcessId 12345 -Minutes 15 -OutputPath ./artifacts/validation/session.csv
```

Replace `12345` with the actual process ID. Use a new output filename for each
run. The script records a baseline and samples every 30 seconds for at least
15 minutes. CPU is the interval process CPU divided by wall time and logical
processor count, expressed as percent of the machine. It also records working
set, private memory, handles, threads, and Windows process responsiveness.
It stops with an error if the process exits, its ID is reused, or it becomes
unresponsive, and warns if sampling takes more than twice the requested
interval. Such gaps interrupt continuous observation and can dilute wall-time
CPU averages. It does not launch or close the pet. Review trends after warm-up
against the provisional 1% idle CPU and 150 MiB working-set investigation
thresholds. Process responsiveness does not prove mouse input or focus works.

For an additional automated WPF endurance check:

```powershell
./.tools/dotnet/dotnet.exe run --project tests/DragonHatchling.Tests -c Release -f net10.0-windows -- --endurance-minutes 15
```

This uses production WPF windows, repeated reactions and busy command guards,
topmost toggles, Reset, and Exit/restart with isolated temporary saves. It
fails if a reaction stays busy for more than five seconds or controls fail
to restore. It supplements the desktop session; it does not inject OS mouse
input, drag windows, or establish focus/transparency behavior.

## Use the application

1. First launch shows an egg in a compact transparent window. Subsequent launches restore the saved stage and window preferences.
2. Click **Hatch**. The egg shakes, cracks, and reveals a baby dragon in about two seconds.
3. The baby returns to **Idle**. Hatch stays disabled; the saved baby cannot hatch again.
   Click **Feed** for food and chewing, or **Play** for happy eyes, sparkles, and hops. Each reaction lasts about two seconds, returns to idle breathing, and is repeatable. Interaction clicks while busy are ignored rather than queued. Window controls remain available.
4. Drag the visible egg or dragon to move the window. Use opaque artwork rather than transparent margins.
5. Toggle **Always on top** to keep the pet above ordinary windows.
6. Click **Reset position**, or press **Ctrl+Home** while the pet window has focus, to center it on the primary display.
7. Use the taskbar or **Alt+Tab** to recover a covered window. Click **Exit** or press **Alt+F4** to close it.

Saves use `%LOCALAPPDATA%/DragonHatchling/save.json`. Stage is saved before the hatch reveal; position after dragging or Reset; topmost after toggling. Temporary reactions are never saved. Restored placement clamps to a monitor work area or centers on the primary display if its monitor is gone. Idle breathing uses a WPF animation capped at 12 frames per second and stops while minimized; reactions use finite asynchronous sequences.

## Save recovery

The JSON schema version is 1: `stage` is `Egg` or `Baby`; `x`/`y` are the window origin in physical virtual-desktop pixels (negative coordinates are legal); `alwaysOnTop` is a Boolean. `save.json.bak` retains the previous valid save. Writes use a unique temporary file in the same directory, flush it, then atomically replace the primary file. Abandoned temporary files are ignored.

A malformed, incomplete, out-of-range, or newer-version save is preserved at its original path. The app loads a valid backup if available, otherwise an egg, and shows a recovery message. Saving remains paused while a suspect primary or backup exists, so gameplay during that recovery session is not retained. To resume saving, close the pet, copy the affected files somewhere safe, and move the suspect files out of the save directory. If the backup is valid, copy it to `save.json` before relaunching. Do not edit a newer-version file into this schema; retain it for a compatible version.

“Progress could not be saved” means a write failed. The pet stays usable; resolve storage permissions/availability and retry by toggling topmost or using Reset position. A successful retry saves the current stage and preferences. A hatch that could not be saved can be lost on restart.

For isolated development checks, override the save location:

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe --save-path ./artifacts/test-save.json
```

This override is optional; normal launches use LocalAppData. Run one copy against a save path at a time; simultaneous copies are not coordinated.

## Run from source during development

With an installed SDK:

```powershell
dotnet run --project src/DragonHatchling.Desktop/DragonHatchling.Desktop.csproj -c Debug
```

With the repository-local SDK:

```powershell
./.tools/dotnet/dotnet.exe run --project src/DragonHatchling.Desktop/DragonHatchling.Desktop.csproj -c Debug
```

Use the published EXE for validating the self-contained export. Running from source uses the development SDK/runtime environment.

## Run lifecycle, persistence, and placement tests

```powershell
dotnet run --project tests/DragonHatchling.Tests/DragonHatchling.Tests.csproj -c Release -f net10.0
```

Or, with the local SDK:

```powershell
./.tools/dotnet/dotnet.exe run --project tests/DragonHatchling.Tests/DragonHatchling.Tests.csproj -c Release -f net10.0
```

The dependency-free executable prints `PASS` on success and exits nonzero on failure. It checks lifecycle/interaction rules, save round trips and backups, restart during hatch, interrupted writes, malformed/newer saves, unavailable storage, and negative/clipped/lost-monitor placement. Use `-f net10.0-windows` to also open temporary WPF windows, invoke Exit during each reaction, and verify Baby/Idle on reopen. Both targets use isolated temporary saves. The Windows target requires the .NET 10 Windows Desktop runtime supplied by the development SDK; it does not represent a clean-machine check. Desktop rendering, input, DPI, and focus behavior require application-level checks; see [Milestone 3 results](MILESTONE_3_RESULTS.md).

## Optional diagnostics

Launch the export with a log path:

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe --diagnostics ./artifacts/window-events.log
```

The parent directory must already exist. Publishing creates `artifacts`, so this example works after a build. The log records window activation, movement, DPI, topmost changes, save success/failure, display recovery, reaction acceptance/completion/cancellation, lifecycle/activity, window-state changes, and closure. Diagnostic coordinates are WPF device-independent units; saved coordinates are physical pixels. An unwritable log does not interrupt the application.

## Optional placeholder art generation

To regenerate the included egg, cracked egg, baby, eating, and playing PNGs on Windows:

```powershell
./scripts/New-PlaceholderArt.ps1
```

This replaces those five files under `src/DragonHatchling.Desktop/Assets`. Rebuild to include them in the export. All five use a transparent 160 × 160 canvas with a shared bottom anchor at (80, 140).

## Troubleshooting

| Symptom | Action |
| --- | --- |
| `dotnet` is not recognized | Make the matching SDK available on PATH, or pass its executable path to `Build.ps1 -Dotnet`. |
| Requested SDK cannot be found | Check `dotnet --list-sdks` against `global.json`. .NET 8/9 or a .NET 10 runtime alone cannot build this project. |
| Restore fails | Read the reported error; check network access, NuGet configuration permissions, and package-cache availability. |
| Publish cannot replace a file | Exit all running copies of DragonHatchling, then rebuild. |
| Published EXE is missing | Run the publish script; ignored build outputs are not present in a fresh clone. |
| Copied EXE fails to start | Copy the entire `artifacts/win-x64` folder, preserving its files and subdirectories. |
| Window is covered or misplaced | Use the taskbar or Alt+Tab, then Reset position or Ctrl+Home. Unavailable saved monitor positions center on relaunch. |
| Relaunch shows an egg again | Check the action-strip recovery message and save directory; see Save recovery above. |

The existing validation covers Milestone 0 at 100% scaling and Milestone 1 at observed 150% scaling. Mixed-DPI monitor behavior, 200% scaling, and monitor removal remain unverified. See [Milestone 0 findings](MILESTONE_0_FINDINGS.md) and [Milestone 1 results](MILESTONE_1_RESULTS.md) for the recorded limits.
