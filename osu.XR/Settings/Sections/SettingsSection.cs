using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components.Groups;

namespace osu.XR.Settings.Sections {
	public abstract class SettingsSection : FillFlowContainer, IHasName, IHasIcon {
		public SettingsSection () {
			Direction = FillDirection.Vertical;
			AutoSizeAxes = Axes.Y;
			RelativeSizeAxes = Axes.X;
		}

		public abstract string DisplayName { get; }
		public abstract Drawable CreateIcon ();
	}

	public class SettingsSectionContainer : SettingsSection {
		public string Title { get; set; } = "SettingsSection";
		public IconUsage Icon = FontAwesome.Regular.QuestionCircle;

		public override string DisplayName => Title;
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = Icon };
	}
}
