using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.XR.Graphics;

public partial class NotRenderingWarning : Container {
	public NotRenderingWarning () {
		RelativeSizeAxes = Axes.Both;

		OsuTextFlowContainer text;
		Add( new Container {
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			AutoSizeAxes = Axes.Both,
			Children = new Drawable[] {
				text = new() {
					TextAnchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					AutoSizeAxes = Axes.Both,
					ParagraphSpacing = 1
				},
				new SpriteIcon {
					Origin = Anchor.CentreLeft,
					Anchor = Anchor.CentreRight,
					Icon = FontAwesome.Solid.ExclamationTriangle,
					Colour = Color4.Yellow,
					Size = new( 40 ),
					Position = new( 16, 10 )
				},
				 new SpriteIcon {
					Origin = Anchor.CentreRight,
					Anchor = Anchor.CentreLeft,
					Icon = FontAwesome.Solid.ExclamationTriangle,
					Colour = Color4.Yellow,
					Size = new( 40 ),
					Position = new( -16, 10 )
				}
			}
		} );

		const float size = 24;
		text.AddParagraph( @"Warning", f => f.Font = OsuFont.GetFont( size: size * 1.2f, weight: FontWeight.Bold ) );
		text.AddParagraph( @"Screen mirroring is turned ", f => f.Font = OsuFont.GetFont( size: size ) );
		text.AddText( @"off", f => {
			f.Font = OsuFont.GetFont( size: size, weight: FontWeight.Bold );
			f.Colour = Color4.HotPink;
		} );
		text.NewLine();

		text.AddText( @"Press ", f => f.Font = OsuFont.GetFont( size: size ) );
		text.AddText( @"F10", f => {
			f.Font = OsuFont.GetFont( size: size, weight: FontWeight.Bold );
		} );
		text.AddText( @" to open settings", f => f.Font = OsuFont.GetFont( size: size ) );
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		return true;
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		return true;
	}
}
