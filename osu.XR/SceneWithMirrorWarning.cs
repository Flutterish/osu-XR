using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.XR.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.XR {
	public class SceneWithMirrorWarning : Scene {
		FormatedTextContainer text;
		SpriteIcon iconA;
		SpriteIcon iconB;

		public SceneWithMirrorWarning () {
			Add( new Container {
				Children = new Drawable[] {
					text = new FormatedTextContainer( () => new FontSetings { Size = 30 } ) {
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,
						TextAnchor = Anchor.Centre,
						AutoSizeAxes = Axes.Both,
						Text = "^^**Warning**^^\n" +
						"Screen mirroring is turned ||off||\n" +
						"You can enable it in XrSettings\n" +
						"~~If the screen jitters, alt-enter a few times~~"
					},

					iconA = new SpriteIcon {
						Icon = FontAwesome.Solid.ExclamationTriangle,
						Origin = Anchor.CentreRight,
						Anchor = Anchor.CentreLeft,
						BypassAutoSizeAxes = Axes.Both,
						Size = new Vector2( 50 ),
						Colour = Color4.Yellow,
						Position = new Vector2( -20, 0 )
					},

					iconB = new SpriteIcon {
						Icon = FontAwesome.Solid.ExclamationTriangle,
						Origin = Anchor.CentreLeft,
						Anchor = Anchor.CentreRight,
						BypassAutoSizeAxes = Axes.Both,
						Size = new Vector2( 50 ),
						Colour = Color4.Yellow,
						Position = new Vector2( 20, 0 )
					}
				},
				AutoSizeAxes = Axes.Both,
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre
			} );

			RenderToScreenBindable.BindValueChanged( v => {
				if ( v.NewValue ) {
					text.FadeOut( 1000 );
					iconA.FadeOut( 1000 );
					iconB.FadeOut( 1000 );
				}
				else {
					text.FadeIn( 1000 );
					iconA.FadeIn( 1000 );
					iconB.FadeIn( 1000 );
				}
			}, true );
		}
	}
}
