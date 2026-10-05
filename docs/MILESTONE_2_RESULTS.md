# Milestone 2 — Interaction slice

Validation date: October 5, 2026. Baseline: the approved Milestone 0 and Milestone 1 repository state. Scope: Milestone 2 in [ARCHITECTURE.md](../ARCHITECTURE.md). Changes are uncommitted for review. Milestone 3 has not begun.

**Exit criteria satisfied: Feed and Play have visibly different placeholder reactions, complete back to Baby/Idle, and remain repeatable under rapid input. Milestone 3 appears ready to proceed after review.**

## Implemented

- Extended the plain C# controller with Eating and Playing activities. Feed and Play require Baby/Idle. All interaction commands are ignored while any reaction is running; no commands are queued. Completion validates the expected activity, preserving the stage and rejecting unrelated or duplicate callbacks.
- Added Feed and Play beside Hatch within the existing 240 × 280 DIP window. All three interaction buttons are disabled while busy. Dragging, topmost, Reset position, Ctrl+Home, taskbar presence, and Exit retain their existing behavior.
- Feed displays food held in front of the dragon and an open mouth, with small chewing squashes. Play displays happy eyes and sparkles with larger upward hops and alternating tilt. Both are finite, approximately two-second sequences using cached PNGs and cancellable UI-thread delays.
- Added gentle idle breathing using a WPF animation capped at 12 frames per second. Reaction start clears idle animation and transforms; completion restores the idle frame and motion. Idle motion is removed when the window becomes minimized and restarted on restore. Closing removes motion and cancels pending delays; shared completion cleanup resets temporary activity.
- Extended the deterministic art script with two transparent 160 × 160 reaction frames using the existing bottom anchor. No art downloads or new dependencies.
- Expanded the existing executable controller tests and updated build/run instructions.

## Executed validation

Release controller tests passed. They cover initial Egg/Idle, rejected egg interactions, commit-before-reveal, all commands rejected during hatching, one-way lifecycle, and 100 alternating Feed/Play pairs. During those 200 reactions, 200,000 mixed-command batches (Hatch, Feed, Play) were rejected while busy. Every reaction preserved Baby, rejected unrelated/Idle/duplicate completion, and returned to Idle without a queued action.

Release Windows x64 self-contained publish succeeded with no warnings/errors using the existing local SDK 10.0.401. As in Milestone 1, the SDK needed permission to read the user's NuGet configuration outside the sandbox. No package dependencies were added.

The exported EXE was controlled through the Windows computer-use skill. Diagnostics from the main validation copy are retained in ignored `artifacts/validation/milestone-2-events.log`; the idle sample is in `milestone-2-idle.json` in the same directory.

| Check | Result |
| --- | --- |
| Initial controls and layout | Egg visible, Hatch enabled, Feed/Play disabled. Artwork and all controls fit at observed 100% scaling and 240 × 280 DIP. Transparent margins showed the underlying desktop. |
| Hatch regression | Triple-click produced one hatch, showed the cracked frame, and reached Baby/Idle. Feed and Play became enabled; Hatch remained disabled. |
| Feed distinction | Captured food, open mouth, and squashed dragon while Eating. All interaction buttons disabled; window controls remained enabled. |
| Play distinction | Captured happy eyes, sparkles, and tilted/hopping dragon while Playing. Visibly different from Feed and Idle. |
| Rapid input and completion | Triple-clicked each reaction twice. Logs contain exactly two Eating and two Playing acceptances, each followed by completion. Durations were approximately 2.004–2.005 seconds. No repeated or queued reaction followed the rapid clicks; controls re-enabled at Baby/Idle. |
| Drag/release | Opaque baby drag moved origin from (1160, 556) to (1180, 571) DIP, with drag-start/end. Subsequent controls responded. |
| Topmost and recovery | Toggle set topmost true; Reset returned origin to (1160, 556). |
| Idle resource sample | 30.018 seconds with Baby/Idle motion: 2.109 CPU seconds, approximately 0.439% machine CPU across 16 logical processors, 126.656 MiB working set. No spontaneous activation event occurred during the sample. This is a short sample, not the Milestone 4 endurance gate. |
| Exit and second launch | Exit closed the main validation window/process. A second launched copy showed Egg/Idle and default controls, then exited. No pet process remained. |

Two copies were launched during tool setup; the diagnostic log and resource sample above belong only to the main copy. The second copy was closed separately. No clean-machine claim is made.

## Deviations and findings

No architectural deviation or cross-cutting change was required. The implementation extends the existing controller/animator/window structure. Persistence, saved placement, bad-save handling, and lost-monitor recovery remain Milestone 3 work. Each launch still begins with an egg.

The baseline `ResizeMode=NoResize` window exposes a disabled Minimize action in its system menu. The new minimized-state animation cleanup is implemented, but minimize/restore through the taskbar was not validated. This does not block the interaction slice; retain it as a desktop validation finding rather than changing window policy in this milestone.

Exit was attempted following the second Play, but logs show that the reaction completed before closure because of automation latency. Actual mid-reaction close/cancellation remains unverified, as does sleep/resume. Controller completion cleanup is tested; those tests do not establish WPF cancellation or operating-system behavior.

Existing 200%, mixed-DPI, negative-coordinate, monitor-removal, and separate-machine checks remain open. This milestone adds 100% interaction evidence to Milestone 1's 150% hatching evidence; it does not establish interaction layout at every scale. Exact per-pixel click-through remains outside the contract.

The short idle sample stayed below the provisional resource thresholds, but background/inactive motion, minimize/restore, and a 15-minute endurance session remain worth checking. No new architectural blocker was found. Milestone 3 still appears ready: connect versioned local persistence to committed lifecycle/window preferences and complete its resilience checks without saving temporary activity.

## Reproduce

1. Build and launch the exported EXE using [Build and run](BUILD_AND_RUN.md).
2. Confirm Feed/Play are unavailable on the egg; rapidly click Hatch and wait for Baby/Idle.
3. Rapidly click Feed. Observe food/chewing, disabled interactions, then Baby/Idle.
4. Rapidly click Play. Observe happy eyes/sparkles/hops, then Baby/Idle. Alternate both actions several times; no queued reaction should follow.
5. Drag visible artwork and check topmost, Reset position, and Exit. Window controls should remain available during a reaction.
6. Relaunch: Egg/Idle is expected until Milestone 3. At the next validation opportunity, test close during each reaction, sleep/resume, taskbar minimize/restore, and higher/mixed DPI.
