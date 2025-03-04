using Godot;

public class JumpAction : PlayerActionEvent
{
    //public event Delegates.VoidDelegate Jumped;

    public FloatReturnDelegateModifiers JumpForceModifiers;
    public Delegates.VoidDelegate JumpStopperDelegate;

    private bool jumpPressed = false;
    static float jumpForce = 8.5f;

    private float timeSincePressedJump = 0.0f;
    private float jumpBuffer = 0.25f;

    private float jumpDecay = 2.0f;

    private float coyoteTimer = 0.0f;
    private float coyoteTimeLimit = 0.3f;

    private float GetDefaultJumpForce() => jumpForce;
    private bool CanJump()
    {
        if (JumpStopperDelegate == null || JumpStopperDelegate.GetInvocationList().Length == 0)
            return true;

        return false;
    }

    // ---------------------------------------------------------------------------

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        JumpForceModifiers.Additive += GetDefaultJumpForce;
        player.OnJumpPressed += JumpButton;
        player.OnLand += ResetCoyoteTime;
        player.WhileInAir += WhileInAir;
    }

    public override void OnEffectPhysicsProcess(double delta)
    {
        Jump(delta);
    }

    public override void OnEffectRemoved()
    {
        JumpForceModifiers.Additive -= GetDefaultJumpForce;
        player.OnJumpPressed -= JumpButton;
        player.OnLand -= ResetCoyoteTime;
        player.WhileInAir -= WhileInAir;
    }

    private void JumpButton()
    {
        if (Input.IsActionJustPressed("Jump"))
            timeSincePressedJump = 0.0f;

        if (Input.IsActionPressed("Jump") && CanJump())
        {
            jumpPressed = true;
        }
    }

    private void Jump(double delta)
    {
        if (jumpPressed && (player.IsOnFloor() || coyoteTimer < coyoteTimeLimit) && player.Velocity.Y <= 0.0f)
        {
            player.Velocity += new Vector3(0, JumpForceModifiers.GetSafeFinalModifier(), 0);

            jumpPressed = false;
            coyoteTimer = coyoteTimeLimit;
        }

        if (jumpPressed)
        {
            timeSincePressedJump += (float)delta;

            if (timeSincePressedJump >= jumpBuffer)
            {
                jumpPressed = false;
            }
        }
    }

    private void ResetCoyoteTime()
    {
        coyoteTimer = 0.0f;
    }

    private void WhileInAir(double delta)
    {
        coyoteTimer += (float)delta;

        if (player.Velocity.Y > 0.0f && Input.IsActionJustReleased("Jump"))
            player.Velocity += player.GetGravity().Normalized() * jumpDecay;
    }
}
