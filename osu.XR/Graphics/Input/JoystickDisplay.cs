using osu.Framework.Graphics.Shapes;

namespace osu.XR.Graphics.Input;

public partial class JoystickDisplay : Container {
	readonly BindableWithCurrent<Vector2> current = new();
	public Bindable<Vector2> JoystickPosition {
		get => current;
		set => current.Current = value;
	}

	CircularContainer outside = null!;
	CircularContainer inside = null!;

	protected override void LoadComplete () {
		base.LoadComplete();

		AddInternal( new Circle {
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			RelativeSizeAxes = Axes.Both,
			Size = new Vector2( 40 / 320f ),
			Colour = Colour4.White
		} );

		AddInternal( outside = new CircularContainer {
			Masking = true,
			BorderThickness = 10,
			BorderColour = Colour4.White,
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
			BorderColour = Colour4.White,
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			RelativePositionAxes = Axes.Both,
			Child = new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = Colour4.White
			}
		} );

		JoystickPosition.BindValueChanged( v => {
			inside.Position = new Vector2( v.NewValue.X, -v.NewValue.Y ) / 2;
		}, true );
	}

	protected override void Update () {
		base.Update();

		outside.BorderThickness = 14 * DrawSize.X / 320;
		inside.BorderThickness = 5 * DrawSize.X / 320;
		inside.Size = DrawSize * 110f / 320 + new Vector2( inside.BorderThickness );
	}
}
