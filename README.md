# DragonHatchling
Project for trying out agentic workflows to create a little transparent window desktop dragon pet.

Milestone 0 implements a disposable Windows desktop feasibility spike: one transparent PNG, dragging, always-on-top, position reset, and Exit. No hatching, gameplay, animation, or persistence is implemented.

The source of truth is [ARCHITECTURE.md](ARCHITECTURE.md) at the repository root. See [Milestone 0 findings](docs/MILESTONE_0_FINDINGS.md) for validation results, limitations, and the decision gate.

## Run the exported spike

On Windows x64, launch `artifacts/win-x64/DragonHatchling.Desktop.exe`. Keep the entire export folder together; it includes the .NET runtime and does not require a separate runtime installation. Build outputs are ignored by Git.

- Drag the opaque diamond to move the window.
- Toggle **Always on top** to keep it visible over other ordinary applications.
- **Reset position** or **Ctrl+Home** centers it on the primary display's work area.
- Use the taskbar or **Alt+Tab** to find it; **Exit** or **Alt+F4** closes it.
- Each launch starts centered with always-on-top off. Preferences are deliberately not saved in this milestone.

## Build

Requires Windows x64 and the .NET 10 SDK selected by `global.json` (10.0.401 with patch roll-forward).

```powershell
./scripts/Build.ps1
```

For this workspace, a checksum-verified SDK was extracted locally rather than installed globally:

```powershell
./scripts/Build.ps1 -Dotnet ./.tools/dotnet/dotnet.exe
```

The script publishes Release, self-contained, Windows x64 output to `artifacts/win-x64`. The application uses only WPF and the .NET libraries.

Optional feasibility event logging (no saved preferences):

```powershell
./artifacts/win-x64/DragonHatchling.Desktop.exe --diagnostics ./artifacts/window-events.log
```

The log records activation, deactivation, dragging, position reset, DPI, topmost changes, and closure. Its parent directory must exist; an unwritable log does not interrupt the app.
