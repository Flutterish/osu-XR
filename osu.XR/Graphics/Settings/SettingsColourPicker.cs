using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Settings;

public partial class SettingsColourPicker : SettingsItem<Colour4> {
	protected override Drawable CreateControl () {
		return new OsuHSVColourPicker() {
			Width = 1,
			RelativeSizeAxes = Axes.X
		};
	}
}
