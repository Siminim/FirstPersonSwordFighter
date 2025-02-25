public class PlayerActionEvent
{
    protected Player player;

    public virtual void OnEffectApplied(Player player)
    {
        this.player = player;
    }

    public virtual void OnEffectProcess(double delta) { }

    public virtual void OnEffectPhysicsProcess(double delta) { }

    public virtual void OnEffectRemoved() { }
}