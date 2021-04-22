using Humanizer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.XR.Components;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR {
	public class SceneWithMirrorWarning : Scene {
		TextFlowContainer text;
		SpriteIcon iconA;
		SpriteIcon iconB;

		public SceneWithMirrorWarning () {
			Add( new Container {
				Children = new Drawable[] {
					text = new TextFlowContainer( s => s.Font = OsuFont.GetFont( Typeface.Torus, 30 ) ) {
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,
						TextAnchor = Anchor.Centre,
						AutoSizeAxes = Axes.Both
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

			text.AddText( "Warning", s => s.Font = OsuFont.GetFont( Typeface.Torus, 40, FontWeight.Bold ) );
			text.AddParagraph( "Screen mirroring is turned " );
			text.AddText( "off", s => { s.Font = OsuFont.GetFont( Typeface.Torus, 30, FontWeight.Bold ); s.Colour = Colour4.HotPink; } );
			text.AddParagraph( "You can enable it in XrSettings" );
			text.AddParagraph( "If the screen jitters, alt-enter a few times", s => { s.Font = OsuFont.GetFont( Typeface.Torus, 20 ); s.Colour = new Colour4( 255, 255, 255, 128 ); } );

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
