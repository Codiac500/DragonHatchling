# Milestone 0 — Desktop feasibility

Validation date: October 5, 2026. Scope: the Milestone 0 row and its desktop decision gate in the root [ARCHITECTURE.md](../ARCHITECTURE.md). There is no `docs/ARCHITECTURE.md` in this repository.

**Decision: the Milestone 0 exit criteria are satisfied on the tested desktop. The Windows-first WPF roadmap remains viable, with higher-DPI and monitor recovery work still unverified. Stop here for review.**

## Implemented

- One .NET 10 WPF application with a fixed 240 × 280 device-independent-unit window, `AllowsTransparency=True`, no window border, and a transparent background.
- One bundled 160 × 160 RGBA PNG: a numbered diamond, deliberately unrelated to hatching/gameplay.
- WPF `DragMove()` from the placeholder region; a compact, opaque action strip with always-on-top, Reset position, and Exit.
- Normal taskbar presence and activation. Topmost defaults to false. Reset position and Ctrl+Home center on the primary work area; every new launch starts centered.
- Explicit PerMonitorV2 DPI manifest. WPF work-area coordinates are used directly for primary-display centering. No native window interop, input passthrough flags, polling loop, animation, or timers were added.
- Optional event diagnostics and a repeatable self-contained Release publish script.

No lifecycle/controller, Hatch/Feed/Play actions, saved state, startup registration, tray behavior, installer, or gameplay art has been added. A lifecycle/save unit-test project would test work outside this milestone and was not created.

## Test environment and export

- Windows 11, OS build 26200, x64; observed DPI scale 1.00 × 1.00 (100%).
- The centered window origin was (1160, 556), corresponding to a primary work area of 2560 × 1392 at this scale. GPU inventory also reported 2560 × 1440 and 2560 × 1600 outputs and virtual/DisplayLink devices; their active arrangement and scaling were not established by this run.
- System SDKs were .NET 8/9. A repository-local Microsoft .NET SDK 10.0.401 ZIP was downloaded and checked against Microsoft's release-metadata SHA-512 before extraction. Local SDK and package caches are ignored.
- Release export bundles Microsoft.NETCore.App and Microsoft.WindowsDesktop.App **10.0.12**. Publish completed successfully with no build warnings/errors.
- Launched the exported EXE, rather than `dotnet run`. The process's loaded `coreclr.dll` came from the export folder. The system had no .NET 10 runtime installed. This establishes use of the bundled runtime on this machine; it is not a clean-machine/VM delivery test.
- Export: 400 files, approximately 139.58 MiB. Keep all files together. No trimming or single-file experiment was needed for this milestone.

Microsoft sources used to select the SDK: [download page](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and [release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json).

## Executed desktop checks

Checks used the Windows computer-use plugin against the actual exported application, with screenshots/accessibility observations and application event logs. Notepad served as the underlying ordinary desktop application; its content was not edited.

| Check | Result and evidence |
| --- | --- |
| Transparent rendering | Passed. Notepad and the Codex desktop surface remained visible through the window and PNG margins. The diamond stayed opaque and controls remained readable. No solid full-window rectangle appeared. |
| PNG/control interaction | Passed. Clicking the checkbox changed topmost state. Reset and Exit responded after dragging; no stuck mouse capture was observed. |
| Transparent-space input | A click at window-relative (30, 30), inside the transparent placeholder border, reached Notepad and changed its caret from line 5 to line 17. The spike remained inactive/topmost. The automation hit-test guard also identified a transparent drag endpoint (200, 160) as belonging to Notepad. This characterizes sampled points only, not a per-pixel guarantee for all future artwork. |
| Dragging and release | Passed in both directions. First drag moved (1160, 556) to (1190, 576); second moved to (1135, 536). Each logged `drag-start`, a location change, and `drag-end`. Controls worked afterward. |
| Focus and ordinary stacking | Passed. Launch activates the window. Switching to Notepad deactivates it. With topmost off, Notepad covers it; with topmost on, it stays visible without retaining focus. Alt+Tab returned to the spike. |
| Always-on-top toggle | Passed in both directions, confirmed in event logs and desktop screenshots. No repeated activation was observed while idle in the background. Secure desktops, fullscreen applications, and other topmost windows were not tested. |
| Position recovery | Passed. Reset button and Ctrl+Home each returned a moved window to (1160, 556). After Exit/relaunch it again started centered with topmost off. Alt+Tab made the borderless window recoverable when covered. |
| DPI/layout at 100% | Passed. The observed physical capture was 240 × 280; logged DPI was 1.00 × 1.00. PNG and all three controls fit without clipping. |
| Exit/relaunch | Passed. Exit removed the window and process, with a `closed` event. Relaunch created one centered window and restored the default unchecked topmost control. |
| Short idle sample | 30.11 seconds in the background: 0.0000 seconds measured process CPU increase, 0.000% machine CPU on 16 logical processors, 126.04 MiB working set. No activation events occurred during the sample. This is a short static-spike observation, not an animation budget or a 15-minute resilience test. |

Raw event and idle evidence from the validation run is retained in the ignored `artifacts/validation/` directory. The summary above is the durable review record. The UI automation helper rejected drag endpoints outside the window and over transparent pixels before sending input; successful drag tests used two opaque points. Consequently, a long drag off the sprite or screen edge has not been established by this automation run.

## Findings that affect later architecture

1. **Transparency is usable without custom interop on this machine.** Sampled alpha-zero margins let the underlying app receive input even though the WPF border has a Transparent brush. Dragging should be instructed from visible artwork; do not rely on visually empty margins as drag handles. Keep the compact footprint and continue to treat exact per-pixel click-through as outside the v0.1 promise. The translucent action-strip rectangle intercepts input intentionally.
2. **Normal activation is sufficient for this spike.** Launching and interacting can focus the app; inactive topmost display works. Do not add no-activate or whole-window passthrough styles without a reproduced requirement. Later idle animation must preserve the observed absence of unsolicited activation.
3. **DPI remains an open validation task.** The manifest expresses the intended mode but does not prove behavior at other scales. 150%, 200%, mixed-DPI monitor moves, negative monitor coordinates, and monitor removal were not exercised in this session. The current reset is deliberately primary-display-only. Before milestone 3 placement restoration, validate coordinate conversions against each monitor's physical bounds/work area; do not generalize this primary-display calculation to mixed-DPI saved positions.
4. **Recovery works for a covered or moved window, with limits.** Taskbar presence, Alt+Tab, Ctrl+Home, and centered relaunch provide recovery without saves. No preference/position is retained. Automatic lost-monitor recovery and invalid saved-position clamping belong to milestone 3. Small work areas below the fixed window size and taskbar-driven minimize/restore were not exercised.
5. **Resource/deployment assumptions remain plausible.** The static window stayed below the provisional 150 MiB working-set threshold during the sample and had no measurable idle CPU increase. Later animation needs its own measurement. The 139.58 MiB self-contained folder confirms the architecture's simplicity-over-size tradeoff. Separate-machine/VM testing remains the milestone 4 delivery gate.

No blocker justifies a Godot comparison or platform change from this spike. Confidence applies to ordinary Windows desktop interaction at 100% scaling; it does not close the deferred DPI, monitor, animation, sleep/resume, persistence, or delivery risks.

## Reproduce

Build and run instructions are in [README.md](../README.md). On the exported EXE:

1. Place another application behind it and verify that the PNG margins are visually transparent.
2. Drag the opaque diamond, release, and verify that the controls still respond.
3. Toggle always-on-top on, focus another app, and verify visible-but-inactive behavior. Toggle off and verify normal occlusion.
4. Click a transparent margin while pinned and verify the underlying application receives input.
5. Recover with Alt+Tab, then Reset position or Ctrl+Home.
6. Exit and relaunch; verify a centered window and topmost off.

At the next desktop validation opportunity, repeat at 150%/200%, across mixed-DPI monitors, and after monitor removal. Record physical bounds and DPI before/after each move. Those checks are outstanding rather than simulated successes.
