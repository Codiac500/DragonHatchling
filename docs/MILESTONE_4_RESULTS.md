# Milestone 4 — Prototype delivery

Validation date: October 5, 2026. Baseline: the approved current repository,
including Milestones 0–3. Scope: Milestone 4 in
[ARCHITECTURE.md](../ARCHITECTURE.md). Changes remain uncommitted for review.

**Milestone 4 is complete under the user's revised prototype acceptance
criteria, confirmed October 5, 2026.** The user identified PC sleep as the
cause of the sampling gap and waived separate Windows-machine/VM testing.
The existing desktop evidence, WPF endurance checks, and exported-process
measurements are accepted for this milestone. No additional manual desktop
session or clean-machine test is claimed. Changes remain uncommitted.

## Implemented

- `Package.ps1` publishes Release, self-contained Windows x64 output into a
  fresh directory, adds tester run notes beside the EXE, and produces a ZIP
  with one complete `win-x64` folder and a SHA-256 sidecar. Fresh output avoids
  stale files from previous exports. No installer, runtime installation,
  startup registration, signing, or update system was added.
- `Build.ps1` accepts an optional output directory while retaining its normal
  `artifacts/win-x64` default. Existing placeholder visuals are retained.
- Bundled [prototype run notes](PROTOTYPE_RUN_NOTES.md) describe controls,
  persistence/recovery, one-copy-per-save-path operation, clean-machine launch,
  and desktop/resource acceptance. [Build and run](BUILD_AND_RUN.md) documents
  packaging and measurement reproduction.
- `Measure-Resources.ps1` samples a chosen running process every 30 seconds,
  retaining CSV evidence for CPU, working set, private memory, handles,
  threads, and responsiveness. CPU is normalized across logical processors.
  Existing evidence is protected against accidental mixing or overwrite;
  exited, reused, or unresponsive processes fail the measurement.
- The Windows integration target accepts `--endurance-minutes 15`. It opens
  production WPF windows and repeatedly invokes Hatch/Feed/Play, rapid busy
  commands, topmost and Reset handlers. It checks completion within five
  seconds, restored controls, and Exit/restart to Baby/Idle, using temporary
  isolated saves. It does not inject OS mouse input.
- Changed the taskbar/window title from “Persistence slice” to “DragonHatchling”.
  Controller, animator, saves, placement, window size and window policy are
  otherwise unchanged.

## Executed validation

Release packaging succeeded without warnings/errors using the existing local
SDK 10.0.401 and package cache. Access to Windows SDK metadata outside the
workspace required approval, as in earlier milestones. No dependencies were
added. Both existing Release test targets passed lifecycle, rapid interaction,
save/recovery, placement, and WPF Exit-during-reaction/restart checks.

The ZIP was extracted to a separate ignored validation directory. All 401
extracted files matched the fresh publish by SHA-256. Published content is
146,414,528 bytes (139.63 MiB); the ZIP is 65,198,087 bytes (62.18 MiB).
It includes .NET and Windows Desktop runtime 10.0.12 and the run notes; it
excludes SDK files, tests, development saves, and diagnostic logs.

Reviewed artifact: `artifacts/DragonHatchling-win-x64.zip`.
SHA-256: `25E50FF3C1FCCF141CEA577273693902F17FFE7AFE63C72BD0AC6FCEDC8007B8`.
Later packaging runs replace this artifact and checksum.

After runtime validation, only the bundled run notes were refreshed to reflect
the user's acceptance revision. Application/runtime binaries are unchanged.
The refreshed ZIP retains 401 entries; its bundled notes match the repository
notes, and its SHA-256 sidecar was updated. Sizes and checksum above describe
this refreshed delivery.

The extracted EXE launched with an isolated Baby save, at observed 100% scaling
and 240 × 280 DIP. Its loaded `hostfxr.dll` and `coreclr.dll` paths were inside
the extracted delivery folder. System runtime inventory contained .NET 6/8/9
and no .NET 10 runtime. This confirms use of the bundled runtime on this
development machine; it is not a separate clean-machine test.

The WPF endurance check passed after **900.2 seconds and 53 completed
reactions**. Controls restored after every reaction, rapid commands did not
leave it stuck, and Exit/restart returned to Baby/Idle. This ran on the actual
production WPF dispatcher and animator, through routed button events.

The exported Baby/Idle process was sampled for **969.096 seconds** from
14:24:10 to 14:40:19 Central time. Its 25 recorded samples all reported
responsive. Measurement used 16 logical processors:

| Metric | Observed result |
| --- | --- |
| Machine CPU, regular intervals | Mean 0.152%; maximum 0.230% |
| Whole-run machine CPU | 0.117%, including PC sleep |
| Process CPU increase | 18.094 seconds |
| Working set | 113.930–125.633 MiB; final 125.633 MiB |
| Private memory | 60.730–68.105 MiB; final 67.691 MiB |
| Handles | 665–687; final 680 |
| Threads | 12–22; final 14 |

Resources stayed below the provisional investigation thresholds. Memory
settled after initial warm-up and fluctuated within the recorded range;
handles and threads did not continually increase. No sustained unbounded
growth was observed at sampled points. Diagnostics recorded no further
export activation during the resource measurement after initial test setup at
14:25:55.

**Measurement context:** the user confirmed that the PC slept during the run,
explaining the 276.648-second interval before the final sample. This is no
longer an unresolved finding or milestone blocker. Regular-interval CPU
excludes that interval; the whole-run average includes sleep. The WPF run
completed successfully after the PC resumed. This incidental sleep is not
a systematic sleep/resume test. The sampler's interval reporting and generic
gap warnings remain useful diagnostics.

PowerShell parsing, extracted-file integrity, protection of existing CSV
evidence, and rejection of nonexistent process IDs passed. A subsequent
one-minute sampler run passed with seven samples and verified the interval
field. The extracted export then accepted a normal window-close request and
exited within five seconds; sending that request required running outside
the sandbox. Raw local evidence,
runtime paths, CSV measurements and summary are retained under ignored
`artifacts/validation/milestone-4/`.

## Deviations, limits and next planning

No cross-cutting change or architectural redesign was needed. The architecture
and v0.1 behavior remain intact. The acceptance deviation is the user's waiver
of separate-machine/VM testing and acceptance of the existing validation for
completion. Production WPF handler tests do not establish desktop mouse
capture, dragging, click-through, or focus behavior; prior milestones retain
their actual desktop evidence and limits.

Earlier hardware limits remain: 200%, mixed-DPI moves, physical monitor removal,
systematic sleep/resume testing, and taskbar minimize/restore. The NoResize window policy is
unchanged. Save recovery still preserves suspect files and pauses writes;
concurrent copies sharing a save path remain unsupported.

A finite endurance observation cannot prove resources will never grow. The
resource measurement covers the exported Baby/Idle process; the concurrent
WPF interaction harness is a different process, so its allocations are not
part of the export's reported working set. An interaction-heavy exported-app
resource session remains an optional follow-up for broader validation.

Tester scheduling and product-review planning are ready to proceed using the
bundled optional checklist. Milestone 4 is complete; no further acceptance work
is required under the revised criteria. The architecture defines no
Milestone 5: subsequent product planning should use tester observations about
discoverability, reaction distinction, and whether they keep the pet visible.
No v0.2 implementation has begun.
