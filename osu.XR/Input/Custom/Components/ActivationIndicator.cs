using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom.Components {
	public class ActivationIndicator : Circle {
		public readonly BindableBool IsActive = new();
		public ActivationIndicator () {
			Colour = Colour4.HotPink.Darken( 0.9f );
			Size = new osuTK.Vector2( 30, 16 );

			IsActive.BindValueChanged( v => {
				if ( v.NewValue ) {
					this.FadeColour( Colour4.HotPink );
					this.FlashColour( Colour4.White, 200 );
					this.ResizeWidthTo( 45, 100, Easing.Out );
				}
				else {
					this.FadeColour( Colour4.HotPink.Darken( 0.9f ), 200 );
					this.ResizeWidthTo( 30, 100, Easing.Out );
				}
			} );
		}
	}
}
