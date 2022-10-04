using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;
using static osu.XR.Graphics.VirtualReality.VrController;

namespace osu.XR.Graphics.VirtualReality;

public class RayPointer : BasicModel {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;

	public RayPointer () {
		Mesh.AddQuad( Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 1, 0.005f );
		Mesh.CreateFullUnsafeUpload().Enqueue();
	}

	Vector3 GlobalPosition => Matrix.Apply( Vector3.Zero );
	Vector3 GlobalForward => (Matrix.Apply( Vector3.UnitZ ) - GlobalPosition).Normalized();

	protected override void Update () {
		if ( physics.TryHitRay( GlobalPosition, GlobalForward, out var hit ) ) {
			ScaleZ = (float)hit.Distance;
			ColliderHovered?.Invoke( hit );
		}
		else {
			ScaleZ = 100;
			NothingHovered?.Invoke();
		}

		base.Update();
	}

	public event Action? NothingHovered;
	public event ColliderHoveredHandler? ColliderHovered;
}