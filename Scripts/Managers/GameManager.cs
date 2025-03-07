using Godot;
using System;

public partial class GameManager : Node
{
    static private Node3D level;

    static private Player player;
    static public Player GetPlayer => player;

    static private PlayerCamera playerCamera;
    static public PlayerCamera GetPlayerCamera => playerCamera;

    static private PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://Object-Collections/Character/Player/Player.tscn");  
    static private PackedScene PlayerCameraScene = ResourceLoader.Load<PackedScene>("res://Object-Collections/Character/Player/PlayerCamera.tscn");

    public override void _Ready()
    {
        GD.Print("Game Manager Ready");
        level = GetTree().Root.GetNode<Node3D>("Level");

        Input.SetMouseMode(Input.MouseModeEnum.Captured);

        player = PlayerScene.Instantiate<Player>();
        player.Position = new Vector3(0.0f, 5.0f, 0.0f);
        level.AddChild(player);

        playerCamera = PlayerCameraScene.Instantiate<PlayerCamera>();
        playerCamera.SetCameraTarget(player.CameraTarget);
        level.AddChild(playerCamera);
    }
}
