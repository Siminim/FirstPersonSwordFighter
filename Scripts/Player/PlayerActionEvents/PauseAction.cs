using Godot;
using System;

public class PauseAction : PlayerActionEvent
{
    //PackedScene pauseMenu = ResourceLoader.Load<PackedScene>("res://UserInterface/PauseMenu/PauseMenu.tscn");
    //public PauseMenu pauseMenuInstance;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        CreatePauseMenu();
        //player.OnPause += Pause;
    }

    public override void OnEffectRemoved()
    {
        //player.OnPause -= Pause;
    }

    private void CreatePauseMenu()
    {
        // if (pauseMenuInstance != null)
        //     return;

        // pauseMenuInstance = pauseMenu.Instantiate<PauseMenu>();
        // player.GetParent().CallDeferred(Player.MethodName.AddChild, pauseMenuInstance);
        // pauseMenuInstance.Hide();
    }

    private void Pause()
    {
        // player.GetTree().Paused = true;
        // pauseMenuInstance.OnPause(player);
    }




}