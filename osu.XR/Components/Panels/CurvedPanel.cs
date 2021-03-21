using osu.Framework.Bindables;
using osu.Framework.XR.Maths;
using osuTK;
using System;

namespace osu.XR.Components.Panels {
	/// <summary>
	/// A curved 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public class CurvedPanel : Panel {
		public float Arc { get => ArcBindable.Value; set => ArcBindable.Value = value; }
		public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
		public readonly BindableFloat ArcBindable = new( MathF.PI * 1.2f ) { MinValue = MathF.PI / 18, MaxValue = MathF.PI * 2 };
		public readonly BindableFloat RadiusBindable = new( 1.6f ) { MinValue = 0.1f, MaxValue = 100 };

		public CurvedPanel () {
			ArcBindable.ValueChanged += _ => IsMeshInvalidated = true;
			RadiusBindable.ValueChanged += _ => IsMeshInvalidated = true;
		}

		protected override void RecalculateMesh () {
			IsMeshInvalidated = false;
			Mesh = new() {
				IsReady = false
			};

			var arc = (float)Arc;
			var radius = (float)Radius;

			var points = 100;
			var arclength = arc * radius;
			var height = arclength / ( (float)MainTexture.Width / MainTexture.Height );
			for ( var i = 0; i < points; i++ ) {
				var start = arc / points * i - arc / 2;
				var end = arc / points * ( i + 1 ) - arc / 2;

				var posA = new Vector2( MathF.Sin( end ), MathF.Cos( end ) ) * radius;
				var posB = new Vector2( MathF.Sin( start ), MathF.Cos( start ) ) * radius;

				Mesh.AddQuad( new Quad(
					new Vector3( posB.X, height / 2, posB.Y ), new Vector3( posA.X, height / 2, posA.Y ),
					new Vector3( posB.X, -height / 2, posB.Y ), new Vector3( posA.X, -height / 2, posA.Y )
				), new Vector2( (float)i / points, 1 ), new Vector2( (float)( i + 1 ) / points, 1 ), new Vector2( (float)i / points, 0 ), new Vector2( (float)( i + 1 ) / points, 0 ) );
			}
			Mesh.IsReady = true;
		}
	}
}
