using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public class VrSettingsPanel : SettingsPanel {
	protected override IEnumerable<SettingsSection> CreateSections () {
		yield return new InputSettingSection();
		yield return new GraphicsSettingSection();
		yield return new PresetsSettingSection();
	}

	protected override SectionsContainer CreateSectionsContainer () {
		return new Sections( false, this );
	}

	protected override Drawable CreateHeader ()
		=> new SettingsHeader( "VR Settings", "change the way osu!XR behaves" );

	public class Sections : SectionsContainer {
		[Cached]
		SettingPresetContainer<OsuXrSetting> presetContainer = new();

		public Sections ( bool showSidebar, SettingsPanel source ) : base( showSidebar, source ) {
		}
	}
}

public class InputSettingSection : SettingsSection {
	public override LocalisableString Header => "Input";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Keyboard
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Children = new Drawable[] {
			new SettingsEnumDropdown<InputMode> { 
				LabelText = "Input mode", 
				TooltipText = "How your controllers interact with the panels" 
			}.PresetComponent( config, OsuXrSetting.InputMode ),
			new ConditionalSettingsContainer<InputMode> {
				Current = config.GetBindable<InputMode>( OsuXrSetting.InputMode ),
				[InputMode.SinglePointer] = new Drawable[] {
					new SettingsCheckbox { 
						LabelText = "[Single pointer] Emulate touch", 
						TooltipText = "Emulate touch instead of mouse" 
					}.PresetComponent( config, OsuXrSetting.SinglePointerTouch ),
				},
				[InputMode.TouchScreen] = new Drawable[] {
					new SettingsCheckbox { 
						LabelText = "[Touchscreen] Tap only on press", 
						TooltipText = "Press a button to tap the screen" 
					}.PresetComponent( config, OsuXrSetting.TapOnPress )
				}
			},
			new SettingsSlider<int, PxSliderBar> { 
				LabelText = "Touch deadzone", 
				TooltipText = "Deadzone after interacting with a panel"
			}.PresetComponent( config, OsuXrSetting.Deadzone ),
			new SettingsEnumDropdown<Hand> { 
				LabelText = "Dominant hand"
			}.PresetComponent( config, OsuXrSetting.DominantHand ),
			new SettingsCheckbox { 
				LabelText = "Disable teleporting"
			}.PresetComponent( config, OsuXrSetting.DisableTeleport ),
		};
	}
}

public class GraphicsSettingSection : SettingsSection {
	public override LocalisableString Header => "Graphics";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.Laptop
	};

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		Children = new Drawable[] {
			new SettingsSlider<float,RadToDegreeSliderBar> {
				LabelText = "Screen arc"
			}.PresetComponent( config, OsuXrSetting.ScreenArc ),
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = "Screen radius"
			}.PresetComponent( config, OsuXrSetting.ScreenRadius ),
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = "Screen height"
			}.PresetComponent( config, OsuXrSetting.ScreenHeight ),

			new SettingsSlider<int,PxSliderBar> { 
				LabelText = "Screen resolution X"
			}.PresetComponent( config, OsuXrSetting.ScreenResolutionX ),
			new SettingsSlider<int,PxSliderBar> {
				LabelText = "Screen resolution Y"
			}.PresetComponent( config, OsuXrSetting.ScreenResolutionY ),

			new SettingsEnumDropdown<FeetSymbols> { 
				LabelText = "Shadow type"
			}.PresetComponent( config, OsuXrSetting.ShadowType )
		};
		// TODO computer interaction - render to screen, either vr view or custom camera
	}
}

public class PresetsSettingSection : SettingsSection {
	public override LocalisableString Header => "Presets";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.BoxOpen
	};

	[Resolved]
	SettingPresetContainer<OsuXrSetting> presetContainer { get; set; } = null!;

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
		//Add( new SettingsButton {
		//	Text = "Toggle Preset creation",
		//	Action = () => presetContainer.IsEditingBindable.Toggle()
		//} );

		foreach ( var i in new[] { config.DefaultPreset, config.PresetTouchscreenSmall, config.PresetTouchscreenBig } ) {
			Add( new SettingsButton {
				Text = i.Name,
				Action = () => config.LoadPreset( i )
			} );
		}
	}
}