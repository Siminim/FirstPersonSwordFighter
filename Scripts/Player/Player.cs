using Godot;
using System;

public partial class Player : CharacterBody3D
{
    #region ActionEvent Delegates

    public Delegates.VoidDelegate OnPause;

    public Delegates.Vector2DoubleParameterDelegate OnMove;
    public Delegates.MouseMotionDelegate OnMouseLook;
    public Delegates.Vector2DoubleParameterDelegate OnControllerLook;

    public Delegates.VoidDelegate OnJumpPressed;
    public Delegates.VoidDelegate OnDash;
    public Delegates.VoidDelegate OnInteract;
    public Delegates.VoidDelegate OnActivateHeldItem;
    public Delegates.VoidDelegate OnActivatePotionEffect;

    public Delegates.VoidDelegate OnLand;
    public Delegates.VoidDelegate OnAirtimeStart;
    public Delegates.DoubleParameterDelegate WhileOnGround;
    public Delegates.DoubleParameterDelegate WhileInAir;
    public Delegates.Node3DParameterDelegate OnLookAtItemChange;

    #endregion

    #region Engine Nodes

    private Node3D cameraTarget;
    public Node3D CameraTarget => cameraTarget;

    private Node3D leftHand;
    public Node3D LeftHand => leftHand;
    private Node3D rightHand;
    public Node3D RightHand => rightHand;

    private Vector3 leftHandDefaultPosition;
    public Vector3 LeftHandDefaultPosition => leftHandDefaultPosition;
    private Vector3 rightHandDefaultPosition;
    public Vector3 RightHandDefaultPosition => rightHandDefaultPosition;

    #endregion

    #region Player Settings and Variables

    // Player Settings
    private float massKg = 80.0f;

    // Player Setting States
    private bool previouslyOnGround = false;

    #endregion

    public PlayerActionEventManager actionEventManager = new PlayerActionEventManager();

    public (HeldItem, HeldItem) heldItems = (null, null);

    // ----------------------------------------------------------------------------------
    // ------------------------- Setup and Ready Functions ------------------------------
    // ---------------------------------------------------------------------------------- 

    public override void _Ready()
    {
        cameraTarget = GetNode<Node3D>("CameraTarget");
        leftHand = GetNode<Node3D>("LeftHand");
        rightHand = GetNode<Node3D>("RightHand");

        leftHandDefaultPosition = leftHand.Position;
        rightHandDefaultPosition = rightHand.Position;

        SetupActionEvents();

        EquipLeftHand(heldItems.Item1);
        EquipRightHand(heldItems.Item2);
    }

    private void SetupActionEvents()
    {
        actionEventManager.AddAction(this, PlayerActionEventType.Pause);

        actionEventManager.AddAction(this, PlayerActionEventType.Look);
        actionEventManager.AddAction(this, PlayerActionEventType.Move);
        actionEventManager.AddAction(this, PlayerActionEventType.Jump);

        actionEventManager.AddAction(this, PlayerActionEventType.Friction);
        actionEventManager.AddAction(this, PlayerActionEventType.Gravity);
        actionEventManager.AddAction(this, PlayerActionEventType.CameraBob);
    }

    // ----------------------------------------------------------------------------------
    // ----------------------------- Process Overrides ----------------------------------
    // ---------------------------------------------------------------------------------- 

    public override void _Process(double delta)
    {
        actionEventManager.ProcessEffects((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        PollActions(delta);
        actionEventManager.PhysicsProcessEffects((float)delta);

        GroundAndAirEvents((float)delta);
        MoveAndSlide();

        RotateBody(delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
            LookMouseAction(mouseMotion);

    }

    private void PollActions(double delta)
    {
        PauseAction();

        LookControllerAction(delta);
        MovementAction(delta);
        JumpAction();

        HeldItems(delta);
    }

    // ----------------------------------------------------------------------------------
    // ---------------------------- Action Event Functions ------------------------------
    // ----------------------------------------------------------------------------------     

    private void PauseAction()
    {
        if (Input.IsActionJustPressed("Pause"))
            OnPause?.Invoke();
    }

    private void LookMouseAction(InputEventMouseMotion mouseMotion)
    {
        OnMouseLook?.Invoke(mouseMotion);
    }

    private void LookControllerAction(double delta)
    {
        Vector2 inputDir = Input.GetVector("LookLeft", "LookRight", "LookUp", "LookDown");
        OnControllerLook?.Invoke(inputDir, delta);
    }

    private void MovementAction(double delta)
    {
        Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
        OnMove?.Invoke(inputDir, delta);
    }

    private void JumpAction()
    {
        if (Input.IsActionPressed("Jump"))
            OnJumpPressed?.Invoke();
    }

    private void GroundAndAirEvents(double delta)
    {
        if (IsOnFloor() && !previouslyOnGround)
        {
            OnLand?.Invoke();
            previouslyOnGround = true;
        }
        else if (!IsOnFloor() && previouslyOnGround)
        {
            OnAirtimeStart?.Invoke();
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

    private void HeldItems(double delta)
    {
        if (heldItems.Item1 != null)
        {
            if (Input.IsActionJustPressed("ActivateLeftHand"))
                heldItems.Item1.OnPressActivate();

            bool leftHandActive = Input.IsActionPressed("ActivateLeftHand");
            heldItems.Item1.OnUpdate(leftHandActive, delta);

            if (Input.IsActionJustReleased("ActivateLeftHand"))
                heldItems.Item1.OnReleaseActivate();
        }

        if (heldItems.Item2 != null)
        {
            if (Input.IsActionJustPressed("ActivateRightHand"))
                heldItems.Item2.OnPressActivate();

            bool rightHandActive = Input.IsActionPressed("ActivateRightHand");
            heldItems.Item2.OnUpdate(rightHandActive, delta);

            if (Input.IsActionJustReleased("ActivateRightHand"))
                heldItems.Item2.OnReleaseActivate();
        }
    }

    //  ----------------------------------------------------------------------------------
    //  ------------------------------ Personal Functions --------------------------------
    //  ----------------------------------------------------------------------------------

    private void RotateBody(double delta)
    {
        float max = Mathf.DegToRad(180);

        Vector3 targetRotation = new Vector3(0, GameManager.GetPlayerCamera.GlobalRotation.Y, 0);
        Vector3 colliderRotation = new Vector3(0, Rotation.Y, 0);

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

    private void PushRigidBodies()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);

            if (collision.GetCollider() is RigidBody3D rigidBody)
            {
                Vector3 pushDirection = -collision.GetNormal();
                float deltaVelocity = Velocity.Dot(pushDirection) - rigidBody.LinearVelocity.Dot(pushDirection);
                deltaVelocity = Mathf.Max(0.0f, deltaVelocity);

                float massRatio = Mathf.Min(1.0f, massKg / rigidBody.Mass);
                pushDirection = new Vector3(pushDirection.X, 0, pushDirection.Z);

                float pushForce = massRatio;
                rigidBody.ApplyImpulse(pushDirection * deltaVelocity * pushForce, collision.GetPosition() - rigidBody.GlobalPosition);
            }
        }
    }

    private void EquipLeftHand(HeldItem heldItem)
    {
        PackedScene shieldScene = ResourceLoader.Load<PackedScene>("res://Object-Collections/Equipment/Held/Shield/StandardShield.tscn");
        StandardShield standardShield = shieldScene.Instantiate<StandardShield>();
        AddChild(standardShield);
        standardShield.Reparent(leftHand);
        standardShield.Position = Vector3.Zero;
        standardShield.Assign(this, leftHand);
        heldItems.Item1 = standardShield;
        heldItems.Item1.OnEquip();
    }

    private void EquipRightHand(HeldItem heldItem)
    {
        PackedScene swordScene = ResourceLoader.Load<PackedScene>("res://Object-Collections/Equipment/Held/StandardSword/StandardSword.tscn");
        StandardSword standardSword = swordScene.Instantiate<StandardSword>();
        AddChild(standardSword);
        standardSword.Reparent(rightHand);
        standardSword.Position = Vector3.Zero;
        standardSword.Assign(this, rightHand);
        heldItems.Item2 = standardSword;
        heldItems.Item2.OnEquip();
    }

}
