using Godot;
using System.Collections.Generic;

public class CharacterActionEventManager
{
    public Dictionary<CharacterActionEventType, CharacterActionEvent> actionDictionary = new Dictionary<CharacterActionEventType, CharacterActionEvent>()
    {
        { CharacterActionEventType.Pause, new PauseAction() },

        { CharacterActionEventType.Jump, new JumpAction() },
    };

    public List<CharacterActionEvent> actions = new List<CharacterActionEvent>();
    public CharacterActionEvent this[CharacterActionEventType actionType] => actionDictionary[actionType];


    public void AddAction(Character character, CharacterActionEventType action)
    {
        CharacterActionEvent characterAction = actionDictionary[action];

        if (actions.Contains(characterAction))
        {
            GD.Print("Action already exists");
            return;
        }

        actions.Add(characterAction);
        characterAction.OnEffectApplied(character);
    }

    public void RemoveAction(CharacterActionEventType action)
    {
        CharacterActionEvent characterAction = actionDictionary[action];

        if (!actions.Contains(characterAction))
        {
            GD.Print("Action does not exist");
            return;
        }

        characterAction.OnEffectRemoved();
        actions.Remove(characterAction);
    }

    public void ProcessEffects(double delta)
    {
        foreach (CharacterActionEvent action in actions)
        {
            action.OnEffectProcess(delta);
        }
    }

    public void PhysicsProcessEffects(double delta)
    {
        foreach (CharacterActionEvent action in actions)
        {
            action.OnEffectPhysicsProcess(delta);
        }
    }

}

public enum CharacterActionEventType
{
    Pause,

    Look,
    Move,
    Jump,
    RotateBody,

    PushObjects,
    Friction,
    Gravity,
    CameraBob
}