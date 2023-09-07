using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public partial class TouchPointer : Model, IPointer {
	[Resolved]
	PhysicsSystem physics { get; set; } = null!;

	public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
	public readonly BindableFloat RadiusBindable = new( 0.023f );

	public TouchPointer () {
		Y = 1;
		RadiusBindable.BindValueChanged( v => Scale = new( v.NewValue ), true );
		Alpha = 0.3f;
	}

	[BackgroundDependencyLoader]
	private void load ( MeshStore meshes ) {
		meshes.GetAsync( "sphere" ).ContinueWith( r => Schedule( mesh => Mesh = mesh, r.Result ) );
	}

	public PointerHit? UpdatePointer ( Vector3 playerPosition, Vector3 position, Quaternion rotation ) {
		var targetPosition = position;
		Rotation = rotation;

		if ( Position != targetPosition ) {
			if ( (targetPosition - Position).Dot(playerPosition - Position) > 0 ) { // going towards the player, this will help if the player got the pointer stuck somewhere
				Position = targetPosition;
				goto end;
			}

			for ( int i = 0; i < 10; i++ ) { // this makes "sliding" on colliders better
				var from = Position;
				var direction = ( targetPosition - Position ).Normalized();
				if ( physics.TryHitRay( Position, direction, out var rayHit ) && rayHit.Distance - Radius / 2 < ( Position - targetPosition ).Length ) {
					Position = rayHit.Point + rayHit.Normal * Radius / 2;
				}
				else {
					Position = targetPosition;
					break;
				}

				if ( ( from - position ).LengthSquared < 0.01f )
					break;
			}
		}

		end:
		return physics.TryHitSphere( Position, Radius, out var hit ) ? hit : null;
	}

	public bool IsTouchSource => true;

	public void SetTint ( Colour4 tint ) {
		Colour = tint.Opacity( 0.3f );
	}

	public void AddToScene ( Scene scene ) {
		scene.Add( this );
	}
	public void RemoveFromScene ( Scene scene ) {
		scene.Remove( this, disposeImmediately: false );
	}
}
