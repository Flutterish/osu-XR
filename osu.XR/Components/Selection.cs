using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.XR.Inspector;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class Selection : Model, INotInspectable {
		public Selection () {
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
