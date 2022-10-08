using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality;

public class RayPointer : BasicModel, IPointer {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;
	HitIndicator indicator;

	public RayPointer ( Scene scene ) {
		Mesh.AddQuad( Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 1, 0.005f );
		Mesh.CreateFullUnsafeUpload().Enqueue();
		scene.Add( indicator = new() );
	}

	Vector3 GlobalPosition => Matrix.Apply( Vector3.Zero );
	Vector3 GlobalForward => (Matrix.Apply( Vector3.UnitZ ) - GlobalPosition).Normalized();

	public void SetTarget ( Vector3 position, Quaternion rotation ) {
		Position = position;
		Rotation = rotation;
	}

	public bool IsTouchSource => false;

	protected override void Update () {
		if ( physics.TryHitRay( GlobalPosition, GlobalForward, out var hit ) ) {
			ScaleZ = (float)hit.Distance;
			indicator.Position = hit.Point;
			indicator.Rotation = hit.Normal.LookRotation();
			indicator.Scale = new( ( Position - hit.Point ).Length / 40 );
			ColliderHovered?.Invoke( hit );
		}
		else {
			ScaleZ = 100;
			indicator.Scale = Vector3.Zero;
			ColliderHovered?.Invoke( null );
		}

		base.Update();
	}

	public event Action<PointerHit?>? ColliderHovered;

	public class HitIndicator : BasicModel {
		public HitIndicator () {
			Mesh.AddCircle( Vector3.UnitZ * 0.1f, Vector3.UnitZ, Vector3.UnitX * 0.5f, 32 );
			Mesh.AddCircularArc( Vector3.UnitZ, Vector3.UnitX, MathF.Tau, 0.9f, 1f, origin: Vector3.UnitZ * 0.1f );
			Mesh.CreateFullUnsafeUpload().Enqueue();
		}
	}
}