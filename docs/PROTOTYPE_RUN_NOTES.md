# DragonHatchling prototype — Windows 11 x64

Extract the ZIP and keep the entire `win-x64` folder together. Double-click
`DragonHatchling.Desktop.exe`. No .NET installation, administrator access,
network connection, installer, or account is needed to run the export.
Windows may warn about this unsigned private prototype; public distribution
and signing are deferred.

First launch shows an egg. Click **Hatch**, then **Feed** for chewing or
**Play** for happy hops. Each reaction lasts about two seconds. Repeated clicks
during a reaction are ignored. Drag the visible artwork to move the pet.
**Always on top** keeps it above ordinary windows. Use the taskbar or Alt+Tab
to find it, **Reset position** or Ctrl+Home to center it, and **Exit** or
Alt+F4 to close it. Closing during a reaction is supported.

Stage, physical window position, and topmost preference are saved under
`%LOCALAPPDATA%/DragonHatchling/save.json`. The previous valid save is
`save.json.bak`. Run only one copy per save path. There are no care meters,
growth beyond baby, startup registration, or automatic updates.

If recovery feedback appears, the app preserves suspect save files and pauses
saving. Close it and copy the primary and backup somewhere safe. Move suspect
files out of the save directory; if the backup is valid, copy it to `save.json`
before reopening. Keep newer-version saves for a compatible release.
“Progress could not be saved” means storage failed; fix permissions or storage
availability and retry with Reset or a topmost toggle. An unsaved hatch can be
lost on restart.

## Optional broader validation

Milestone 4 is complete under the user's revised prototype acceptance criteria.
Separate-machine/VM testing was waived, and PC sleep explains the resource
sampling gap. The following checks are optional follow-ups, not completion
requirements.

On the current Windows 11 x64 desktop, or optionally a separate clean machine/VM:

1. Record Windows version, display scaling, and absence of .NET. Extract the
   complete ZIP in a writable folder and launch the EXE. Confirm the egg and
   readable controls appear without installing dependencies. Recording absence
   of .NET applies only to an optional clean-machine check.
2. Hatch, Feed, Play, drag, toggle topmost, switch to another app, Reset, Exit,
   and relaunch. Verify Baby/Idle, saved placement, and topmost preference.
3. Leave it visible during 15 minutes of ordinary desktop use. Repeat Feed,
   Play, dragging and focus changes at the start, middle, and end. Rapid clicks
   should produce one reaction and no queued actions. Exit during a reaction
   and reopen once. Check for stuck input and unexpected focus stealing.
4. Sample the pet process in Task Manager, or use the repository's
   `Measure-Resources.ps1`. Record CPU, working set, private memory, handles,
   and threads over time. Investigate sustained idle machine CPU above 1%
   or working set above 150 MiB. Look for continued growth after warm-up;
   one high sample alone is not proof of a leak.
5. Report pass/fail, resource range, and any visible failures. Where hardware
   permits, check 200%, mixed-DPI moves, monitor removal, and sleep/resume.

The current development-machine checks do not establish clean-machine launch.
Higher/mixed DPI, physical monitor removal, sleep/resume, and taskbar
minimize/restore remain unverified unless explicitly recorded in results.
