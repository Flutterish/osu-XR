using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables.Containers {
	public class OsuXrTooltipContainer : OsuTooltipContainer {
		public OsuXrTooltipContainer ( CursorContainer cursor = null ) : base( cursor ) { }

		protected override ITooltip CreateTooltip () => new OsuXrTooltip();

        public class OsuXrTooltip : Tooltip { // fork of the OsuTooltip that uses multiline spritetext
            private readonly Box background;
            private readonly OsuTextFlowContainer text;
            private string current = string.Empty;
            private bool instantMovement = true;

            public override bool SetContent ( object content ) {
                if ( content is not string contentString )
                    return false;

                if ( contentString == current ) return true;

                text.Text = current = contentString;

                if ( IsPresent ) {
                    AutoSizeDuration = 250;
                    background.FlashColour( OsuColour.Gray( 0.4f ), 1000, Easing.OutQuint );
                }
                else
                    AutoSizeDuration = 0;

                return true;
            }

            public OsuXrTooltip () {
                BypassAutoSizeAxes = Axes.Both;
                AutoSizeEasing = Easing.OutQuint;

                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity( 40 ),
                    Radius = 5,
                };
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.9f,
                    },
                    text = new OsuTextFlowContainer(f => f.Font = OsuFont.GetFont(weight: FontWeight.Regular))
                    {
                        Padding = new MarginPadding(5),
                        AutoSizeAxes = Axes.Both
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load ( OsuColour colour ) {
                background.Colour = colour.Gray3;
            }

            protected override void PopIn () {
                instantMovement |= !IsPresent;
                this.FadeIn( 500, Easing.OutQuint );
            }

            protected override void PopOut () => this.Delay( 150 ).FadeOut( 500, Easing.OutQuint );

            public override void Move ( Vector2 pos ) {
                if ( instantMovement ) {
                    Position = pos;
                    instantMovement = false;
                }
                else {
                    this.MoveTo( pos, 200, Easing.OutQuint );
                }
            }
        }
    }
}
