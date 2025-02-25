using Godot;
using System.Collections.Generic;

public class PlayerActionEventManager
{
    public Dictionary<PlayerActionEventType, PlayerActionEvent> actionDictionary = new Dictionary<PlayerActionEventType, PlayerActionEvent>()
    {
        { PlayerActionEventType.Pause, new PauseAction() },

        { PlayerActionEventType.Look, new LookAction() },
        { PlayerActionEventType.Move, new MoveAction() },
        { PlayerActionEventType.Jump, new JumpAction() },

        { PlayerActionEventType.Friction, new FrictionEvent() },
        { PlayerActionEventType.Gravity, new GravityEvent() },
        { PlayerActionEventType.CameraBob, new CameraBobEvent() }
    };

    public List<PlayerActionEvent> actions = new List<PlayerActionEvent>();
    public PlayerActionEvent this[PlayerActionEventType actionType] => actionDictionary[actionType];


    public void AddAction(Player player, PlayerActionEventType action)
    {
        PlayerActionEvent playerAction = actionDictionary[action];

        if (actions.Contains(playerAction))
        {
            GD.Print("Action already exists");
            return;
        }

        actions.Add(playerAction);
        playerAction.OnEffectApplied(player);
    }

    public void RemoveAction(PlayerActionEventType action)
    {
        PlayerActionEvent playerAction = actionDictionary[action];

        if (!actions.Contains(playerAction))
        {
            GD.Print("Action does not exist");
            return;
        }

        playerAction.OnEffectRemoved();
        actions.Remove(playerAction);
    }

    public void ProcessEffects(double delta)
    {
        foreach (PlayerActionEvent action in actions)
        {
            action.OnEffectProcess(delta);
        }
    }

    public void PhysicsProcessEffects(double delta)
    {
        foreach (PlayerActionEvent action in actions)
        {
            action.OnEffectPhysicsProcess(delta);
        }
    }

}

public enum PlayerActionEventType
{
    Pause,

    Look,
    Move,
    Jump,

    Friction,
    Gravity,
    CameraBob
}