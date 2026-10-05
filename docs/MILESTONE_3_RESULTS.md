# Milestone 3 — Persistence and resilience

Validation date: October 5, 2026. Baseline: the current repository state, including approved Milestones 0–2. Scope: Milestone 3 in [ARCHITECTURE.md](../ARCHITECTURE.md). The user approved committing the implementation and documentation after review. Milestone 4 has not begun.

**Exit criteria satisfied on the tested Windows desktop and automated checks: restart preserves stage and window preferences; bad-save and unavailable-monitor positions recover; Exit works during all reactions. Milestone 4 appears ready after review, with the hardware checks below still outstanding.**

## Implemented

- Plain C# `SaveStore` reads and validates one schema-version-1 JSON save at `%LOCALAPPDATA%/DragonHatchling/save.json`. Required fields are `schemaVersion`, `stage`, `x`, `y`, and `alwaysOnTop`. Only Egg/Baby string values and finite bounded coordinates are accepted; temporary activity is never serialized.
- Saves flush a unique temporary file in the same directory, then use atomic replacement with the previous valid primary retained as `save.json.bak`. Interrupted temporary files are ignored. Save failures return readable feedback rather than throwing into gameplay. The primary and backup are revalidated before replacement.
- A bad or newer-version primary loads a valid backup when available, otherwise defaults to Egg/Idle. Suspect primary and backup files remain at their original paths. Saving pauses while such files exist, with an action-strip message; [Build and run](BUILD_AND_RUN.md#save-recovery) explains manual recovery. Ordinary storage errors can be retried by toggling topmost or resetting position.
- The controller initializes from a validated stage at Idle. An accepted Hatch commits Baby, saves synchronously, then starts the reveal. Closing during any reaction therefore reopens Baby/Idle when the stage save succeeded. Feed/Play do not persist temporary activity.
- Drag release, Reset/Ctrl+Home, and topmost toggles save current stage and preferences. Launch restores the saved origin and topmost setting without saving over recovery files.
- Placement rules are centralized. Saved origins and native monitor work areas use physical virtual-desktop pixels, including negative coordinates. Partial clipping is clamped; absent/invalid monitor origins center on the primary work area. Placement rechecks physical window size after moving to account for DPI changes. A small window-specific helper enumerates monitor work areas and reads/moves the window without activation; display-setting changes trigger recovery on the dispatcher. The fixed 240 × 280 DIP footprint, WPF dragging, taskbar presence, and existing window policy remain intact.
- The existing single test project now offers a plain `net10.0` target and a `net10.0-windows` integration target. The latter invokes actual WPF button handlers with temporary saves. Explicit assembly asset URIs let that harness load the same placeholder PNGs as the application. Optional `--save-path` supports isolated exported-app validation.

## Executed validation

Release plain tests passed for existing lifecycle and rapid interaction rules, save round trips, previous-valid backup retention, Baby/Idle restart during hatch, interrupted temporary writes, malformed/incomplete/null JSON, undefined/numeric stage values, newer schema versions, invalid coordinates, invalid backup preservation, externally changed invalid saves, and unavailable storage. Placement tests cover negative monitor coordinates, clipped windows, lost-monitor origins, nonfinite placement, and undersized work areas.

Release Windows integration tests passed using real `MainWindow`, `PetAnimator`, dispatcher delays, and the Exit button's routed event. For each of Hatch, Feed, and Play, the test confirmed the reaction was still active, invoked Exit approximately 100 ms after reaction start, confirmed the window closed, and reopened Baby/Idle. Hatch additionally verified the file already contained Baby before closure. These are actual WPF event/animation checks, rather than OS mouse-injection checks.

The Windows x64 self-contained Release export published successfully without warnings/errors using local SDK 10.0.401. Build access to Windows SDK/NuGet metadata outside the workspace required approval, as in previous milestones. No new packages were added. This export is used to validate Milestone 3, not to begin Milestone 4 delivery work.

The computer-use skill controlled the exported EXE at observed 100% scaling. All validation saves used isolated paths under ignored `artifacts/validation/milestone-3/`; the normal LocalAppData save was not changed. Diagnostic logs and fixtures remain there as local evidence.

| Check | Result and evidence |
| --- | --- |
| First launch and hatch save | Egg/Idle and normal controls. `saved` precedes `Hatching-accepted`; JSON contains Baby and no activity. Hatch completes to Idle. |
| Placement and preference save | Dragged from (1160, 556) to (1190, 576) physical pixels at 100%. JSON retained the latter origin and topmost true. Controls responded after drag release. |
| Exit/restart | Exit closed the export. Relaunch showed Baby/Idle, disabled Hatch, enabled Feed/Play, checked topmost, and loaded origin (1190, 576). |
| Bad save with valid backup | Deliberately malformed primary `{broken-save` loaded a Baby backup and displayed “Save recovered from backup.” Original primary and backup remained byte-for-byte unchanged after Reset attempted a save. Saving reported the protected-file condition. |
| Unavailable saved monitor | The recovery backup had origin (90000, 90000), outside the active desktop. The exported window centered at (1160, 556), visible and usable. This proves recovery from an unavailable saved origin, not physical monitor-disconnection behavior. |
| Failed storage | A regular file blocked creation of the save directory. The final export hatched and returned to Baby; the action strip displayed “Progress could not be saved,” with Feed/Play enabled. Exit remained usable. |
| Reaction Exit | Windows integration tests exercised Exit during all three active reactions and verified Baby/Idle on reopening. |

## Deviations and findings

No cross-cutting redesign or change to the roadmap was needed. The small native placement helper addresses the existing WPF primary-only placement limitation identified in Milestone 0; controller, animator, window, and save-store responsibilities stay as planned. Physical coordinate storage makes the coordinate convention explicit rather than extending primary-display WPF work-area arithmetic to mixed-DPI monitors. No later gameplay or delivery milestone was implemented.

Recovery intentionally preserves suspect files in place and pauses writes, rather than automatically renaming or converting them. Gameplay continues, but progress in that session is not retained until the files are manually recovered and the app relaunched. A failed hatch write likewise can lose the hatch on restart; the UI reports that failure. This is the main operational limitation of the conservative recovery policy.

The last valid backup can be an earlier Egg save; it cannot guarantee recovery of the most recent hatch after primary-file corruption. Actual power loss during replacement was not induced; interrupted-write tests establish that abandoned temporary data does not replace the primary. Multiple simultaneous app copies using the same path are not coordinated; run one copy per save path.

100% placement/restoration and earlier 150% layout evidence do not establish mixed-DPI behavior. Physical monitor removal, cross-monitor DPI transitions, 200% scaling, sleep/resume, and minimize/restore remain unverified on hardware. Synthetic geometry tests do not close those checks. The baseline NoResize/minimize finding from Milestone 2 remains unchanged. An undersized work area preserves an accessible origin but cannot fit the fixed window entirely.

## Milestone 4 readiness

Milestone 4 remains ready to proceed after review. Its clean-machine launch, run notes, measured resource use, and 15-minute desktop session have not been performed here. Include the outstanding monitor/DPI and sleep/resume checks in that validation when the hardware is available. No architectural blocker was discovered.

## Reproduce

1. Run the plain tests with `dotnet run --project tests/DragonHatchling.Tests -c Release -f net10.0`.
2. On Windows with the matching desktop runtime, run the same command with `-f net10.0-windows` to include reaction-Exit checks.
3. Publish using the existing build script; launch the EXE with an isolated `--save-path` and optional `--diagnostics`.
4. Hatch, drag, toggle topmost, Exit, and relaunch against the same path. Verify Baby/Idle, saved placement, and preference.
5. With the app closed, retain copies of the fixture files; introduce malformed JSON and a valid off-screen backup. Launch, verify visible recovery and feedback, then confirm suspect files remain unchanged.
6. Point an isolated save path beneath a regular file to make storage unavailable. Hatch, verify readable failure feedback and usable controls, then Exit.

Normal save and recovery instructions are in [Build and run](BUILD_AND_RUN.md).
