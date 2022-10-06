using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Settings;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Menu.Settings;

public class PresetPreview : SettingsPanel.SectionsContainer {
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
		settings.StopEditingPresetRequested += () => StopEditingPresetRequested?.Invoke();
		settings.RequestPresetRemoval += p => RequestPresetRemoval?.Invoke( p );
		settings.EditPresetRequested += p => EditPresetRequested?.Invoke( p );
	}


	PresetSettingsSection settings;
	protected override IEnumerable<SettingsSection> CreateSections () {
		yield return settings;
		yield return new InputSettingSection();
		yield return new GraphicsSettingSection();
	}

	protected override Drawable CreateHeader ()
		=> new SettingsHeader( "Preset Preview", "" );

	public event Action? StopEditingPresetRequested;
	public event Action<ConfigurationPreset<OsuXrSetting>>? EditPresetRequested;
	public event Action<ConfigurationPreset<OsuXrSetting>>? RequestPresetRemoval;

	class PresetSettingsSection : SettingsSection {
		[Resolved]
		SettingPresetContainer<OsuXrSetting> presetContainer { get; set; } = null!;

		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Cog };

		public override LocalisableString Header => "Preset settings";

		Bindable<ConfigurationPreset<OsuXrSetting>?> presetBindable = new();
		readonly BindableWithCurrent<bool> isEditingBindable = new();

		protected override void LoadComplete () {
			base.LoadComplete();

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
						if ( isEditingBindable.Value )
							StopEditingPresetRequested?.Invoke();
						else
							EditPresetRequested?.Invoke( preset );
					}
				} );
				Add( new SettingsButton {
					Text = "Save",
					Action = preset.SaveDefaults
				} );
				Add( new SettingsButton {
					Text = "Revert defauls",
					Action = preset.RevertToDefault
				} );
				Add( new DangerousSettingsButton {
					Text = "Delete",
					Action = () => {
						RequestPresetRemoval?.Invoke( preset );
					}
				} );
				// TODO clone
			} );
		}

		public event Action? StopEditingPresetRequested;
		public event Action<ConfigurationPreset<OsuXrSetting>>? EditPresetRequested;
		public event Action<ConfigurationPreset<OsuXrSetting>>? RequestPresetRemoval;
	}
}
