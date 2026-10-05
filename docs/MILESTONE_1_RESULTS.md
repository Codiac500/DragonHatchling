# Milestone 1 — Hatching slice

Validation date: October 5, 2026. Baseline: the approved Milestone 0 repository. Scope: Milestone 1 in [ARCHITECTURE.md](../ARCHITECTURE.md). No later milestone was started. The user authorized committing the implementation and documentation after review.

**Exit criteria satisfied: the egg hatches exactly once per launch, repeated Hatch commands are ignored, and the baby returns to Idle. Milestone 2 remains ready to proceed after review.**

## Implemented

- Plain C# `PetState` and `PetController`, independent of WPF. Lifecycle (`Egg`/`Baby`) is distinct from temporary activity (`Idle`/`Hatching`). Only Egg/Idle accepts Hatch. The controller commits Baby/Hatching before animation, rejects further requests both while busy and after completion, and ignores duplicate completion callbacks.
- Explicit Hatch button and status text. During the reaction and after hatching, the button is disabled. Baby/Idle displays “Hatched.” No click queue or reset-to-egg action is provided.
- Cached transparent PNG egg, cracked egg, and baby placeholders with consistent 160 × 160 canvas and bottom anchor. A deterministic Windows script regenerates these assets without downloads or art dependencies.
- `PetAnimator` runs a finite approximately two-second sequence: egg shakes, crack frame, baby reveal/pop, static idle. UI-thread asynchronous delays avoid blocking controls; there is no continuous polling or animation timer. Window closure cancels pending delays and controller cleanup returns the temporary activity to Idle.
- Preserved 240 × 280 DIP window, taskbar entry, WPF dragging, topmost toggle, position reset, Ctrl+Home, and Exit. The action strip grows upward within the same window dimensions to fit Hatch. Visible artwork remains the drag target, incorporating Milestone 0's transparent-margin findings.
- One small dependency-free executable test project compiling the production lifecycle sources. No packages or architecture abstractions were introduced.

## Executed validation

Release lifecycle tests passed: initial Egg/Idle, rejected premature completion, accepted first hatch, Baby committed before reveal, 1,000 rejected commands while Hatching, completion to Baby/Idle, rejected duplicate completion, and 1,000 rejected hatch commands on the idle baby. Rejected commands leave the state unchanged. Failures throw and produce a nonzero process exit.

Release Windows x64 self-contained publish succeeded with no warnings/errors using the existing local SDK 10.0.401 and package cache. Tests/build needed access to the user's NuGet configuration outside the sandbox; no package dependencies were added.

The exported EXE was launched and controlled through the Windows computer-use plugin. Raw event evidence is in ignored `artifacts/validation/milestone-1-events.log`.

| Check | Result |
| --- | --- |
| Initial appearance | Egg visible with readable Hatch and recovery controls; transparent margins showed the underlying desktop. |
| Scaling/layout | Diagnostics reported 1.50 × 1.50 (150%) and 240 × 280 DIP dimensions. Egg, baby, and controls fit without clipping. System scaling settings were not changed. |
| Rapid Hatch input | A triple click produced one accepted hatch; button became disabled and Hatching appeared. No queued second reaction occurred. |
| Crack/reveal/idle | Captured the cracked egg and subsequent baby. First run logged Baby/Hatching at 13:03:17.185 and Baby/Idle at 13:03:19.166 (local time). A second run completed in approximately 1.97 seconds. |
| Drag/release | Dragging opaque baby artwork moved the window from (733.33, 369.33) to (753.33, 384.00) DIP; logged drag start/end. Subsequent controls responded. |
| Topmost/recovery | Toggle set topmost true; Reset returned the moved window to (733.33, 369.33). |
| Exit/relaunch | Exit removed the window. Relaunch showed Egg/Idle, centered with topmost false, as expected before persistence. Second Exit also removed the window; no pet process remained. |

The attempted manual Exit-during-hatch check completed after the short animation because of automation round-trip latency. Logs establish Exit after completion, not mid-reaction cancellation. That cancellation path is implemented but remains unverified in the actual desktop application. No sleep/resume check was executed.

## Scope and architecture

No cross-cutting change or architecture deviation was necessary. The architectural save-on-hatch rule will be connected in Milestone 3; this slice commits stage in memory only. Closing/reopening therefore starts with an egg. “Idle” here means the completed temporary state and static baby image; continuous idle motion, Feed, and Play remain Milestone 2 work. Persistence, placement restoration, malformed-save handling, delivery/resource budgets, and clean-machine testing remain their planned later gates.

The lightweight executable test runner uses assertions rather than a third-party test framework, keeping the test project independent of WPF and avoiding new package dependencies.

## Risks and next milestone

No new architectural blocker was found. Primary-display layout/dragging at observed 150% adds evidence beyond Milestone 0's 100% checks, but 200%, mixed-DPI monitor movement, negative monitor coordinates, and monitor removal remain unverified. Alpha-zero input behavior has not been recharacterized for every pixel of the new artwork.

Reaction cancellation on close, sleep/resume, inactive animation focus behavior, and resource use with continuous idle motion still need validation. No new resource-budget or clean-machine claim is made. When adding Milestone 2, extend the existing command guards and animation cleanup for Eating/Playing; validate their distinct appearance and completion under rapid input. No architectural redesign is indicated.
