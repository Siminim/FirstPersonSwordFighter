using Godot;

public struct ShapeCastingInfo
{
    Node3D[] collidingBodies;
    Vector3[] collisionPoints;
    Vector3[] collisionNormals;

    public ShapeCastingInfo(Node3D[] collidingBodies, Vector3[] collisionPoints, Vector3[] collisionNormals)
    {
        this.collidingBodies = collidingBodies;
        this.collisionPoints = collisionPoints;
        this.collisionNormals = collisionNormals;
    }

    public Node3D[] CollidingBodies => collidingBodies;
    public Vector3[] CollisionPoints => collisionPoints;
    public Vector3[] CollisionNormals => collisionNormals;

    public int Count => collidingBodies.Length;

    public Node3D GetBody(int index = 0) => collidingBodies[index];
    public Vector3 GetCollisionPoint(int index = 0) => collisionPoints[index];
    public Vector3 GetCollisionNormal(int index = 0) => collisionNormals[index];
}

public partial class ShapeCasting : Node
{
    private static ShapeCast3D sphereCaster;

    public override void _Ready()
    {
        InitializeSphere();
    }

    private void InitializeSphere()
    {
        SphereShape3D sphere = new SphereShape3D();
        sphereCaster = new ShapeCast3D();

        sphere.Radius = 1.0f;
        sphereCaster.Shape = sphere;
        sphereCaster.CollisionMask = 0b0000;
        sphereCaster.ProcessMode = ProcessModeEnum.Disabled;
        AddChild(sphereCaster);
    }

    public static ShapeCastingInfo SphereCast(Vector3 position, float radius = 1.0f, uint collisionMask = 0b0001, Godot.Collections.Array<Rid> ignore = null)
    {
        sphereCaster.ClearExceptions();
    
        sphereCaster.Position = position;
        (sphereCaster.Shape as SphereShape3D).Radius = radius;
        sphereCaster.CollisionMask = collisionMask;

        if (ignore != null)
        {
            for (int i = 0; i < ignore.Count; i++)
            {
                sphereCaster.AddExceptionRid(ignore[i]);
            }
        }

        sphereCaster.ForceShapecastUpdate();

        Node3D[] bodies = new Node3D[sphereCaster.CollisionResult.Count];
        Vector3[] collisionPoints = new Vector3[sphereCaster.CollisionResult.Count];
        Vector3[] collisionNormals = new Vector3[sphereCaster.CollisionResult.Count];

        for (int i = 0; i < sphereCaster.CollisionResult.Count; i++)
        {
            bodies[i] = sphereCaster.GetCollider(i) as Node3D;
            collisionPoints[i] = sphereCaster.GetCollisionPoint(i);
            collisionNormals[i] = sphereCaster.GetCollisionNormal(i);
        }

        return new ShapeCastingInfo(bodies, collisionPoints, collisionNormals);
    }
    public static ShapeCastingInfo SphereCast(Vector3 position, float radius = 1.0f, uint collisionMask = 4294967295, params Rid[] ignore)
    {
        return SphereCast(position, radius, collisionMask, new Godot.Collections.Array<Rid>(ignore));
    }
}
