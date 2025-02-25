using Godot;
using System;

public class LookAction : PlayerActionEvent
{
    private const float MouseSensitivity = 0.00125f;
    private const float ControllerSensitivity = 3.0f;
    public readonly float reach = 3.0f;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        player.OnMouseLook += LookMouse;
        player.OnControllerLook += LookController;
    }

    public override void OnEffectRemoved()
    {
        player.OnMouseLook -= LookMouse;
        player.OnControllerLook -= LookController;
    }

    private void LookMouse(InputEventMouseMotion mouseMotion)
    {
        PlayerCamera playerCamera = GameManager.GetPlayerCamera;

        playerCamera.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
        playerCamera.Camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

        float x = Mathf.Clamp(playerCamera.Camera.Rotation.X, -Mathf.DegToRad(89.0f), Mathf.DegToRad(89.0f));
        playerCamera.Camera.Rotation = new Vector3(x, playerCamera.Camera.Rotation.Y, playerCamera.Camera.Rotation.Z);
    }

    private void LookController(Vector2 direction, double delta)
    {
        PlayerCamera playerCamera = GameManager.GetPlayerCamera;

        playerCamera.RotateY(-direction.X * ControllerSensitivity * (float)delta);
        playerCamera.Camera.RotateX(-direction.Y * ControllerSensitivity * (float)delta);

        float x = Mathf.Clamp(playerCamera.Camera.Rotation.X, -Mathf.DegToRad(89.0f), Mathf.DegToRad(89.0f));
        playerCamera.Camera.Rotation = new Vector3(x, playerCamera.Camera.Rotation.Y, playerCamera.Camera.Rotation.Z);
    }
}
