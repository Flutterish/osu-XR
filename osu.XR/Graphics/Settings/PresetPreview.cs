using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Configuration.Presets;
using osu.XR.Graphics.Panels.Menu;

namespace osu.XR.Graphics.Settings;

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
		=> new SettingsHeader( Localisation.Config.Presets.PreviewStrings.Header, "" );

	partial class PresetSettingsSection : SettingsSection {
		[Resolved]
		SettingPresetContainer<OsuXrSetting> presetContainer { get; set; } = null!;

		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Cog };

		public override LocalisableString Header => Localisation.Config.Presets.PreviewStrings.Settings;

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
					LabelText = Localisation.Config.Presets.PreviewStrings.PresetName,
					Current = preset.Name
				} );
				Add( new SettingsButton {
					Text = Localisation.Config.Presets.PreviewStrings.ToggleItems,
					Action = () => {
						isEditingBindable.Value = !isEditingBindable.Value;
					}
				} );
				Add( new SettingsButton {
					Text = Localisation.Config.Presets.PreviewStrings.Save,
					Action = preset.SaveDefaults
				} );
				Add( new SettingsButton {
					Text = Localisation.Config.Presets.PreviewStrings.Clone,
					Action = () => {
						var clone = preset.Clone();
						clone.Name.Value = $@"{preset.Name.Value} (Copy)"; // TODO this from localisable string
						presetContainer.Presets.Add( clone );
						presetContainer.IsEditingBindable.Value = false;
						presetContainer.SelectedPresetBindable.Value = clone;
					}
				} );
				Add( new SettingsButton {
					Text = Localisation.Config.Presets.PreviewStrings.Revert,
					Action = preset.RevertToDefault
				} );
				Add( new DangerousSettingsButton {
					Text = Localisation.Config.Presets.PreviewStrings.Delete,
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
