using Godot;
using System;

public partial class Character : CharacterBody3D
{
    public Delegates.VoidDelegate OnLand;
    public Delegates.VoidDelegate OnAirborne;
    public Delegates.DoubleParameterDelegate WhileOnGround;
    public Delegates.DoubleParameterDelegate WhileInAir;

    #region Nodes

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

    public float frictionAmount = 0.8f;

    #endregion

    #region Movement Variables

    public FloatReturnDelegateModifiers TopSpeedModifiers;

    private float groundAcceleration = 5.0f;
    private float airAcceleration = 1.0f;
    private float topSpeed = 8.0f;

    public Vector3 localMovementVector = Vector3.Zero;

    private float GetDefaultTopSpeed() => topSpeed;

    #endregion

    // ------------------------------------------

    // Variables
    public CharacterActionEventManager actionEventManager = new CharacterActionEventManager();
    public (HeldItem, HeldItem) heldItems = (null, null);

    // Player Settings
    protected float massKg = 80.0f;
    public float MassKg => massKg;

    // Player Setting States
    protected bool previouslyOnGround = false;

    // ------------------------------------------

    public override void _EnterTree()
    {
        GravityModifiers.Additive += GetGravitySpeed;
        TopSpeedModifiers.Additive += GetDefaultTopSpeed;
        
        WhileInAir += ApplyGravity;
        WhileOnGround += ApplyFriction;
    }

    public override void _ExitTree()
    {
        GravityModifiers.Additive -= GetGravitySpeed;
        TopSpeedModifiers.Additive -= GetDefaultTopSpeed;
        
        WhileInAir -= ApplyGravity;
        WhileOnGround -= ApplyFriction;
    }

    public override void _Ready()
    {
        leftHand = GetNode<Node3D>("LeftHand");
        rightHand = GetNode<Node3D>("RightHand");

        leftHandDefaultPosition = leftHand.Position;
        rightHandDefaultPosition = rightHand.Position;

        //SetupActionEvents();
    }

    public override void _Process(double delta)
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        PushObjects();
        GroundAndAirEvents(delta);
        MoveAndSlide();
    }

    // ------------------------------------------------------------
    // -------------------- Always Active -------------------------
    // ------------------------------------------------------------

    private void GroundAndAirEvents(double delta)
    {
        if (IsOnFloor() && !previouslyOnGround)
        {
            OnLand?.Invoke();
            previouslyOnGround = true;
        }
        else if (!IsOnFloor() && previouslyOnGround)
        {
            OnAirborne?.Invoke();
            previouslyOnGround = false;
        }

        if (IsOnFloor())
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

        // if (Mathf.Sign(localMovementVector.X) != Mathf.Sign(character.Velocity.X))
        //     xBooster = topSpeedBoost;
        // if (Mathf.Sign(localMovementVector.Z) != Mathf.Sign(character.Velocity.Z))
        //     zBooster = topSpeedBoost;

        Vector3 velocityDif = targetVelocity - new Vector3(Velocity.X * xBooster, 0, Velocity.Z * zBooster);

        if (IsOnFloor())
            velocityDif *= groundAcceleration;
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

        Rotation += rotationDiff * (float)delta * 10.0f;
    }


    // protected virtual void SetupActionEvents()
    // {
    //     actionEventManager.AddAction(this, CharacterActionEventType.Move);
    //     actionEventManager.AddAction(this, CharacterActionEventType.Jump);
    //     actionEventManager.AddAction(this, CharacterActionEventType.RotateBody);

    //     actionEventManager.AddAction(this, CharacterActionEventType.PushObjects);
    //     actionEventManager.AddAction(this, CharacterActionEventType.Friction);
    //     actionEventManager.AddAction(this, CharacterActionEventType.Gravity);
    // }
}
