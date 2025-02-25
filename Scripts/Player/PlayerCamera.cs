using Godot;
using System;

public partial class PlayerCamera : Node3D
{
    private Node3D cameraTarget;
    
    private Camera3D camera;
    public Camera3D Camera => camera;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
    }

    public override void _Process(double delta)
    {
        InterpolateCamera(delta);
    }

    public void SetCameraTarget(Node3D target)
    {
        cameraTarget = target;
    }

    private void InterpolateCamera(double delta)
    {
        GlobalPosition = GlobalPosition.Lerp(cameraTarget.GlobalPosition, (float)delta * 25.0f);
    }

}
