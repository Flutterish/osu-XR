using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.XR.Graphics.Panels.Menu;

public partial class ThankYouFooter : FillFlowContainer {
	OsuTextFlowContainer text;

	public ThankYouFooter () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		Padding = new MarginPadding {
			Top = 20f,
			Bottom = 30f,
			Horizontal = 20f
		};

		text = new() {
			RelativeSizeAxes= Axes.X,
			AutoSizeAxes = Axes.Y,
			TextAnchor = Anchor.TopCentre
		};
		Add( text );
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		text.AlwaysPresent = true;
		changeSupporter();
	}

	void changeSupporter () {
		text.Clear();

		var supporter = supporters[RNG.Next( supporters.Count )];
		text.AddText( supporter.LeadIn );
		text.AddText( " " );
		text.AddText( supporter.Name, st => {
			st.Colour = Color4.HotPink;
			st.Font = st.Font.With( weight: FontWeight.Bold );
		} );
		text.AddText( " " );
		text.AddArbitraryDrawable( new BeatSyncedFlashingDrawable {
			AutoSizeAxes = Axes.Both,
			Colour = Color4.HotPink,
			Child = new SpriteIcon { Icon = supporter.Icon, Size = new( 14 ) }
		} );
		text.NewLine();
		text.AddText( supporter.Message, st => {
			st.Alpha /= 2;
			st.Font = st.Font.With( size: st.Font.Size * 4 / 5 );
		} );
		text.NewLine();
		text.AddText( "And all " );
		var link = new LinkButton( "Ko-fi", "https://ko-fi.com/perigee" );
		text.AddArbitraryDrawable( link );
		text.AddText( " supporters!" );

		text.FadeIn( 1500, Easing.Out ).Delay( 40_000 ).Then().FadeOut( 1500, Easing.Out ).Then().Schedule( changeSupporter );
	}

	public partial class BeatSyncedFlashingDrawable : BeatSyncedContainer {
		protected override void OnNewBeat ( int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes ) {
			this.FlashColour( Colour4.White, timingPoint.BeatLength * 0.8f, Easing.Out );
		}
	}

	partial class LinkButton : OsuAnimatedButton {
		[Resolved]
		GameHost host { get; set; } = null!;

		[Resolved]
		ToastMessageStack? toasts { get; set; }

		public LinkButton ( string name, string url ) {
			TooltipText = url;
			AutoSizeAxes = Axes.Both;
			Action = () => {
				host.OpenUrlExternally( url );
				toasts?.PostMessage( $@"Opened {url}!" );
			};

			Add( new OsuSpriteText {
				Text = name,
				Colour = Colour4.Cyan
			} );

			Add( new Circle {
				RelativeSizeAxes = Axes.X,
				Height = 2.4f,
				Origin = Anchor.BottomCentre,
				Anchor = Anchor.BottomCentre,
				Colour = Color4.Cyan
			} );
		}

		protected override void Update () {
			base.Update();
			Content.ScaleTo( 1 );
		}
	}

	record FooterMessage ( string Name, string Message ) {
		public string LeadIn { get; init; } = "Osu!XR is made with support of";
		public IconUsage Icon { get; init; } = FontAwesome.Solid.Heart;
	}

	static readonly List<FooterMessage> supporters = new() {
		new FooterMessage( "Peri", "That's me!" ) { Icon = FontAwesome.Solid.Terminal, LeadIn = "Osu!XR would not exist without" },
		new FooterMessage( "Bloom", "A wonderful friend" ) { Icon = FontAwesome.Solid.Terminal },
		new FooterMessage( "Nooraldeen", "A great friend and an even greater mental support" ),
		new FooterMessage( "Mae", "The best girlfriend on the planet. Rest in peace, darling" ) { Icon = FontAwesome.Solid.Cat },
		new FooterMessage( "Ifnis", "The best ex-boyfriend on the planet" ),
		new FooterMessage( "You", "Thanks for playing my game!" ) { Icon = FontAwesome.Solid.Star, LeadIn = "Osu!XR would not be the same without" },
		new FooterMessage( "Peppy", "He made osu! Thank you for inspiring me" ) { Icon = OsuIcon.RulesetOsu, LeadIn = "Osu!XR would not exist if not for" },
		new FooterMessage( "jjbeaniguess", "They made controller bindings for oculus touch (before the rework)" ),
		new FooterMessage( "Nek0ffee", "A Ko-fi supporter" ),
		new FooterMessage( "Valent1", "A Ko-fi supporter who helped squash some bugs" )
	};
}
