using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;

namespace osu.XR.Graphics.Settings;

public partial class OverlaySettings : Game.Overlays.SettingsPanel {
	public OverlaySettings ( bool showSidebar ) : base( showSidebar ) { }

	protected override IEnumerable<SettingsSection> CreateSections () {
		yield return new OverlayGraphicsSettingSection();
	}

	protected override Drawable CreateHeader ()
		=> new SettingsHeader( @"Window Settings", Localisation.ConfigStrings.Flavour );
}

public partial class OverlayGraphicsSettingSection : SettingsSection {
	public override Drawable CreateIcon () {
		return new SpriteIcon { Icon = FontAwesome.Solid.Laptop };
	}

	public override LocalisableString Header => Localisation.Config.GraphicsStrings.Header;

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Add( new SettingsEnumDropdown<CameraMode> {
			LabelText = @"Render to screen",
			Current = config.GetBindable<CameraMode>( OsuXrSetting.CameraMode )
		} );
		Add( new SettingsCheckbox {
			LabelText = @"Input display",
			Current = config.GetBindable<bool>( OsuXrSetting.ShowInputDisplay )
		} );
	}
}

public partial class HiddenButton : Container {
	OsuAnimatedButton button;
	public HiddenButton () {
		Size = new( 100, 1 );
		RelativeSizeAxes = Axes.Y;

		Add( button = new OsuAnimatedButton {
			AutoSizeAxes = Axes.Both,
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre
		} );

		button.Add( new SpriteIcon() {
			Anchor = Anchor.Centre,
			Origin = Anchor.Centre,
			Margin = new( 4 ),
			Size = new( 16 ),
			Icon = FontAwesome.Solid.ChevronRight
		} );

		Alpha = 0;
		AlwaysPresent = true;
	}

	protected override bool OnHover ( HoverEvent e ) {
		this.FadeIn( 300, Easing.Out );
		return true;
	}

	protected override void OnHoverLost ( HoverLostEvent e ) {
		this.FadeOut( 300, Easing.Out );
		base.OnHoverLost( e );
	}

	public Action? Action { set => button.Action = value; }
}