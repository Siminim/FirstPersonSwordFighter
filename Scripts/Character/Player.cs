using Godot;
using System;

public partial class Player : Character
{
    #region Nodes

    private Node3D cameraTarget;
    public Node3D CameraTarget => cameraTarget;

    #endregion

    #region Looking Variables

    public FloatReturnDelegateModifiers MouseSensitivityModifiers;
    public FloatReturnDelegateModifiers ControllerSensitivityModifiers;

    private const float MouseSensitivity = 0.00125f;
    private const float ControllerSensitivity = 3.0f;
    public readonly float reach = 3.0f;

    private float GetDefaultMouseSensitivity() => MouseSensitivity;
    private float GetDefaultControllerSensitivity() => ControllerSensitivity;

    #endregion

    #region Camera Bob Variables

    private Vector3 baseCameraTargetPosition;

    private readonly float headBobSpeed = 32.0f;
    private readonly float headBobDistance = 0.035f;

    private float headBobTimer = 0.0f;

    #endregion

    #region Camera Roll Variables

    private readonly float headRollAmount = 0.05f;

    private float headRollTarget = 0.0f;

    #endregion

    // ----------------------------------------------------------------------------------
    // -------------------------- Default Godot Functions -------------------------------
    // ---------------------------------------------------------------------------------- 

    public override void _EnterTree()
    {
        base._EnterTree();

        MouseSensitivityModifiers.Additive += GetDefaultMouseSensitivity;
        ControllerSensitivityModifiers.Additive += GetDefaultControllerSensitivity;
    }

    public override void _ExitTree()
    {
        MouseSensitivityModifiers.Additive -= GetDefaultMouseSensitivity;
        ControllerSensitivityModifiers.Additive -= GetDefaultControllerSensitivity;

        base._ExitTree();
    }

    public override void _Ready()
    {
        base._Ready();
        cameraTarget = GetNode<Node3D>("CameraTarget");
        baseCameraTargetPosition = cameraTarget.Position;

        PackedScene swordScene = ResourceLoader.Load<PackedScene>("res://Object-Collections/Equipment/Held/StandardSword/StandardSword.tscn");
        StandardSword standardSword = swordScene.Instantiate<StandardSword>();
        AddChild(standardSword);

        PackedScene shieldScene = ResourceLoader.Load<PackedScene>("res://Object-Collections/Equipment/Held/Shield/StandardShield.tscn");
        StandardShield standardShield = shieldScene.Instantiate<StandardShield>();
        AddChild(standardShield);

        EquipInHand(HeldItemSlot.LeftHand, standardShield);
        EquipInHand(HeldItemSlot.RightHand, standardSword);
    }

    public override void _PhysicsProcess(double delta)
    {
        CameraBob(delta);
        RotateBodyToCamera(delta);

        MovementAction(delta);
        JumpAction();
        PollHeldItemActions();

        base._PhysicsProcess(delta);
    }

    public override void _Input(InputEvent @event)
    {
        LookMouseAction(@event);
    }

    // ----------------------------------------------------------------------------------
    // ---------------------------------- Actions ---------------------------------------
    // ---------------------------------------------------------------------------------- 

    private void MovementAction(double delta)
    {
        Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");
        Move(inputDir, delta);
    }

    private void LookMouseAction(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion mouseMotion || Input.MouseMode != Input.MouseModeEnum.Captured)
            return;

        PlayerCamera playerCamera = GameManager.GetPlayerCamera;

        playerCamera.RotateY(-mouseMotion.Relative.X * MouseSensitivityModifiers.GetSafeFinalModifier());
        playerCamera.Camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivityModifiers.GetSafeFinalModifier());

        float x = Mathf.Clamp(playerCamera.Camera.Rotation.X, -Mathf.DegToRad(89.0f), Mathf.DegToRad(89.0f));
        playerCamera.Camera.Rotation = new Vector3(x, playerCamera.Camera.Rotation.Y, playerCamera.Camera.Rotation.Z);
    }

    private void LookControllerAction(double delta)
    {
        Vector2 inputDir = Input.GetVector("LookLeft", "LookRight", "LookUp", "LookDown");

        if (inputDir == Vector2.Zero)
            return;

        PlayerCamera playerCamera = GameManager.GetPlayerCamera;

        playerCamera.RotateY(-inputDir.X * ControllerSensitivityModifiers.GetSafeFinalModifier() * (float)delta);
        playerCamera.Camera.RotateX(-inputDir.Y * ControllerSensitivityModifiers.GetSafeFinalModifier() * (float)delta);

        float x = Mathf.Clamp(playerCamera.Camera.Rotation.X, -Mathf.DegToRad(89.0f), Mathf.DegToRad(89.0f));
        playerCamera.Camera.Rotation = new Vector3(x, playerCamera.Camera.Rotation.Y, playerCamera.Camera.Rotation.Z);
    }

    private void JumpAction()
    {
        if (Input.IsActionPressed("Jump"))
            QueueJump();

        if (Input.IsActionJustReleased("Jump"))
            EndJumpEarly();
    }

    private void PollHeldItemActions()
    {
        if (Input.IsActionJustPressed("ActivateLeftHand"))
            ActivateItemInHand(HeldItemSlot.LeftHand);
        else if (Input.IsActionJustReleased("ActivateLeftHand"))
            DeactivateItemInHand(HeldItemSlot.LeftHand);

        if (Input.IsActionJustPressed("ActivateRightHand"))
            ActivateItemInHand(HeldItemSlot.RightHand);
        else if (Input.IsActionJustReleased("ActivateRightHand"))
            DeactivateItemInHand(HeldItemSlot.RightHand);
    }

    // ----------------------------------------------------------------------------------
    // ------------------------------- Other Functions ----------------------------------
    // ---------------------------------------------------------------------------------- 

    private void RotateBodyToCamera(double delta)
    {
        Vector3 targetRotation = new Vector3(0, GameManager.GetPlayerCamera.GlobalRotation.Y, 0);
        RotateBody(targetRotation, delta);
    }

    private void CameraBob(double delta)
    {
        if (IsOnFloor() && Velocity.X == 0 && Velocity.Z == 0)
        {
            headBobTimer = 0.0f;
        }
        else
        {
            //float velocityPercent = Velocity.Length() / TopSpeedModifiers.GetSafeFinalModifier();
            float velocityPercent = Velocity.Length() * 0.05f;
            headBobTimer += headBobSpeed * velocityPercent * (float)delta;
        }

        float moveCyclePercent = Mathf.Sin(headBobTimer);
        CameraTarget.Position = baseCameraTargetPosition + Vector3.Up * moveCyclePercent * headBobDistance;
    }

    private void CameraRoll(double delta)
    {
        float sideVelocity = Velocity.Dot(GameManager.GetPlayerCamera.Basis * Vector3.Right) / TopSpeedModifiers.GetSafeFinalModifier(); // probably should just be static number

        if (!IsOnFloor() || (sideVelocity < 0.15f && sideVelocity > -0.15f))
            headRollTarget = 0.0f;
        else
            headRollTarget = headRollAmount * -sideVelocity;


        Vector3 cameraV = GameManager.GetPlayerCamera.Rotation;
        GameManager.GetPlayerCamera.Rotation = GameManager.GetPlayerCamera.Rotation.Lerp(new Vector3(cameraV.X, cameraV.Y, headRollTarget), (float)delta);
    }


    // private void HeldItems(double delta)
    // {
    //     if (heldItems.Item1 != null)
    //     {
    //         if (Input.IsActionJustPressed("ActivateLeftHand"))
    //             heldItems.Item1.OnPressActivate();

    //         bool leftHandActive = Input.IsActionPressed("ActivateLeftHand");
    //         heldItems.Item1.OnUpdate(leftHandActive, delta);

    //         if (Input.IsActionJustReleased("ActivateLeftHand"))
    //             heldItems.Item1.OnReleaseActivate();
    //     }

    //     if (heldItems.Item2 != null)
    //     {
    //         if (Input.IsActionJustPressed("ActivateRightHand"))
    //             heldItems.Item2.OnPressActivate();

    //         bool rightHandActive = Input.IsActionPressed("ActivateRightHand");
    //         heldItems.Item2.OnUpdate(rightHandActive, delta);

    //         if (Input.IsActionJustReleased("ActivateRightHand"))
    //             heldItems.Item2.OnReleaseActivate();
    //     }
    // }
}
