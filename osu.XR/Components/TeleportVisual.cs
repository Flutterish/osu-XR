using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.XR;
using osu.Framework.XR.Components;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class TeleportVisual : CompositeDrawable3D {
		private Path3D path;
		private Model circle;
		Cached isPathValid = new();

		public readonly BindableBool IsActive = new();
		public readonly Bindable<Vector3> GravityBindable = new( Vector3.UnitY * -9.81f );
		public readonly Bindable<Vector3> OriginBindable = new();
		public readonly Bindable<Vector3> DirectionBindable = new( Vector3.UnitY );

		protected override void LoadComplete () {
			base.LoadComplete();

			Root.Add( path = new() );
			Root.Add( circle = new() );
			circle.Scale = new Vector3( 0.1f );
			circle.Alpha = 0.3f;
			circle.Mesh.AddCircle( Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, 32 );
			IsActive.BindValueChanged( v => {
				if ( v.NewValue ) {
					path.IsVisible = true;
					circle.IsVisible = HasHitGround;
				}
				else {
					path.IsVisible = false;
					circle.IsVisible = false;
				}
			}, true );

			(GravityBindable, OriginBindable, DirectionBindable).BindValuesChanged( () => isPathValid.Invalidate(), true );
		}

		protected override void Update () {
			base.Update();
			if ( IsActive.Value && !isPathValid.IsValid ) {
				regenPath();
				isPathValid.Validate();
			}
		}

		public bool HasHitGround { get; private set; }
		public Vector3 HitPosition { get; private set; }
		void regenPath () {
			const float timeStep = 0.03f;
			Vector3 pos = OriginBindable.Value;
			Vector3 velocity = DirectionBindable.Value;
			path.ClearNodes();
			for ( int i = 0; i < 200; i++ ) {
				path.AddNode( pos );
				pos += velocity * timeStep;
				velocity += GravityBindable.Value * timeStep;

				if ( pos.Y <= 0 && pos.Y < OriginBindable.Value.Y ) {
					HasHitGround = true;
					circle.Position = HitPosition = pos - velocity * ( pos.Y / velocity.Y );
					circle.Y = 0.01f;
					circle.IsVisible = true;
					return;
				}
			}
			HasHitGround = false;
			circle.IsVisible = false;
		}
	}
}
