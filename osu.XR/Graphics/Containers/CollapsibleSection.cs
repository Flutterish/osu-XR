using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.XR.Graphics.Containers;

public partial class CollapsibleSection : Container {
	public LocalisableString Header {
		get => header.Text;
		set => header.Text = value;
	}

	FillFlowContainer content;
	protected override Container<Drawable> Content => content;

	Box background;
	OsuSpriteText header;
	OsuAnimatedButton button;
	Container border;
	public CollapsibleSection () {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		Margin = new() { Vertical = 10 };
		Padding = new() { Horizontal = 20 };
		AddInternal( border = new Container {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y,
			Masking = true,
			CornerRadius = 8,
			BorderThickness = 6,
			Children = new Drawable[] {
				new Box {
					RelativeSizeAxes = Axes.Both,
					Alpha = 0,
					AlwaysPresent = true
				},
				new Container {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Child = new FillFlowContainer {
						Direction = FillDirection.Vertical,
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Children = new Drawable[] {
							button = new StopScaling {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y
							},
							content = new FillFlowContainer {
								Direction = FillDirection.Horizontal,
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Padding = new( 10 ) { Bottom = 20 }
							}
						}
					}
				}
			}
		} );

		button.AddRange( new Drawable[] {
			background = new Box {
				Depth = 1,
				RelativeSizeAxes = Axes.Both
			},
			header = new OsuSpriteText() { Margin = new( 10 ), Depth = 1 },
			new SpriteIcon {
				Depth = 1,
				Icon = FontAwesome.Solid.ChevronDown,
				Origin = Anchor.CentreRight,
				Anchor = Anchor.CentreRight,
				Size = new(16),
				Margin = new( 10 )
			}
		} );

		button.Action = IsExpanded.Toggle;
		IsExpanded.BindValueChanged( v => {
			const float duration = 160;
			const Easing easing = Easing.Out;
			if ( v.NewValue ) {
				content.ResizeHeightTo( content.Height );
				content.AutoSizeDuration = duration;
				content.AutoSizeEasing = easing;
				content.AutoSizeAxes = Axes.Y;
				content.Delay( duration ).Then().Schedule( () => {
					content.AutoSizeDuration = 0;
				} );
			}
			else {
				content.AutoSizeAxes = Axes.None;
				content.ResizeHeightTo( 0, duration, easing );
			}
		}, true );
		FinishTransforms( true );
	}

	public readonly BindableBool IsExpanded = new( false );
	public bool Expanded {
		get => IsExpanded.Value;
		set => IsExpanded.Value = value;
	}

	[BackgroundDependencyLoader]
	private void load ( OverlayColourProvider colours ) {
		background.Colour = colours.Background5;
		border.BorderColour = colours.Background5;
	}

	partial class StopScaling : OsuAnimatedButton {
		protected override void Update () {
			base.Update();
			Content.ScaleTo( 1 );
		}
	}
}
