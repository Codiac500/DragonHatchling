# Build and run DragonHatchling

These instructions describe the current Milestone 2 application. Run PowerShell commands from the repository root, the directory containing `global.json` and `scripts`.

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

Keep **the entire publish folder together** when copying the application. The EXE depends on neighboring files, including its bundled .NET runtime. A separate runtime installation is not required to run this export. Build outputs are ignored by Git, so build first on a fresh clone. Separate-machine delivery remains an outstanding Milestone 4 validation task.

Close any running copy before publishing again so its files can be replaced.

## Use the application

1. Launch to see an egg in a compact transparent window.
2. Click **Hatch**. The egg shakes, cracks, and reveals a baby dragon in about two seconds.
3. The baby returns to **Idle**. Hatch stays disabled; the egg can hatch only once per launch.
   Click **Feed** for food and chewing, or **Play** for happy eyes, sparkles, and hops. Each reaction lasts about two seconds, returns to idle breathing, and is repeatable. Interaction clicks while busy are ignored rather than queued. Window controls remain available.
4. Drag the visible egg or dragon to move the window. Use opaque artwork rather than transparent margins.
5. Toggle **Always on top** to keep the pet above ordinary windows.
6. Click **Reset position**, or press **Ctrl+Home** while the pet window has focus, to center it on the primary display.
7. Use the taskbar or **Alt+Tab** to recover a covered window. Click **Exit** or press **Alt+F4** to close it.

Every launch currently starts with an egg, centered, with always-on-top off. Lifecycle and window preferences are not yet saved. Idle breathing uses a WPF animation capped at 12 frames per second and stops while minimized; reactions use finite asynchronous sequences.

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

## Run lifecycle tests

```powershell
dotnet run --project tests/DragonHatchling.Tests/DragonHatchling.Tests.csproj -c Release
```

Or, with the local SDK:

```powershell
./.tools/dotnet/dotnet.exe run --project tests/DragonHatchling.Tests/DragonHatchling.Tests.csproj -c Release
```

The dependency-free executable prints `PASS` on success and exits nonzero on failure. It checks initial state, the one-way hatch transition, egg restrictions, repeatable Feed/Play, rapid mixed commands while busy, and wrong/duplicate completion callbacks. Desktop rendering, input, DPI, and focus behavior require application-level checks; see [Milestone 2 results](MILESTONE_2_RESULTS.md).

## Optional diagnostics

Launch the export with a log path:

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe --diagnostics ./artifacts/window-events.log
```

The parent directory must already exist. Publishing creates `artifacts`, so this example works after a build. The log records window activation, movement, DPI, topmost changes, reaction acceptance/completion/cancellation, lifecycle/activity, window-state changes, and closure. An unwritable log does not interrupt the application. These diagnostics do not save pet progress or preferences.

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
| Window is covered or misplaced | Use the taskbar or Alt+Tab, then Reset position or Ctrl+Home. Relaunching also centers it. |
| Relaunch shows an egg again | Expected in Milestone 2; persistence is planned for Milestone 3. |

The existing validation covers Milestone 0 at 100% scaling and Milestone 1 at observed 150% scaling. Mixed-DPI monitor behavior, 200% scaling, and monitor removal remain unverified. See [Milestone 0 findings](MILESTONE_0_FINDINGS.md) and [Milestone 1 results](MILESTONE_1_RESULTS.md) for the recorded limits.
