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
		// TODO presets section
	}

	protected override Drawable CreateHeader ()
		=> new SettingsHeader( "VR Settings", "change the way osu!XR behaves" );
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
				Current = config.GetBindable<InputMode>( OsuXrSetting.InputMode ), 
				TooltipText = "How your controllers interact with the panels" 
			},
			new ConditionalSettingsContainer<InputMode> {
				Current = config.GetBindable<InputMode>( OsuXrSetting.InputMode ),
				[InputMode.SinglePointer] = new Drawable[] {
					new SettingsCheckbox { 
						LabelText = "[Single pointer] Emulate touch", 
						Current = config.GetBindable<bool>( OsuXrSetting.SinglePointerTouch ), 
						TooltipText = "Emulate touch instead of mouse" 
					},
				},
				[InputMode.TouchScreen] = new Drawable[] {
					new SettingsCheckbox { 
						LabelText = "[Touchscreen] Tap only on press", 
						Current = config.GetBindable<bool>( OsuXrSetting.TapOnPress ), 
						TooltipText = "Press a button to tap the screen" 
					}
				}
			},
			new SettingsSlider<int, PxSliderBar> { 
				LabelText = "Touch deadzone", 
				Current = config.GetBindable<int>( OsuXrSetting.Deadzone ), 
				TooltipText = "Deadzone after interacting with a panel"
			},
			new SettingsEnumDropdown<Hand> { LabelText = "Dominant hand", Current = config.GetBindable<Hand>( OsuXrSetting.DominantHand ) },
			new SettingsCheckbox { LabelText = "Disable teleporting", Current = config.GetBindable<bool>( OsuXrSetting.DisableTeleport ) },
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
				LabelText = "Screen arc",
				Current = config.GetBindable<float>( OsuXrSetting.ScreenArc )
			},
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = "Screen radius",
				Current = config.GetBindable<float>( OsuXrSetting.ScreenRadius )
			},
			new SettingsSlider<float,MetersSliderBar> { 
				LabelText = "Screen height",
				Current = config.GetBindable<float>( OsuXrSetting.ScreenHeight )
			},

			new SettingsSlider<int,PxSliderBar> { 
				LabelText = "Screen resolution X",
				Current = config.GetBindable<int>( OsuXrSetting.ScreenResolutionX )
			},
			new SettingsSlider<int,PxSliderBar> {
				LabelText = "Screen resolution Y",
				Current = config.GetBindable<int>( OsuXrSetting.ScreenResolutionY ) 
			},

			new SettingsEnumDropdown<FeetSymbols> { 
				LabelText = "Shadow type", 
				Current = config.GetBindable<FeetSymbols>( OsuXrSetting.ShadowType )
			}
		};
		// TODO computer interaction - render to screen, either vr view or custom camera
	}
}