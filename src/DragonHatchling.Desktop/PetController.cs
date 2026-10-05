namespace DragonHatchling.Desktop;

// Lifecycle rules have no dependency on the window or animation system.
public sealed class PetController
{
    public PetState State { get; private set; } = new(PetStage.Egg, PetActivity.Idle);

    public bool TryHatch()
    {
        if (State != new PetState(PetStage.Egg, PetActivity.Idle)) return false;
        // Commit lifecycle before reveal. Persistence is added in milestone 3.
        State = new(PetStage.Baby, PetActivity.Hatching);
        return true;
    }

    public bool TryFeed() => TryReact(PetActivity.Eating);
    public bool TryPlay() => TryReact(PetActivity.Playing);

    private bool TryReact(PetActivity activity)
    {
        if (State != new PetState(PetStage.Baby, PetActivity.Idle)) return false;
        State = new(PetStage.Baby, activity);
        return true;
    }

    public bool CompleteHatch() => CompleteReaction(PetActivity.Hatching);

    public bool CompleteReaction(PetActivity expected)
    {
        if (expected == PetActivity.Idle || State.Activity != expected) return false;
        State = new(PetStage.Baby, PetActivity.Idle);
        return true;
    }
}
