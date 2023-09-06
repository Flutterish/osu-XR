using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class LabelIcon : CircularContainer {
	Box background;
	OsuSpriteText text;
	public LocalisableString Text {
		get => text.Text;
		set => text.Text = value;
	}

	public LabelIcon () {
		AutoSizeAxes = Axes.Both;
		Masking = true;
		Add( background = new() {
			RelativeSizeAxes = Axes.Both
		} );
		Add( text = new() {
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			UseFullGlyphHeight = false,
			Margin = new( 4 ),
			Font = OsuFont.GetFont( weight: FontWeight.SemiBold )
		} );
	}

	[BackgroundDependencyLoader]
	private void load ( OverlayColourProvider colours ) {
		background.Colour = colours.Colour0;
		text.Colour = colours.Background3;
	}
}
