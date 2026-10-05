namespace DragonHatchling.Desktop;

public enum PetStage { Egg, Baby }
public enum PetActivity { Idle, Hatching, Eating, Playing }

public readonly record struct PetState(PetStage Stage, PetActivity Activity);
