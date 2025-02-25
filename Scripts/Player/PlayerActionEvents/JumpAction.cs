using Godot;

public class JumpAction : PlayerActionEvent
{
    //public event Delegates.VoidDelegate Jumped;

    public event Delegates.FloatReturnDelegate JumpForceAdditive;
    public event Delegates.FloatReturnDelegate JumpForceMultipliers;
    public event Delegates.FloatReturnDelegate JumpForceDividers;

    public event Delegates.VoidDelegate JumpStopperDelegate;

    private bool jumpPressed = false;
    static float jumpForce = 8.5f;

    private float timeSincePressedJump = 0.0f;
    private float jumpBuffer = 0.25f;

    private float jumpDecay = 2.0f;

    private float coyoteTimer = 0.0f;
    private float coyoteTimeLimit = 0.3f;

    private float GetDefaultJumpForce() => jumpForce;

    public override void OnEffectApplied(Player player)
    {
        base.OnEffectApplied(player);
        JumpForceAdditive += GetDefaultJumpForce;
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
        JumpForceAdditive -= GetDefaultJumpForce;
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
            player.Velocity += new Vector3(0, GetTotalJumpForce(), 0);

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

    private float GetTotalJumpForce()
    {
        return GetJumpForceAdditive() * GetJumpForceMultiplier() / GetJumpForceDivider();
    }

    private float GetJumpForceAdditive()
    {
        if (JumpForceAdditive == null || JumpForceAdditive.GetInvocationList().Length == 0)
            return 0.0f;

        float jf = 0.0f;

        foreach (Delegates.FloatReturnDelegate jumpForceDelegate in JumpForceAdditive.GetInvocationList())
        {
            jf += jumpForceDelegate.Invoke();
        }

        return jf;
    }

    private float GetJumpForceMultiplier()
    {
        if (JumpForceMultipliers == null || JumpForceMultipliers.GetInvocationList().Length == 0)
            return 1.0f;

        float jf = 0.0f;

        foreach (Delegates.FloatReturnDelegate jumpForceDelegate in JumpForceMultipliers.GetInvocationList())
        {
            jf += jumpForceDelegate.Invoke();
        }

        return jf;
    }

    private float GetJumpForceDivider()
    {
        if (JumpForceDividers == null || JumpForceDividers.GetInvocationList().Length == 0)
            return 1.0f;

        float jf = 0.0f;

        foreach (Delegates.FloatReturnDelegate jumpForceDelegate in JumpForceDividers.GetInvocationList())
        {
            jf += jumpForceDelegate.Invoke();
        }

        return jf;
    }

    private bool CanJump()
    {
        if (JumpStopperDelegate == null || JumpStopperDelegate.GetInvocationList().Length == 0)
            return true;

        return false;
    }
}
