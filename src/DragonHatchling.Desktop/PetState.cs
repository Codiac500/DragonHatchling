namespace DragonHatchling.Desktop;

public enum PetStage { Egg, Baby }
public enum PetActivity { Idle, Hatching }

public readonly record struct PetState(PetStage Stage, PetActivity Activity);
