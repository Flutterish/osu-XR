using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Editors {
	public class Vector3Editor : ValueEditor<Vector3> {
		OsuTextBox textboxX;
		OsuTextBox textboxY;
		OsuTextBox textboxZ;
		public Vector3Editor ( Vector3 defaultValue = default ) : base( defaultValue ) {
			Add( textboxX = new OsuTextBox {
				Margin = new MarginPadding { Horizontal = 15 },
				PlaceholderText = "X"
			} );
			textboxX.OnUpdate += _ => textboxX.Width = DrawWidth - 30;
			Add( textboxY = new OsuTextBox {
				Margin = new MarginPadding { Horizontal = 15 },
				PlaceholderText = "Y"
			} );
			textboxY.OnUpdate += _ => textboxY.Width = DrawWidth - 30;
			Add( textboxZ = new OsuTextBox {
				Margin = new MarginPadding { Horizontal = 15 },
				PlaceholderText = "Z"
			} );
			textboxY.OnUpdate += _ => textboxZ.Width = DrawWidth - 30;

			textboxX.Current.Value = Current.Value.X.ToString();
			textboxY.Current.Value = Current.Value.Y.ToString();
			textboxZ.Current.Value = Current.Value.Z.ToString();

			textboxX.Current.ValueChanged += _ => textChanged();
			textboxY.Current.ValueChanged += _ => textChanged();
			textboxZ.Current.ValueChanged += _ => textChanged();
		}

		private void textChanged () {
			bool ok = true;

			if ( !float.TryParse( textboxX.Current.Value, out var x ) ) {
				textboxX.FadeColour( Color4.Red, 200 );
				ok = false;
			}
			else {
				textboxX.FadeColour( Color4.White, 200 );
			}

			if ( !float.TryParse( textboxY.Current.Value, out var y ) ) {
				textboxY.FadeColour( Color4.Red, 200 );
				ok = false;
			}
			else {
				textboxY.FadeColour( Color4.White, 200 );
			}

			if ( !float.TryParse( textboxZ.Current.Value, out var z ) ) {
				textboxZ.FadeColour( Color4.Red, 200 );
				ok = false;
			}
			else {
				textboxZ.FadeColour( Color4.White, 200 );
			}

			if ( ok ) {
				Current.Value = new Vector3( x, y, z );
			}
		}
	}
}
