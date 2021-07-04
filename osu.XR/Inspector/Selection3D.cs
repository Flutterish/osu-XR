using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Projection;
using osuTK;
using osuTK.Graphics;

namespace osu.XR.Inspector {
	public class Selection3D : Model, INotInspectable, IRenderedLast {
		public Selection3D () {
			Mesh = Mesh.FromOBJFile( "./Resources/selection.obj" );
			Tint = Color4.Lime;

			AutoOffsetOrigin = Vector3.Zero;
		}

		private BindableFloat animationProgress = new( 1 );
		Drawable3D selected;
		Drawable3D over;
		public Drawable3D Selected {
			get => selected;
			set => Select( value );
		}
		public void Select ( Drawable3D drawable ) {
			if ( selected == drawable ) return;
			
			selected = drawable;
			if ( selected is null ) {
				this.TransformBindableTo( animationProgress, 0, 140, Easing.Out );
			}
			else {
				over = selected;
				animationProgress.Value = 0;
				this.TransformBindableTo( animationProgress, 1, 140, Easing.Out );
				Parent = selected.Root;
				Transform.SetParent( selected.Transform, transformKey );
			}
		}

		protected override void Update () {
			base.Update();

			if ( over is null ) {
				return;
			}

			Scale = over.RequiredParentSizeToFit * 1.03f * ( 1 + ( 1 - animationProgress.Value ) * 0.3f );
			Alpha = animationProgress.Value;
			Position = over.Centre;
		}
	}
}
