using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using System;

namespace osu.XR.Inspector.Editors {
	public class TextfieldEditor<T> : ValueEditor<T> {
		OsuTextBox textbox;
		Func<string, (bool couldParse, T result)> parser;

		public TextfieldEditor ( Func<string, (bool couldParse, T result)> parser, T defaultValue = default ) : base( defaultValue ) {
			this.parser = parser;
			Add( textbox = new OsuTextBox {
				Margin = new MarginPadding { Horizontal = 15 }
			} );
			textbox.OnUpdate += _ => textbox.Width = DrawWidth - 30;

			textbox.Current.Value = Current.Value?.ToString() ?? "";
			textbox.Current.ValueChanged += textChanged;
		}

		private void textChanged ( Framework.Bindables.ValueChangedEvent<string> v ) {
			var (couldParse, result) = parser( v.NewValue );
			if ( couldParse ) {
				Current.Value = result;
				textbox.FadeColour( Color4.White, 200 );
			}
			else {
				textbox.FadeColour( Color4.Red, 200 );
			}
		}

		protected override void HandleException ( Exception e ) {
			base.HandleException( e );
			textbox.FadeColour( Color4.Red, 200 );
		}
	}
}
