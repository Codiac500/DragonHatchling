namespace DragonHatchling.Desktop;

// Lifecycle rules have no dependency on the window or animation system.
public sealed class PetController
{
    public PetController(PetStage stage = PetStage.Egg)
    {
        if (!Enum.IsDefined(stage)) throw new ArgumentOutOfRangeException(nameof(stage));
        State = new(stage, PetActivity.Idle);
    }

    public PetState State { get; private set; }

    public bool TryHatch()
    {
        if (State != new PetState(PetStage.Egg, PetActivity.Idle)) return false;
        // Commit lifecycle before the window saves and begins the reveal.
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
