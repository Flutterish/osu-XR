using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.XR.Inspector {
	public class Selection3D : Model, INotInspectable {
		public Selection3D () {
			Mesh = Mesh.FromOBJFile( "./Resources/selection.obj" );
			Tint = Color4.Lime;

			AutoOffsetOrigin = Vector3.Zero;
		}

		Drawable3D selected;
		public Drawable3D Selected {
			get => selected;
			set => Select( value );
		}
		public void Select ( Drawable3D drawable ) {
			Parent = drawable?.Root;

			selected = drawable;
			Transform.SetParent( drawable?.Transform, transformKey );
		}

		protected override void Update () {
			base.Update();

			if ( selected is null ) {
				return;
			}

			Scale = selected.RequiredParentSizeToFit * 1.03f;
			Position = selected.Centre;
		}
	}
}
