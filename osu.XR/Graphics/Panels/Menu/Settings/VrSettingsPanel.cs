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

	public PresetsSettingSection () {
		Add( new ListSubsection() );
		Add( new ManagementSubsection() );
	}

	public class ListSubsection : SettingsSubsection {
		protected override LocalisableString Header => "Load";

		[Resolved]
		OsuXrConfigManager config { get; set; } = null!;

		protected override void LoadComplete () {
			foreach ( var i in new[] { config.DefaultPreset, config.PresetTouchscreenSmall, config.PresetTouchscreenBig } ) {
				AddPreset( i );
			}
		}

		public void AddPreset ( ConfigurationPreset<OsuXrSetting> preset ) {
			SettingsButton main;
			SettingsButton edit;
			SettingsButton delete;
			Container buttonsContainer;

			Add( buttonsContainer = new Container {
				RelativeSizeAxes = Axes.X,
				Padding = new() { Horizontal = 20 },
				AutoSizeAxes = Axes.Y,
				Children = new[] {
					main = new SettingsButton {
						Text = preset.Name,
						RelativeSizeAxes = Axes.None,
						Padding = default,
						Action = () => config.LoadPreset( preset ),
					},
					edit = new SettingsButton {
						RelativeSizeAxes = Axes.None,
						Padding = default,
						Action = () => { }
					},
					delete = new DangerousSettingsButton {
						RelativeSizeAxes = Axes.None,
						Padding = default,
						Action = () => { }
					}
				}
			} );

			edit.Add( new SpriteIcon {
				RelativeSizeAxes = Axes.Both,
				Size = new( 0.5f ),
				Icon = FontAwesome.Solid.PaintBrush,
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre
			} );

			delete.Add( new SpriteIcon {
				RelativeSizeAxes = Axes.Both,
				Size = new( 0.5f ),
				Icon = FontAwesome.Solid.Trash,
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre
			} );

			buttonsContainer.OnUpdate += _ => {
				var gap = 10;
				edit.Width = delete.Width = main.DrawHeight;
				main.Width = DrawWidth - buttonsContainer.Padding.TotalHorizontal - delete.Width - edit.Width - gap * 2;
				edit.X = main.Width + gap;
				delete.X = edit.X + edit.Width - edit.Padding.TotalHorizontal + gap;
			};
		}
	}

	public class ManagementSubsection : SettingsSubsection {
		protected override LocalisableString Header => "Manage";

		[Resolved]
		SettingPresetContainer<OsuXrSetting> presetContainer { get; set; } = null!;

		[BackgroundDependencyLoader]
		private void load ( OsuXrConfigManager config ) {
			SettingsButton createButton;
			Add( createButton = new SettingsButton {
				Text = "Create new preset",
				TooltipText = "Slide out the settings to add them to the preset",
				Action = () => presetContainer.IsEditingBindable.Toggle()
			} );

			presetContainer.IsEditingBindable.BindValueChanged( v => {
				createButton.Enabled.Value = !v.NewValue;
			}, true );
		}
	}
}