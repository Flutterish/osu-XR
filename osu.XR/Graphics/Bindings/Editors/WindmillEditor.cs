using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.XR.Input.Actions.Gestures;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class WindmillEditor : FillFlowContainer {
	public WindmillEditor ( WindmillBinding source ) {
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		Add( new OsuTextFlowContainer {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y,
			Text = @"The windmill gesture allows you to use your whole arm/controller as a joystick.",
			Padding = new MarginPadding( 20 ) with { Top = 6 }
		} );

		Add( new SettingsCheckbox {
			LabelText = @"Enable Left",
			Current = source.IsLeftEnabled,
			ShowsDefaultIndicator = false
		} );
		Add( new SettingsCheckbox {
			LabelText = @"Enable Right",
			Current = source.IsRightEnabled,
			ShowsDefaultIndicator = false
		} );

		Add( source.CreateHandler()! );
	}

	protected override bool ShouldBeAlive => true;
}
