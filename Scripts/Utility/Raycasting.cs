using Godot;

public partial class Raycasting : Node3D
{
    public static Raycasting instance { get; private set; }

    public override void _Ready()
    {
        instance = this;
    }
    
    public static RaycastInfo MouseCast(uint collisionMask = 4294967295, Godot.Collections.Array<Rid> ignore = null)
    {
        Vector2 mousePos = instance.GetViewport().GetMousePosition();
        Camera3D camera = instance.GetViewport().GetCamera3D();

        Vector3 start = camera.ProjectRayOrigin(mousePos);
        Vector3 direction = camera.ProjectRayNormal(mousePos);

        PhysicsDirectSpaceState3D spaceState = instance.GetWorld3D().DirectSpaceState;

        Vector3 end = start + direction * 1000;

        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(start, end, collisionMask, ignore);
        query.CollideWithAreas = true;

        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);

        return new RaycastInfo(result, direction, end);
    }
    public static RaycastInfo MouseCast(uint collisionMask = 4294967295, params Rid[] ignore)
    {
        return MouseCast(collisionMask, new Godot.Collections.Array<Rid>(ignore));
    }

    public static RaycastInfo LineCast(Vector3 start, Vector3 end, uint collisionMask = 4294967295, Godot.Collections.Array<Rid> ignore = null)
    {
        PhysicsDirectSpaceState3D spaceState = instance.GetWorld3D().DirectSpaceState;

        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(start, end, collisionMask, ignore);
        query.CollideWithBodies = true;

        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);

        Vector3 direction = (end - start).Normalized();

        return new RaycastInfo(result, direction, end);
    }
    public static RaycastInfo LineCast(Vector3 start, Vector3 end, uint collisionMask = 4294967295, params Rid[] ignore)
    {
        return LineCast(start, end, collisionMask, new Godot.Collections.Array<Rid>(ignore));
    }

    public static RaycastInfo RayCast(Vector3 start, Vector3 direction, float distance, uint collisionMask = 4294967295, Godot.Collections.Array<Rid> ignore = null)
    {
        PhysicsDirectSpaceState3D spaceState = instance.GetWorld3D().DirectSpaceState;

        Vector3 end = start + direction * distance;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(start, end, collisionMask, ignore);
        query.CollideWithBodies = true;

        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);

        return new RaycastInfo(result, direction, end);
    }
    public static RaycastInfo RayCast(Vector3 start, Vector3 direction, float distance, uint collisionMask = 4294967295, params Rid[] ignore)
    {
        return RayCast(start, direction, distance, collisionMask, new Godot.Collections.Array<Rid>(ignore));
    }

}

public class RaycastInfo
{
    public static readonly Vector3 Invalid = new Vector3(-1.0f, -1.0f, -1.0f);

    private Godot.Collections.Dictionary info;
    private Vector3 direction;
    private Vector3 target;

    public RaycastInfo(Godot.Collections.Dictionary rayInfo, Vector3 direction, Vector3 targetPosition)
    {
        this.info = rayInfo;
        this.direction = direction;
        this.target = targetPosition;
    }

    public bool IsEmpty
    {
        get { return info.Count > 0 ? false : true; }
    }

    public Vector3 CollisionPoint
    {
        get { return !IsEmpty ? info["position"].AsVector3() : Invalid; }
    }

    public Vector3 CollisionNormal
    {
        get { return !IsEmpty ? info["normal"].AsVector3() : Invalid; }
    }

    public Vector3 Direction
    {
        get { return direction; }
    }

    public Vector3 TargetPosition
    {
        get { return target; }
    }

    public GodotObject Collider
    {
        get { return !IsEmpty ? info["collider"].AsGodotObject() : null; }
    }
}


