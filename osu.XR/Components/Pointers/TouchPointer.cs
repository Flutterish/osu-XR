using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.XR.Physics;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components.Pointers {
	public class TouchPointer : Pointer {
		public Transform Source;
		public double Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
		public readonly BindableDouble RadiusBindable = new( 0.023 );

		public TouchPointer () {
			Mesh = Mesh.FromOBJFile( "./Resources/shpere.obj" );
		}

		protected override void UpdatePointer () { // TODO back and forward motion should trigger a tap even while blocked
			var targetPos = Source.Position; // ISSUE this can get stuck behind the screen

			var direction = ( targetPos - Position ).Normalized();
			if ( PhysicsSystem.TryHit( Position, direction, out var rayHit ) && rayHit.Distance - Radius / 2 < ( Position - targetPos ).Length ) {
				Position = rayHit.Point + rayHit.Normal * (float)Radius / 2;
			}
			else {
				Position = targetPos;
			}
			
			Scale = new Vector3( (float)Radius );
			if ( PhysicsSystem.TryHit( Position, Radius, out var hit ) ) {
				RaycastHit = new Raycast.RaycastHit(
					point: hit.Point,
					origin: hit.Origin,
					normal: ( hit.Origin - hit.Point ).Normalized(),
					direction: hit.Point - hit.Origin,
					distance: hit.Distance,
					trisIndex: hit.TrisIndex,
					collider: hit.Collider
				);
				CurrentHit = hit.Collider;
			}
			else {
				RaycastHit = default;
				CurrentHit = null;
			}
		}

		public override bool IsActive => Source is not null && IsVisible;
	}
}

/*
TODO handheld/movable screen (grip binding)
TODO cursor warping (at least figure out which vertices to warp)
*/