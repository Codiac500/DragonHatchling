using DragonHatchling.Desktop;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var pet = new PetController();
Check(pet.State == new PetState(PetStage.Egg, PetActivity.Idle), "Starts Egg/Idle");
Check(!pet.CompleteHatch(), "Premature completion does not hatch");
Check(pet.TryHatch(), "First hatch accepted");
Check(pet.State == new PetState(PetStage.Baby, PetActivity.Hatching), "Stage commits before animation");
for (var i = 0; i < 1000; i++) Check(!pet.TryHatch(), "Busy clicks ignored");
Check(pet.CompleteHatch(), "Completion returns to idle");
Check(pet.State == new PetState(PetStage.Baby, PetActivity.Idle), "Baby/Idle after completion");
Check(!pet.CompleteHatch(), "Duplicate callbacks ignored");
for (var i = 0; i < 1000; i++) Check(!pet.TryHatch(), "Baby cannot hatch again");
Check(pet.State == new PetState(PetStage.Baby, PetActivity.Idle), "Rejected commands leave state intact");
Console.WriteLine("PASS: initial state, premature completion, commit-before-reveal, 1000 busy clicks, completion, duplicate callback, 1000 baby clicks.");
