using osu.Framework.Bindables;
using osu.Framework.XR.Components;
using System;

namespace osu.XR.Components.Panels {
	/// <summary>
	/// A curved 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public class CurvedPanel : InteractivePanel {
		public float Arc { get => ArcBindable.Value; set => ArcBindable.Value = value; }
		public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
		public readonly BindableFloat ArcBindable = new( MathF.PI * 1.2f ) { MinValue = MathF.PI / 18, MaxValue = MathF.PI * 2 };
		public readonly BindableFloat RadiusBindable = new( 1.6f ) { MinValue = 0.1f, MaxValue = 100 };

		public CurvedPanel () {
			ArcBindable.ValueChanged += _ => IsMeshInvalidated = true;
			RadiusBindable.ValueChanged += _ => IsMeshInvalidated = true;
		}

		protected override void RecalculateMesh () {
			Mesh.Clear();
			Panel.Shapes.MakeCurved( Mesh, (float)MainTexture.Width / MainTexture.Height, (float)Arc, (float)Radius, 100 );
		}
	}
}
