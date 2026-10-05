using DragonHatchling.Desktop;

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
