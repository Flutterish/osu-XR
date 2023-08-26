using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public partial class RayPointer : CompositeDrawable3D, IPointer {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;

	BasicModel ray;
	BasicModel indicator;

	public RayPointer () {
		AddInternal( ray = new() );
		AddInternal( indicator = new() );

		ray.Mesh.AddQuad( Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 1, 0.005f );
		ray.Mesh.CreateFullUnsafeUpload().Enqueue();

		indicator.Mesh.AddCircle( Vector3.UnitZ * 0.1f, Vector3.UnitZ, Vector3.UnitX * 0.5f, 32 );
		indicator.Mesh.AddCircularArc( Vector3.UnitZ, Vector3.UnitX, MathF.Tau, 0.9f, 1f, origin: Vector3.UnitZ * 0.1f );
		indicator.Mesh.CreateFullUnsafeUpload().Enqueue();
	}

	public PointerHit? UpdatePointer ( Vector3 playerPosition, Vector3 position, Quaternion rotation ) {
		Position = position;
		Rotation = rotation;

		if ( physics.TryHitRay( GlobalPosition, GlobalForward, out var hit ) ) {
			ray.ScaleZ = (float)hit.Distance;
			indicator.GlobalPosition = hit.Point;
			indicator.GlobalRotation = hit.Normal.LookRotation();
			indicator.Scale = new( ( Position - hit.Point ).Length / 40 );
			return hit;
		}
		else {
			ray.ScaleZ = 100;
			indicator.Scale = Vector3.Zero;
			return null;
		}
	}

	public bool IsTouchSource => false;

	public void SetTint ( Colour4 tint ) {
		ray.Colour = indicator.Colour = tint;
	}
}