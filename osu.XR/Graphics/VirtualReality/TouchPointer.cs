using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality;

public partial class TouchPointer : Model, IPointer {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;

	public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
	public readonly BindableFloat RadiusBindable = new( 0.023f );

	public TouchPointer () {
		RadiusBindable.BindValueChanged( v => Scale = new( v.NewValue ), true );
		Alpha = 0.3f;
	}

	[BackgroundDependencyLoader]
	private void load ( MeshStore meshes ) {
		meshes.GetAsync( "sphere" ).ContinueWith( r => Schedule( mesh => Mesh = mesh, r.Result ) );
	}

	public PointerHit? UpdatePointer ( Vector3 position, Quaternion rotation ) {
		var targetPosition = position;
		Rotation = rotation;

		if ( Position != targetPosition ) {
			var direction = ( targetPosition - Position ).Normalized();
			if ( physics.TryHitRay( Position, direction, out var rayHit ) && rayHit.Distance - Radius / 2 < ( Position - targetPosition ).Length ) {
				Position = rayHit.Point + rayHit.Normal * Radius / 2;
			}
			else {
				Position = targetPosition;
			}
		}

		return physics.TryHitSphere( Position, Radius, out var hit ) ? hit : null;
	}

	public bool IsTouchSource => true;

	public void SetTint ( Colour4 tint ) {
		Colour = tint.Opacity( Alpha );
	}
}
