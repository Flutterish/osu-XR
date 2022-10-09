using osu.Framework.XR.Graphics;
using osu.Framework.XR.Parsing.Wavefront;
using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality;

public class TouchPointer : Model, IPointer {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;

	public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
	public readonly BindableFloat RadiusBindable = new( 0.023f );

	public TouchPointer () {
		// TODO meshes via resource store
		File.ReadAllTextAsync( "./Resources/Models/sphere.obj" ).ContinueWith( r => {
			var mesh = SimpleObjFile.Load( r.Result );
			mesh.CreateFullUnsafeUpload().Enqueue();
			Mesh = mesh;
		} );

		RadiusBindable.BindValueChanged( v => Scale = new( v.NewValue ), true );
		Alpha = 0.3f;
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
}
