using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class JoystickPiece : Container {
	readonly BindableWithCurrent<Vector2> current = new();
	public Bindable<Vector2> JoystickPosition {
		get => current;
		set => current.Current = value;
	}

	CircularContainer outside = null!;
	CircularContainer inside = null!;

	[Resolved]
	protected OverlayColourProvider Colours { get; private set; } = null!;

	protected override Container<Drawable> Content => outside;

	protected override void LoadComplete () {
		base.LoadComplete();

		AddInternal( new Circle {
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			RelativeSizeAxes = Axes.Both,
			Size = new Vector2( 20 / 320f ),
			Colour = Colours.Highlight1
		} );

		CreateInteractiveElements();

		AddInternal( outside = new CircularContainer {
			Masking = true,
			BorderThickness = 10,
			BorderColour = Colours.Highlight1,
			RelativeSizeAxes = Axes.Both,
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			Child = new Box {
				RelativeSizeAxes = Axes.Both,
				Alpha = 0,
				AlwaysPresent = true
			}
		} );
		AddInternal( inside = new CircularContainer {
			Masking = true,
			BorderThickness = 5,
			BorderColour = Colours.Highlight1,
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			RelativePositionAxes = Axes.Both,
			Child = new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = Colours.Background5
			}
		} );

		JoystickPosition.BindValueChanged( v => {
			inside.MoveTo( v.NewValue / 2, 50, Easing.Out );
		}, true );
	}

	protected virtual void CreateInteractiveElements () { }

	protected override void Update () {
		base.Update();

		outside.BorderThickness = 10 * DrawSize.X / 320;
		inside.BorderThickness = 5 * DrawSize.X / 320;
		inside.Size = DrawSize * 74f / 320 + new Vector2( inside.BorderThickness );
	}
}
