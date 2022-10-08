using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality;

public class TouchPointer : BasicModel, IPointer {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;

	public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
	public readonly BindableFloat RadiusBindable = new( 0.023f );

	public TouchPointer () {
		Mesh.AddCircle( Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, 32 );
		Mesh.AddCircle( Vector3.Zero, Vector3.UnitY, Vector3.UnitX, 32 );
		Mesh.AddCircle( Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 32 );
		Mesh.CreateFullUnsafeUpload().Enqueue();

		RadiusBindable.BindValueChanged( v => Scale = new( v.NewValue ), true );
		Alpha = 0.5f;
	}

	Vector3 targetPosition;
	public void SetTarget ( Vector3 position, Quaternion rotation ) {
		targetPosition = position;
		Rotation = rotation;
	}
	public bool IsTouchSource => true;

	protected override void Update () {
		if ( Position != targetPosition ) {
			var direction = ( targetPosition - Position ).Normalized();
			if ( physics.TryHitRay( Position, direction, out var rayHit ) && rayHit.Distance - Radius / 2 < ( Position - targetPosition ).Length ) {
				Position = rayHit.Point + rayHit.Normal * Radius / 2;
			}
			else {
				Position = targetPosition;
			}
		}

		if ( physics.TryHitSphere( Position, Radius, out var hit ) ) {
			ColliderHovered?.Invoke( hit );
		}
		else {
			ColliderHovered?.Invoke( null );
		}
	}

	public event Action<PointerHit?>? ColliderHovered;
}
