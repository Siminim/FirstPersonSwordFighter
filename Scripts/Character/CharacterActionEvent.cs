public class CharacterActionEvent
{
    protected Character character;

    public virtual void OnEffectApplied(Character character)
    {
        this.character = character;
    }

    public virtual void OnEffectProcess(double delta) { }

    public virtual void OnEffectPhysicsProcess(double delta) { }

    public virtual void OnEffectRemoved() { }
}