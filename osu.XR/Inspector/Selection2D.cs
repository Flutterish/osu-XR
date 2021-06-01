using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector {
	public class Selection2D : Box, INotInspectable {
		public Selection2D () {
			Colour = Colour4.HotPink;
			Alpha = 0.3f;
		}

		Drawable selected;
		public Drawable Selected {
			get => selected;
			set => Select( value );
		}
		public void Select ( Drawable drawable ) {
			if ( selected == drawable ) return;

			selected = drawable;

			( Parent as Container )?.Remove( this );
			if ( selected is null ) return;
			Container container = null;
			while ( drawable.Parent is not null and not Drawable3D ) {
				drawable = drawable.Parent;
				if ( drawable is Container c && ( IsLoaded || c.Dependencies.Get( typeof( ShaderManager ) ) is not null ) ) container = c;
			}

			container?.Add( this );
			this.FlashColour( Colour4.White, 200, Easing.In );
			this.FadeTo( 0.6f ).FadeTo( 0.3f, 200, Easing.In );
		}

		public override bool ReceivePositionalInputAt ( Vector2 screenSpacePos )
			=> false;

		protected override Quad ComputeScreenSpaceDrawQuad ()
			=> selected?.ScreenSpaceDrawQuad ?? base.ComputeScreenSpaceDrawQuad();
	}
}
