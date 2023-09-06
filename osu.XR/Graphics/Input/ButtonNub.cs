using osu.Framework.Graphics.Shapes;

namespace osu.XR.Graphics.Input;

public partial class ButtonNub : CircularContainer {
	BindableWithCurrent<bool> value = new();
	public Bindable<bool> Current {
		get => value;
		set => this.value.Current = value;
	}

	Box fill;
	public ButtonNub () {
		Masking = true;
		BorderThickness = 3;
		BorderColour = Colour4.White;
		AddInternal( fill = new() {
			RelativeSizeAxes = Axes.Both,
			AlwaysPresent = true
		} );

		Current.BindValueChanged( v => {
			fill.Alpha = v.NewValue ? 1 : 0;
		}, true );
	}
}
