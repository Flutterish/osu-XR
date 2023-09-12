using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Sceneries.Components;

public interface IConfigurableSceneryComponent : ISceneryComponent {
	SceneryComponentSettingsSection CreateSettings ();
}

public partial class SceneryComponentSettingsSection : SettingsSection {
	public SceneryComponentSettingsSection ( ISceneryComponent source ) {
		Header = source.Name;
		OnLoadComplete += _ => Add( new DangerousSettingsButton {
			Text = @"Remove",
			Action = () => RemoveRequested?.Invoke()
		} );
	}

	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Cube
	};

	public override LocalisableString Header { get; }
	public event Action? RemoveRequested;
}