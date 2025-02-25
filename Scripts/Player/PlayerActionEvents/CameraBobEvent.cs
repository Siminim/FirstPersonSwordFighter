using Godot;
using System;

public class CameraBobEvent : PlayerActionEvent
{
    private Vector3 baseCameraTargetPosition = Vector3.Zero;

    private readonly float headBobSpeed = 20.0f;
    private readonly float headBobDistance = 0.06f;
    private float headBobTimer = 0.0f;

    private readonly float headRollAmount = 0.05f;
    private float headRollTarget = 0.0f;

    MoveAction moveAction = null;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        baseCameraTargetPosition = player.CameraTarget.Position;
        moveAction = player.actionEventManager[PlayerActionEventType.Move] as MoveAction;
    }

    public override void OnEffectProcess(double delta)
    {
        HeadBob(delta);
        HeadRoll(delta);
    }

    private void HeadBob(double delta)
    {
        if (player.IsOnFloor() && player.Velocity.X == 0 && player.Velocity.Z == 0)
        {
            headBobTimer = 0.0f;
        }
        else
        {
            float velocityPercent = player.Velocity.Length() / moveAction.GetTotalTopSpeed();
            headBobTimer += headBobSpeed * (float)delta * velocityPercent;
        }

        float moveCyclePercent = Mathf.Sin(headBobTimer);
        player.CameraTarget.Position = baseCameraTargetPosition + Vector3.Up * moveCyclePercent * headBobDistance;
    }

    private void HeadRoll(double delta)
    {
        float sideVelocity = player.Velocity.Dot(GameManager.GetPlayerCamera.Basis * Vector3.Right) / moveAction.GetTotalTopSpeed();

        if (!player.IsOnFloor() || (sideVelocity < 0.15f && sideVelocity > -0.15f))
            headRollTarget = 0.0f;
        else
            headRollTarget = headRollAmount * -sideVelocity;


        Vector3 cameraV = GameManager.GetPlayerCamera.Rotation;
        GameManager.GetPlayerCamera.Rotation = GameManager.GetPlayerCamera.Rotation.Lerp(new Vector3(cameraV.X, cameraV.Y, headRollTarget), (float)delta);
    }
}