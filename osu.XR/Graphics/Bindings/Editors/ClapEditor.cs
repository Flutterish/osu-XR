using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.XR.Input.Actions;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class ClapEditor : FillFlowContainer {
	Container thresholdBar;
	Drawable fill;
	RulesetActionDropdown dropdown;
	ThresholdBar thresholdA;
	ThresholdBar thresholdB;
	ActivationIndicator indicator;

	public ClapEditor ( ClapBinding source ) {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		InternalChild = new FillFlowContainer {
			Direction = FillDirection.Vertical,
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y,
			Children = new Drawable[] {
				new Container {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Child = indicator = new() {
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre,
						Margin = new MarginPadding { Bottom = 6 }
					}
				},
				thresholdBar = new Container {
					Height = 32,
					Masking = true,
					CornerRadius = 5,
					Margin = new MarginPadding { Left = 16, Right = 16, Bottom = 10 },
					Children = new Drawable[] {
						new Box {
							RelativeSizeAxes = Axes.Both,
							Colour = Colour4.Cyan
						},
						fill = new Box {
							Colour = Colour4.Orange,
							RelativeSizeAxes = Axes.Both
						},
						thresholdA = new(),
						thresholdB = new()
					}
				},
				dropdown = new RulesetActionDropdown()
			}
		};

		dropdown.RulesetAction.BindTo( source.Action );
		thresholdA.Progress.BindTo( source.ThresholdABindable );
		thresholdB.Progress.BindTo( source.ThresholdBBindable );
	}

	protected override void Update () {
		base.Update();
		thresholdBar.Width = thresholdBar.Parent.DrawWidth - 32;
	}

	partial class ThresholdBar : CompositeDrawable {

		[Resolved]
		protected OverlayColourProvider Colours { get; private set; } = null!;

		public readonly BindableDouble Progress = new() { MinValue = 0, MaxValue = 1 };
		Circle child;
		public ThresholdBar () {
			RelativePositionAxes = Axes.X;
			AutoSizeAxes = Axes.Both;
			Anchor = Anchor.CentreLeft;
			Origin = Anchor.Centre;

			AddInternal( child = new Circle {
				Colour = Colour4.HotPink,
				Height = 35,
				Width = 14,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			} );
			AddInternal( new HoverClickSounds() );

			Progress.BindValueChanged( v => {
				this.MoveToX( 1 - (float)v.NewValue, 50 );
			} );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			child.Colour = Colours.Highlight1;
		}

		protected override bool OnHover ( HoverEvent e ) {
			child.ScaleTo( 1.2f, 100, Easing.Out );
			return base.OnHover( e );
		}
		protected override void OnHoverLost ( HoverLostEvent e ) {
			child.ScaleTo( 1, 100, Easing.Out );
			base.OnHoverLost( e );
		}
		protected override bool OnDragStart ( DragStartEvent e ) {
			return true;
		}
		protected override void OnDrag ( DragEvent e ) {
			base.OnDrag( e );
			Progress.Value = 1 - e.MousePosition.X / Parent.DrawWidth;
		}
	}
}
