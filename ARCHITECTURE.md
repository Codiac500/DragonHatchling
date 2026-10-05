# DragonHatchling architecture discovery

Discovery date: October 5, 2026. Status: proposed; no application implemented or runtime behavior verified.

Implementation update, October 5, 2026: **Milestone 0 is complete on the tested Windows desktop at 100% scaling**. The Release self-contained WPF export passed transparency, dragging, ordinary input/focus, topmost toggling, recovery, and Exit checks. See [Milestone 0 findings](docs/MILESTONE_0_FINDINGS.md) for evidence and unverified DPI/monitor cases. The discovery assessment below is retained as the source of truth for later milestones; no gameplay milestones have begun.

Milestone 1 update, October 5, 2026: the hatching slice meets its exit criteria in lifecycle tests and the exported application at observed 150% scaling. See [Milestone 1 results](docs/MILESTONE_1_RESULTS.md). Milestone 1 is complete; Milestone 2 has not begun. The roadmap and architecture below remain unchanged.

## Recommendation and assumptions

Build a Windows-first, offline desktop widget using **C#, WPF, and .NET 10 LTS**, with transparent PNG artwork and simple frame/transform animations. Use one application project and one small test project. A game engine is unnecessary for this scope. “Smallest” means the least implementation and operational complexity, not the smallest possible executable.

The repository currently contains only a README. No existing application, dependency manifest, or repository-specific agent instructions were found. A `dotnet` executable is available, but SDK version and build readiness have not been verified.

Working assumptions, which are choices rather than confirmed requirements:

- Windows 11 x64 is the first and only v0.1 target, inferred from the current workspace. Confirm before implementation; macOS/Linux support would change the recommendation.
- One developer, one pet, one compact window, local use and a few testers.
- The dragon is a 2D sprite, remains where placed, and has only a handful of short reactions.
- No existing language expertise is assumed. Strong prior experience with Godot or web development could change development cost.
- The experiment asks whether hatching and interacting with a desktop companion feels enjoyable. It does not attempt a balanced long-term care simulation.

WPF is Windows-only and offers desktop controls, graphics, and animation. Microsoft documents transparent, borderless windows through `AllowsTransparency`, `WindowStyle=None`, and a transparent background. These capabilities make it a good candidate, but do not establish correct input behavior on our target machine. [WPF overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/), [window transparency](https://learn.microsoft.com/en-us/dotnet/api/system.windows.window.allowstransparency?view=windowsdesktop-10.0).

Use the current supported .NET 10 patch when implementation begins; .NET 10 is an LTS release. [Support policy](https://dotnet.microsoft.com/en-us/platform/support/policy).

## Alternatives and tradeoffs

These are architectural judgments for this scope, not measured benchmarks.

| Approach | Why consider it | Cost for this prototype | Decision |
| --- | --- | --- | --- |
| C# / WPF | Direct Windows UI support; one language; sufficient simple animation | Windows lock-in; manual sprite sequencing; desktop input still needs testing | Recommended under Windows-first assumption |
| Godot / GDScript | Animation editor and game-oriented workflow; candidate for multiple OS targets | More engine capability than needed; platform-specific overlay behavior remains | Preferred alternative if animation/gameplay or multiple OS targets dominate |
| Tauri / TypeScript + Rust | Web UI with native window integration | Frontend plus Rust tooling and WebView runtime; extra integration boundaries | Consider if existing web expertise is decisive |
| Electron / TypeScript | Familiar HTML/CSS animation workflow | Bundled browser runtime is a poor fit for a tiny background companion; transparency does not automatically provide click-through | Consider only if delivery speed from existing expertise outweighs runtime cost |

Godot documents overlay settings and warns of transparent-window problems on some Windows hybrid-GPU setups. That warrants an exported-build test, not an assumption of failure. [Godot application/overlay guide](https://docs.godotengine.org/en/stable/tutorials/ui/creating_applications.html).

Tauri requires Rust and, on Windows, C++ build tools and WebView2; transparency also has platform caveats. [Prerequisites](https://tauri.app/start/prerequisites/), [window configuration](https://v2.tauri.app/reference/config/). Electron explicitly distinguishes transparent appearance from mouse click-through. [Electron window styles](https://www.electronjs.org/docs/latest/tutorial/custom-window-styles).

Do not start with Unity, a custom native renderer, an ECS, or a cross-engine abstraction. Nothing in the proposed interactions calls for physics, 3D, or a scene-management framework. Revisit an engine if the pet later explores the desktop, plays substantial minigames, or requires a richer animation pipeline.

## Narrow v0.1 contract

The complete loop should be demonstrable in about one minute:

1. Launch into a compact transparent window showing one egg, with a visible Hatch action.
2. Hatch produces a short crack/reveal animation and permanently changes the pet into a baby dragon.
3. Feed plays an eating reaction. Play produces a visibly different happy reaction. Each action returns to idle and is repeatable.
4. Drag the pet to place it. A small action strip provides the current actions, an always-on-top toggle, and Exit. Keep a taskbar entry so the application remains discoverable.
5. Close and reopen: the egg/baby stage, position, and always-on-top preference are restored.

Use explicit hatching rather than a long timer. Feed and Play are expressive interactions without hunger decay, penalties, counters, or progression bars. Their value is emotional feedback; persistence only needs to remember lifecycle stage and window preferences.

The pet window starts around 240 by 280 device-independent units, adjusted to the artwork. The pet itself remains opaque while unused surrounding space is visually transparent. The compact footprint is mandatory; a full-desktop invisible input surface is unacceptable. Exact click-through around individual sprite pixels is not a v0.1 promise. Characterize empty-space input during the first spike; if necessary, accept a documented compact rectangular interaction area for private testing.

Include placeholder egg and dragon art, idle motion, hatch, eat, and play reactions. A few PNG frames plus scale/translation changes are enough; final artwork and audio are deferred.

Exclude: multiple pets, growth beyond baby, breeding, currency, inventory, minigames, AI dialogue, networking, accounts, cloud saves, offline progression, death, autonomous roaming, physics, startup registration, tray-only operation, installer, auto-update, and additional OS targets.

## Minimal internal structure

One process and the normal WPF UI event loop; no continuous simulation loop or worker service.

| Piece | Responsibility |
| --- | --- |
| `PetState` and `PetController` | Plain C# lifecycle rules and command validation, independent of WPF |
| `MainWindow` | Window placement, buttons, drag handling, focus, and display bindings; small code-behind is acceptable |
| `PetAnimator` | Select frames/transforms and signal reaction completion |
| `SaveStore` | Read, validate, and safely replace one versioned JSON file |
| `Assets` | Local transparent PNGs with consistent canvas dimensions and an origin/anchor |

Keep lifecycle and temporary activity separate: saved stage is `Egg` or `Baby`; activity is `Idle`, `Hatching`, `Eating`, or `Playing`. Hatch is accepted only for an idle egg. Feed and Play require an idle baby. Ignore further interaction commands during a reaction while allowing Exit and window movement. Do not queue clicks.

On Hatch, commit and save the Baby stage, then show the reveal. Closing mid-animation therefore reopens at Baby/Idle. Temporary activity is never saved. Keep stage changes in the controller so animation callbacks cannot duplicate a hatch or mutate lifecycle state.

Save `schemaVersion`, stage, window position, and always-on-top preference under `%LOCALAPPDATA%/DragonHatchling/save.json`. Write a temporary file in the same directory and replace the prior save; keep the last valid backup. Validate enums and numeric ranges. Preserve malformed or newer-version files and show a brief recovery message rather than silently overwriting them. A failed write must not crash the pet; report that progress could not be saved.

Clamp restored placement to an available monitor's work area; center on the primary display if placement is invalid. Save position after a drag, preference after a toggle, and stage when hatching. No database, dependency injection container, event bus, or general-purpose persistence framework.

## Risks and how to retire them

| Risk | Probe and response |
| --- | --- |
| Transparency and input are different problems | Test over another application: transparent appearance, clickable pet/actions, behavior of empty space, and drag release. Do not enable whole-window mouse passthrough, which would also disable interaction. |
| Borderless window becomes hard to recover or steals focus | Keep taskbar presence and explicit Exit. Verify launching, clicking, switching apps, and always-on-top toggling; idle animation must not repeatedly activate the window. |
| DPI and monitor coordinates produce jumps or lost windows | Test 100%, 150%, and 200% scaling, moving between monitors where available, monitor removal, and restoring an invalid saved position. Centralize coordinate conversion. |
| Background pet consumes too much CPU/GPU | Bound frame updates, cache images, and stop unnecessary animation work when minimized. Measure a release build before optimizing. Initial investigation thresholds: sustained idle CPU above 1% on the test machine or working set above 150 MB; these are provisional budgets, not guarantees. |
| Sleep/resume or repeated clicks leave reactions stuck | Use completion/cancellation cleanup that returns to Idle, including after resume. Verify rapid input and closing during each reaction. |
| Save failure loses the hatch | Test interrupted writes, malformed JSON, and unavailable storage; recover last valid data and report failures. |
| Deployment works only on developer machine | Publish a self-contained Windows x64 folder and test on a separate machine or VM without a preinstalled .NET runtime. It includes a runtime and is not size-minimal. Public signing/distribution can wait. |
| Art effort obscures the experiment | Use placeholders until the loop works. Distinct Feed and Play feedback matters more than frame count. |

## Incremental roadmap and decision gates

Effort ranges are rough focused developer time, excluding tooling installation, final art, and tester scheduling. The first spike may change all later estimates.

| Milestone | Deliverable | Exit condition | Rough effort |
| --- | --- | --- | --- |
| 0. Desktop feasibility | Disposable WPF window with one PNG, drag, toggle, and Exit | Exported app is transparent, movable, recoverable, and usable over other apps; document input and DPI findings | Half to one day |
| 1. Hatching slice | Egg, explicit Hatch, baby, and minimal lifecycle controller | Hatches exactly once; ignores repeated Hatch; returns to idle | Half a day |
| 2. Interaction slice | Idle, Feed, Play, placeholder reactions | Both reactions are visibly different, finish reliably, and survive rapid input | Half to one day |
| 3. Persistence and resilience | Local save, placement recovery, error feedback | Restart preserves stage; bad save and lost-monitor cases recover; Exit works during reactions | Half to one day |
| 4. Prototype delivery | Self-contained folder, run notes, measured resource use | Clean-machine launch plus a 15-minute desktop session without stuck input or unbounded resource growth | Half to one day |

At milestone 0, stop adding gameplay if transparency or ordinary desktop interaction is unreliable. Time-box diagnosis to the spike; compare a matching Godot experiment if necessary. If multiple OS targets are confirmed, reassess before building the WPF UI. Add native window interop only for a reproduced blocker, behind a small window-specific helper.

Automate a few meaningful tests: legal lifecycle transitions, ignored commands while busy, restart during hatch, save round-trip, and malformed-save recovery. Use manual checks for transparency, focus, dragging, mixed DPI, and resource use; unit tests cannot establish these OS behaviors. Record unavailable hardware checks as unverified.

For the product review, observe a few testers without explaining the controls. Can they hatch, feed, play, move, and close the pet? Do the two reactions feel distinct? Do they choose to leave it visible during normal desktop work? Use those observations to decide whether v0.2 deserves care meters, better animation, or a different platform. Do not assume that more systems will make the companion more appealing.

Current next action: review Milestone 1, then proceed to Milestone 2 if approved. The original discovery recommended Milestone 0 as the first decision gate; its results and subsequent milestone results are linked above.
