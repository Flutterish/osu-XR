using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Settings;

public partial class SettingsColourPicker : SettingsItem<Colour4> {
	protected override Drawable CreateControl () {
		return new NoBackgoundOsuHSVColourPicker() {
			Width = 1,
			RelativeSizeAxes = Axes.X
		};
	}

	partial class NoBackgoundOsuHSVColourPicker : OsuHSVColourPicker {
		[BackgroundDependencyLoader]
		private void load () {
			Background.Alpha = 0;
		}
	}
}
