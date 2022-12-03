using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Settings;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Menu.Settings;

public partial class PresetPreview : SettingsPanel.SectionsContainer {
	[Cached]
	public readonly SettingPresetContainer<OsuXrSetting> PresetContainer = new();

	public readonly Bindable<ConfigurationPreset<OsuXrSetting>?> PresetBindable = new();
	public ConfigurationPreset<OsuXrSetting>? Preset {
		get => PresetBindable.Value;
		set => PresetBindable.Value = value;
	}

	public PresetPreview ( bool showSidebar ) : base( showSidebar ) {
		PresetContainer.SelectedPresetBindable.BindTo( PresetBindable );
		PresetContainer.IsPreviewBindable.Value = true;
		settings = new();
	}


	PresetSettingsSection settings;
	protected override IEnumerable<SettingsSection> CreateSections () {
		yield return settings;
		yield return new InputSettingSection();
		yield return new GraphicsSettingSection();
	}

	protected override Drawable CreateHeader ()
		=> new SettingsHeader( "Preset Preview", "" );

	partial class PresetSettingsSection : SettingsSection {
		[Resolved]
		SettingPresetContainer<OsuXrSetting> presetContainer { get; set; } = null!;

		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Cog };

		public override LocalisableString Header => "Preset settings";

		Bindable<ConfigurationPreset<OsuXrSetting>?> presetBindable = new();
		readonly BindableWithCurrent<bool> isEditingBindable = new();

		[BackgroundDependencyLoader]
		private void load () {
			presetBindable.BindTo( presetContainer.SelectedPresetBindable );
			isEditingBindable.BindTo( presetContainer.IsEditingBindable );

			presetBindable.BindValueChanged( v => {
				if ( v.OldValue is ConfigurationPreset<OsuXrSetting> old ) {
					old.SaveDefaults();
				}

				Clear();
				if ( v.NewValue is not ConfigurationPreset<OsuXrSetting> preset )
					return;

				Add( new SettingsTextBox {
					LabelText = "Title",
					Current = preset.NameBindable
				} );
				Add( new SettingsButton {
					Text = "Toggle adding/removing items",
					Action = () => {
						isEditingBindable.Value = !isEditingBindable.Value;
					}
				} );
				Add( new SettingsButton {
					Text = "Save",
					Action = preset.SaveDefaults
				} );
				Add( new SettingsButton {
					Text = "Clone",
					Action = () => {
						var clone = preset.Clone();
						clone.Name = $"{preset.Name} (Copy)";
						presetContainer.Presets.Add( clone );
						presetContainer.IsEditingBindable.Value = false;
						presetContainer.SelectedPresetBindable.Value = clone;
					}
				} );
				Add( new SettingsButton {
					Text = "Revert defauls",
					Action = preset.RevertToDefault
				} );
				Add( new DangerousSettingsButton {
					Text = "Delete",
					Action = () => {
						presetContainer.Presets.Remove( preset );
						presetBindable.Value = null;
						isEditingBindable.Value = false;
					}
				} );
			} );
		}
	}
}
