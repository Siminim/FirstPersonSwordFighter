using Godot;
using System;

public partial class Character : CharacterBody3D
{
    public Delegates.VoidDelegate OnLand;
    public Delegates.VoidDelegate OnAirborne;
    public Delegates.DoubleParameterDelegate WhileOnGround;
    public Delegates.DoubleParameterDelegate WhileInAir;

    #region Nodes

    public AnimationPlayer animationPlayer;

    protected Node3D leftHand;
    public Node3D LeftHand => leftHand;
    protected Node3D rightHand;
    public Node3D RightHand => rightHand;



    protected Vector3 leftHandDefaultPosition;
    public Vector3 LeftHandDefaultPosition => leftHandDefaultPosition;
    protected Vector3 rightHandDefaultPosition;
    public Vector3 RightHandDefaultPosition => rightHandDefaultPosition;

    #endregion

    #region Gravity Variables

    public FloatReturnDelegateModifiers GravityModifiers;

    private float maxFallVelocity = -58.8f;

    private float GetGravitySpeed() => GetGravity().Length();
    private Vector3 GetGravityDirection() => GetGravity().Normalized();

    #endregion

    #region Friction Variables

    public float frictionAmount = 0.85f;

    #endregion

    #region Rotate Body Variables

    public FloatReturnDelegateModifiers RotateBodyModifiers;

    private float rotateBodySpeed = 12.0f;

    private float GetDefaultRotateBodySpeed() => rotateBodySpeed;

    #endregion

    #region Movement Variables

    public FloatReturnDelegateModifiers TopSpeedModifiers;
    public FloatReturnDelegateModifiers GroundAccelerationModifiers;

    private float groundAcceleration = 3.5f;
    private float airAcceleration = 1.0f;
    private float accelerationBoost = 3.0f;
    private float topSpeed = 5.0f;
    private float runningSpeedBoost = 5.0f;
    private float runningAccelerationBoost = 5.0f;
    private float runningRotateBodySpeedDivider = 3.0f;
    private bool isRunning = false;

    public Vector3 localMovementVector = Vector3.Zero;

    private float GetDefaultTopSpeed() => topSpeed;
    private float GetDefaultGroundAcceleration() => groundAcceleration;
    private float GetRunningSpeedBoost() => runningSpeedBoost;
    private float GetRunningAccelerationBoost() => runningAccelerationBoost;
    private float GetRunningRotateBodySpeedDivider() => runningRotateBodySpeedDivider;

    #endregion

    #region Jump Variables

    public FloatReturnDelegateModifiers JumpForceModifiers;
    public Delegates.VoidDelegate DisableJumpDelegate;

    private bool queuedJump = false;
    private float jumpForce = 8.5f;

    private float timeSinceQueuedJump = 0.0f;
    private float jumpBuffer = 0.125f;

    private float jumpDecay = 2.0f;

    private float coyoteTimer = 0.0f;
    private float coyoteTimeLimit = 0.3f;

    private float GetDefaultJumpForce() => jumpForce;
    private bool CanJump()
    {
        if (DisableJumpDelegate == null || DisableJumpDelegate.GetInvocationList().Length == 0)
            return true;

        return false;
    }

    #endregion

    #region Push Objects Variables

    protected float massKg = 80.0f;
    public float MassKg => massKg;

    #endregion

    #region Ground Condition Variables

    protected bool onGround = false;
    protected bool previouslyOnGround = false;

    #endregion

    private HeldItem leftHandItem = null;
    private HeldItem rightHandItem = null;

    // ----------------------------------------------------------------------------------
    // -------------------------- Default Godot Functions -------------------------------
    // ---------------------------------------------------------------------------------- 

    public override void _EnterTree()
    {
        GravityModifiers.Additive += GetGravitySpeed;
        TopSpeedModifiers.Additive += GetDefaultTopSpeed;
        GroundAccelerationModifiers.Additive += GetDefaultGroundAcceleration;
        JumpForceModifiers.Additive += GetDefaultJumpForce;
        RotateBodyModifiers.Additive += GetDefaultRotateBodySpeed;

        WhileInAir += ApplyGravity;
        WhileInAir += CoyoteTimeCounter;
        WhileOnGround += ApplyFriction;
        OnLand += ResetCoyoteTimeCounter;
    }

    public override void _ExitTree()
    {
        GravityModifiers.Additive -= GetGravitySpeed;
        TopSpeedModifiers.Additive -= GetDefaultTopSpeed;
        GroundAccelerationModifiers.Additive -= GetDefaultGroundAcceleration;
        JumpForceModifiers.Additive -= GetDefaultJumpForce;
        RotateBodyModifiers.Additive -= GetDefaultRotateBodySpeed;

        WhileInAir -= ApplyGravity;
        WhileInAir -= CoyoteTimeCounter;
        WhileOnGround -= ApplyFriction;
        OnLand -= ResetCoyoteTimeCounter;
    }

    public override void _Ready()
    {
        leftHand = GetNode<Node3D>("LeftHand");
        rightHand = GetNode<Node3D>("RightHand");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        leftHandDefaultPosition = leftHand.Position;
        rightHandDefaultPosition = rightHand.Position;
    }

    public override void _Process(double delta)
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        ValidateRun();
        //PushObjects();
        GroundAndAirEvents(delta);
        StepUp(delta);
        Jump(delta);
        MoveAndSlide();
    }

    // ------------------------------------------------------------
    // -------------------- Always Active -------------------------
    // ------------------------------------------------------------

    private void GroundAndAirEvents(double delta)
    {
        if (IsOnFloor() && !onGround)
        {
            OnLand?.Invoke();
            onGround = true;
        }
        else if (!IsOnFloor() && onGround)
        {
            if (!StepDown(delta))
            {
                OnAirborne?.Invoke();
                onGround = false;
            }
        }

        if (onGround)
        {
            WhileOnGround?.Invoke(delta);
        }
        else
        {
            WhileInAir?.Invoke(delta);
        }
    }

    private void PushObjects()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);

            if (collision.GetCollider() is RigidBody3D rigidBody)
            {
                Vector3 pushDirection = -collision.GetNormal();
                float deltaVelocity = Velocity.Dot(pushDirection) - rigidBody.LinearVelocity.Dot(pushDirection);
                deltaVelocity = Mathf.Max(0.0f, deltaVelocity);

                float massRatio = Mathf.Min(1.0f, MassKg / rigidBody.Mass);
                pushDirection = new Vector3(pushDirection.X, 0, pushDirection.Z);

                float pushForce = massRatio;
                rigidBody.ApplyImpulse(pushDirection * deltaVelocity * pushForce, collision.GetPosition() - rigidBody.GlobalPosition);
            }
        }
    }

    private void Jump(double delta)
    {
        if (queuedJump && (onGround || coyoteTimer < coyoteTimeLimit) && Velocity.Y <= 0.0f)
        {
            Velocity += new Vector3(0, JumpForceModifiers.GetSafeFinalModifier(), 0);
            queuedJump = false;
            coyoteTimer += coyoteTimeLimit;
        }

        timeSinceQueuedJump += (float)delta;
        if (timeSinceQueuedJump >= jumpBuffer)
            queuedJump = false;
    }

    private bool StepDown(double delta)
    {
        if (Velocity.Y > 0.0f)
            return false;

        KinematicCollision3D collision = MoveAndCollide(Vector3.Down * 0.55f, true);

        if (collision == null)
            return false;

        if (collision.GetPosition().Y - Position.Y < -0.01f)
        {
            float collisionLength = Mathf.Abs((Position - collision.GetPosition()).Length());
            Position -= new Vector3(0, collision.GetNormal().Y * collisionLength * (float)delta * 20.0f, 0);
            Velocity += Vector3.Down * 0.05f;
        }

        return true;
    }

    private void StepUp(double delta)
    {
        if (Velocity.Y < 0.0f)
            return;
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta, true);

        if (collision == null || Mathf.Abs(collision.GetPosition().Y - Position.Y) > 0.5f)
            return;

        DebugDraw3D.DrawSphere(collision.GetPosition(), 0.02f, new Color(0.0f, 0.0f, 1.0f), 0.0f);

        float collisionLength = Mathf.Abs((Position - collision.GetPosition()).Length());
        Position += new Vector3(localMovementVector.X * collisionLength * (float)delta, collision.GetNormal().Y * collisionLength * (float)delta * 50.0f, localMovementVector.Z * collisionLength * (float)delta);
    }

    // ------------------------------------------------------------
    // ------------------ Called by Delegates ---------------------
    // ------------------------------------------------------------

    private void ApplyGravity(double delta)
    {
        Velocity += GetGravityDirection() * GravityModifiers.GetSafeFinalModifier() * (float)delta;

        // if (Velocity.Y <= 0.0f)
        //     Velocity += GetGravity().Normalized() * fastFallBoost * (float)delta;

        if (Velocity.Y < maxFallVelocity)
            Velocity = new Vector3(Velocity.X, maxFallVelocity, Velocity.Z);
    }

    private void ApplyFriction(double delta)
    {
        if (localMovementVector != Vector3.Zero || (Velocity.X == 0 && Velocity.Z == 0))
            return;

        Vector3 frictionMask = new Vector3(Velocity.X, 0, Velocity.Z);
        Vector3 totalFriction = frictionMask.Normalized() * frictionAmount;
        Velocity -= totalFriction;

        if (Velocity.Length() < 0.5f)
            Velocity = Vector3.Zero;
    }

    private void CoyoteTimeCounter(double delta)
    {
        coyoteTimer += (float)delta;
    }

    private void ResetCoyoteTimeCounter()
    {
        coyoteTimer = 0.0f;
    }


    // ------------------------------------------------------------
    // ---------------- Use in Inherited Classes ------------------
    // ------------------------------------------------------------

    protected void Move(Vector2 inputDir, double delta)
    {
        if (inputDir == Vector2.Zero)
        {
            localMovementVector = Vector3.Zero;
            return;
        }

        localMovementVector = (GameManager.GetPlayerCamera.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        Vector3 targetVelocity = localMovementVector * TopSpeedModifiers.GetSafeFinalModifier();

        float xBooster = 1.0f;
        float zBooster = 1.0f;

        if (Mathf.Sign(localMovementVector.X) != Mathf.Sign(Velocity.X))
            xBooster = accelerationBoost;
        if (Mathf.Sign(localMovementVector.Z) != Mathf.Sign(Velocity.Z))
            zBooster = accelerationBoost;

        Vector3 velocityDif = targetVelocity - new Vector3(Velocity.X * xBooster, 0, Velocity.Z * zBooster);

        if (onGround)
            velocityDif *= GroundAccelerationModifiers.GetSafeFinalModifier();
        else
            velocityDif *= airAcceleration;

        Velocity += new Vector3(velocityDif.X, 0, velocityDif.Z) * (float)delta;
    }

    protected void RotateBody(Vector3 targetRotation, double delta)
    {
        Vector3 colliderRotation = new Vector3(0, Rotation.Y, 0);

        float max = Mathf.DegToRad(180);
        Vector3 rotationDiff = targetRotation - colliderRotation;

        if (Mathf.Abs(rotationDiff.Y) >= max)
        {
            if (Mathf.Sign(rotationDiff.Y) >= 0)
                rotationDiff.Y -= max * 2;
            else if (Mathf.Sign(rotationDiff.Y) < 0)
                rotationDiff.Y += max * 2;
        }

        Rotation += rotationDiff * RotateBodyModifiers.GetSafeFinalModifier() * (float)delta;
    }

    protected void QueueJump()
    {
        timeSinceQueuedJump = 0.0f;

        if (CanJump())
            queuedJump = true;
    }

    protected void EndJumpEarly()
    {
        if (Velocity.Y <= 0.0f)
            return;

        float velocityMod = Velocity.Y * 0.25f;
        Velocity += GetGravityDirection() * velocityMod * jumpDecay;
    }

    protected void EquipInHand(HeldItemSlot slot, HeldItem item)
    {
        if (slot == HeldItemSlot.LeftHand)
        {
            item.Reparent(leftHand);
            item.Assign(this, leftHand);
            leftHandItem = item;
        }
        else if (slot == HeldItemSlot.RightHand)
        {
            item.Reparent(rightHand);
            item.Assign(this, rightHand);
            rightHandItem = item;
        }
        item.OnEquip();
        item.Position = Vector3.Zero;
    }

    protected void ActivateItemInHand(HeldItemSlot slot)
    {
        if (slot == HeldItemSlot.LeftHand && leftHandItem != null && !rightHandItem.Active)
            leftHandItem.Activate();
        else if (slot == HeldItemSlot.RightHand && rightHandItem != null && !leftHandItem.Active)
            rightHandItem.Activate();
    }

    protected void DeactivateItemInHand(HeldItemSlot slot)
    {
        if (slot == HeldItemSlot.LeftHand && leftHandItem != null)
            leftHandItem.Deactivate();
        else if (slot == HeldItemSlot.RightHand && rightHandItem != null)
            rightHandItem.Deactivate();
    }

    protected virtual bool ActivateRun()
    {
        if (isRunning || !onGround)
            return false;

        Vector3 forwardDirection = Basis * Vector3.Forward;
        float directionAngle = forwardDirection.AngleTo(localMovementVector);

        if (directionAngle > Mathf.DegToRad(70))
            return false;

        isRunning = true;
        TopSpeedModifiers.Additive += GetRunningSpeedBoost;
        GroundAccelerationModifiers.Additive += GetRunningAccelerationBoost;
        RotateBodyModifiers.Divider += GetRunningRotateBodySpeedDivider;

        return true;
    }

    protected void ValidateRun()
    {
        if (!isRunning)
            return;

        Vector3 forwardDirection = Basis * Vector3.Forward;
        float directionAngle = forwardDirection.AngleTo(localMovementVector);

        if (directionAngle > Mathf.DegToRad(70))
            DeactivateRun();
    }

    protected virtual bool DeactivateRun()
    {
        if (!isRunning)
            return false;

        isRunning = false;
        TopSpeedModifiers.Additive -= GetRunningSpeedBoost;
        GroundAccelerationModifiers.Additive -= GetRunningAccelerationBoost;
        RotateBodyModifiers.Divider -= GetRunningRotateBodySpeedDivider;

        return true;
    }

    protected bool ActivateDodgeDash()
    {
        if (!onGround)
            return false;

        Vector3 forwardDirection = Basis * Vector3.Forward;
        float directionAngle = forwardDirection.AngleTo(localMovementVector);

        if (directionAngle <= Mathf.DegToRad(70))
            return false;

        Velocity = new Vector3(topSpeed * localMovementVector.X * 1.8f, 0, topSpeed * localMovementVector.Z * 1.8f);

        return true;
    }

    // ------------------------------------------------------------
    // ------------------ Use in Other Classes --------------------
    // ------------------------------------------------------------

    public void ApplyDamage(float damage)
    {
        GD.Print($"Character took {damage} damage");
    }

}
