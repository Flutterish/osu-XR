using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.XR.Inspector.Editors {
	public class Vector2Editor : ValueEditor<Vector2> {
		OsuTextBox textboxX;
		OsuTextBox textboxY;
		public Vector2Editor ( Vector2 defaultValue = default ) : base( defaultValue ) {
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

			textboxX.Current.Value = Current.Value.X.ToString();
			textboxY.Current.Value = Current.Value.Y.ToString();

			textboxX.Current.ValueChanged += _ => textChanged();
			textboxY.Current.ValueChanged += _ => textChanged();
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

			if ( ok ) {
				Current.Value = new Vector2( x, y );
			}
		}
	}
}
