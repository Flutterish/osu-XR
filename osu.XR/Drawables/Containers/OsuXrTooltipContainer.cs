using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.XR.Drawables.Containers {
	public class OsuXrTooltipContainer : TooltipContainer {
        protected override ITooltip CreateTooltip () => new OsuXrTooltip();

        public OsuXrTooltipContainer ( CursorContainer cursor )
            : base( cursor ) {
        }

        protected override double AppearDelay => ( 1 - CurrentTooltip.Alpha ) * base.AppearDelay; // reduce appear delay if the tooltip is already partly visible.

        public class OsuXrTooltip : Tooltip {
            private readonly Box background;
            private readonly OsuTextFlowContainer text;
            LocalisableString current;
            private bool instantMovement = true;

			public override void SetContent ( LocalisableString contentString ) {
                if ( contentString == current ) return;

                current = contentString;
                text.Text = current.ToString();

                if ( IsPresent ) {
                    AutoSizeDuration = 250;
                    background.FlashColour( OsuColour.Gray( 0.4f ), 1000, Easing.OutQuint );
                }
                else
                    AutoSizeDuration = 0;

                return;
            }

            public OsuXrTooltip () {
                AutoSizeEasing = Easing.OutQuint;
                BypassAutoSizeAxes = Axes.Both;

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
