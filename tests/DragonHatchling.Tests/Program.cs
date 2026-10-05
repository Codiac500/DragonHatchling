using DragonHatchling.Desktop;
using System.IO;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var pet = new PetController();
Check(pet.State == new PetState(PetStage.Egg, PetActivity.Idle), "Starts Egg/Idle");
Check(!pet.CompleteHatch(), "Premature completion does not hatch");
Check(!pet.TryFeed() && !pet.TryPlay(), "Egg cannot feed or play");
Check(pet.TryHatch(), "First hatch accepted");
Check(pet.State == new PetState(PetStage.Baby, PetActivity.Hatching), "Stage commits before animation");
for (var i = 0; i < 1000; i++)
    Check(!pet.TryHatch() && !pet.TryFeed() && !pet.TryPlay(), "All commands ignored while hatching");
Check(pet.CompleteHatch(), "Completion returns to idle");
Check(pet.State == new PetState(PetStage.Baby, PetActivity.Idle), "Baby/Idle after completion");
Check(!pet.CompleteHatch(), "Duplicate callbacks ignored");
for (var i = 0; i < 1000; i++) Check(!pet.TryHatch(), "Baby cannot hatch again");
Check(pet.State == new PetState(PetStage.Baby, PetActivity.Idle), "Rejected commands leave state intact");
for (var repeat = 0; repeat < 100; repeat++)
{
    foreach (var activity in new[] { PetActivity.Eating, PetActivity.Playing })
    {
        Check(activity == PetActivity.Eating ? pet.TryFeed() : pet.TryPlay(), "Idle baby accepts interaction");
        Check(pet.State == new PetState(PetStage.Baby, activity), "Interaction preserves baby stage");
        for (var i = 0; i < 1000; i++)
            Check(!pet.TryHatch() && !pet.TryFeed() && !pet.TryPlay(), "Rapid mixed input ignored while busy");
        var wrong = activity == PetActivity.Eating ? PetActivity.Playing : PetActivity.Eating;
        Check(!pet.CompleteReaction(wrong) && !pet.CompleteHatch() && !pet.CompleteReaction(PetActivity.Idle),
            "Unrelated completion cannot end reaction");
        Check(pet.State.Activity == activity, "Rejected input leaves reaction intact");
        Check(pet.CompleteReaction(activity), "Reaction completes or cancels to idle");
        Check(!pet.CompleteReaction(activity), "Duplicate completion ignored");
        Check(pet.State == new PetState(PetStage.Baby, PetActivity.Idle), "No queued action after completion");
    }
}
Console.WriteLine("PASS: hatch lifecycle, egg restrictions, 200 repeatable Feed/Play reactions, 200000 busy mixed-command batches, wrong/duplicate completion, no queued clicks.");

var directory = Path.Combine(Path.GetTempPath(), "DragonHatchling-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
try
{
    var path = Path.Combine(directory, "save.json");
    var store = new SaveStore(path);
    Check(store.Load() == new SaveLoadResult(null, null), "Absent save is a clean first launch");
    var egg = new SaveData(1, PetStage.Egg, -100, 200, false);
    var baby = new SaveData(1, PetStage.Baby, 400, -200, true);
    Check(store.Save(egg) is null && store.Save(baby) is null, "Atomic save and replacement succeed");
    Check(new SaveStore(path).Load().Data == baby, "Round trip preserves stage, coordinates, topmost");
    Check(new SaveStore(path + ".bak").Load().Data == egg, "Backup retains previous valid save");
    Check(new PetController(new SaveStore(path).Load().Data!.Stage).State == new PetState(PetStage.Baby, PetActivity.Idle),
        "Restart during hatch restores Baby/Idle");
    File.WriteAllText(path + ".interrupted.tmp", "{truncated");
    Check(new SaveStore(path).Load().Data == baby, "Interrupted temporary write does not affect live save");
    foreach (var bad in new[] { "{broken", "null", "[]", "{}",
        "{\"schemaVersion\":2,\"stage\":\"Baby\",\"x\":1,\"y\":2,\"alwaysOnTop\":true}",
        "{\"schemaVersion\":1,\"stage\":\"Dragon\",\"x\":1,\"y\":2,\"alwaysOnTop\":true}",
        "{\"schemaVersion\":1,\"stage\":1,\"x\":1,\"y\":2,\"alwaysOnTop\":true}",
        "{\"schemaVersion\":1,\"stage\":\"Baby\",\"x\":1e99,\"y\":2,\"alwaysOnTop\":true}" })
    {
        File.WriteAllText(path, bad);
        var recovery = new SaveStore(path);
        var result = recovery.Load();
        Check(result.Data == egg && result.Message is not null, "Malformed/newer save recovers valid backup with feedback");
        Check(recovery.Save(baby) is not null && File.ReadAllText(path) == bad, "Suspect original preserved during gameplay");
        Check(new SaveStore(path + ".bak").Load().Data == egg, "Recovery does not overwrite valid backup");
    }
    var lone = Path.Combine(directory, "lone.json");
    File.WriteAllText(lone, "bad");
    Check(new SaveStore(lone).Load() is { Data: null, Message: not null }, "Bad save without backup falls back to egg with feedback");
    var changed = Path.Combine(directory, "changed.json");
    var changedStore = new SaveStore(changed);
    changedStore.Load();
    File.WriteAllText(changed, "external newer file");
    Check(changedStore.Save(baby) is not null && File.ReadAllText(changed) == "external newer file", "External invalid changes are preserved");
    var blocked = Path.Combine(directory, "blocked");
    File.WriteAllText(blocked, "file blocks directory creation");
    Check(new SaveStore(Path.Combine(blocked, "save.json")).Save(baby) is not null, "Unavailable storage reports failure without throwing");
    Check(store.Save(baby with { X = double.NaN }) is not null, "Nonfinite placement rejected");
    var badBackupPath = Path.Combine(directory, "bad-backup.json");
    var badBackupStore = new SaveStore(badBackupPath);
    Check(badBackupStore.Save(baby) is null, "First save succeeds");
    File.WriteAllText(badBackupPath + ".bak", "newer or broken backup");
    var badBackupLoad = new SaveStore(badBackupPath);
    Check(badBackupLoad.Load() is { Data.Stage: PetStage.Baby, Message: not null }, "Valid primary with invalid backup reports protection on launch");
    Check(badBackupLoad.Save(egg) is not null && File.ReadAllText(badBackupPath + ".bak") == "newer or broken backup",
        "Invalid backup is preserved rather than replaced");

    WorkArea[] areas = [new(0, 0, 1920, 1040), new(-2560, -400, 0, 1040)];
    Check(Placement.Recover(-2400, -300, 360, 420, areas, 0) == (-2400, -300), "Negative monitor coordinates preserved");
    Check(Placement.Recover(1900, 1000, 240, 280, areas, 0) == (1680, 760), "Clipped window clamped to work area");
    Check(Placement.Recover(9000, 9000, 240, 280, areas, 0) == (840, 380), "Lost monitor centers on primary");
    Check(Placement.Recover(double.NaN, 0, 240, 280, areas, 0) == (840, 380), "Invalid placement centers on primary");
    Check(Placement.Recover(null, null, 240, 280, [new(0, 0, 200, 200)], 0) == (0, 0), "Small work area keeps origin accessible");
    Console.WriteLine("PASS: save round trip, backup, hatch restart, interrupted writes, malformed/newer preservation, write failure, negative/clipped/lost-monitor placement.");
}
finally { Directory.Delete(directory, recursive: true); }

#if WINDOWS
var enduranceIndex = Array.IndexOf(args, "--endurance-minutes");
var enduranceMinutes = 0;
if (enduranceIndex >= 0 && (enduranceIndex + 1 >= args.Length ||
    !int.TryParse(args[enduranceIndex + 1], out enduranceMinutes) || enduranceMinutes is < 1 or > 60))
    throw new ArgumentException("--endurance-minutes requires an integer from 1 to 60.");
WpfChecks.Run(enduranceMinutes);
#endif
