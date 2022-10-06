using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Panels.Settings;

public class PresetsSettingSection : SettingsSection {
	public override LocalisableString Header => "Presets";
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.BoxOpen
	};

	public readonly ListSubsection List;
	public readonly ManagementSubsection Management;
	public PresetsSettingSection () {
		Add( List = new ListSubsection() );
		Add( Management = new ManagementSubsection() );

		List.EditPresetRequested += p => EditPresetRequested?.Invoke( p );
		List.RequestPresetRemoval += p => RequestPresetRemoval?.Invoke( p );
		List.StopEditingPresetRequested += () => StopEditingPresetRequested?.Invoke();
		Management.CreatePresetRequested += () => CreatePresetRequested?.Invoke();
	}

	public event Action? StopEditingPresetRequested;
	public event Action<ConfigurationPreset<OsuXrSetting>>? EditPresetRequested;
	public event Action<ConfigurationPreset<OsuXrSetting>>? RequestPresetRemoval;
	public event Action? CreatePresetRequested;

	public class ListSubsection : SettingsSubsection {
		protected override LocalisableString Header => "Load";

		[Resolved]
		SettingPresetContainer<OsuXrSetting> presetContainer { get; set; } = null!;

		[Resolved]
		OsuXrConfigManager config { get; set; } = null!;

		protected override void LoadComplete () {
			foreach ( var i in new[] { config.DefaultPreset, config.PresetTouchscreenSmall, config.PresetTouchscreenBig } ) {
				AddPreset( i );
			}
		}

		Dictionary<ConfigurationPreset<OsuXrSetting>, Drawable> buttonsByPreset = new();
		public void RemovePreset ( ConfigurationPreset<OsuXrSetting> preset ) {
			if ( !buttonsByPreset.Remove( preset, out var button ) )
				return;

			Remove( button, true );
		}

		public void AddPreset ( ConfigurationPreset<OsuXrSetting> preset ) {
			if ( buttonsByPreset.ContainsKey( preset ) )
				return;

			SettingsButton main;
			SettingsButton edit;
			SettingsButton delete;
			PresetButtonContainer buttonsContainer;

			Add( buttonsContainer = new PresetButtonContainer {
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
						Action = () => {
							if ( presetContainer.SelectedPresetBindable.Value == preset )
								StopEditingPresetRequested?.Invoke();
							else
								EditPresetRequested?.Invoke( preset );
						}
					},
					delete = new DangerousSettingsButton {
						RelativeSizeAxes = Axes.None,
						Padding = default,
						Action = () => RequestPresetRemoval?.Invoke( preset )
					}
				}
			} );

			buttonsContainer.IsEditingBindable.BindValueChanged( v => main.Enabled.Value = !v.NewValue );
			buttonsContainer.IsEditingBindable.BindTo( presetContainer.IsEditingBindable );
			buttonsContainer.NameBindable.BindValueChanged( v => main.Text = v.NewValue );
			buttonsContainer.NameBindable.BindTo( preset.NameBindable );

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
			// TODO I want the delete button to be "hold to confirm"

			buttonsContainer.OnUpdate += _ => {
				var gap = 10;
				edit.Width = delete.Width = main.DrawHeight;
				main.Width = DrawWidth - buttonsContainer.Padding.TotalHorizontal - delete.Width - edit.Width - gap * 2;
				edit.X = main.Width + gap;
				delete.X = edit.X + edit.Width - edit.Padding.TotalHorizontal + gap;
			};

			buttonsByPreset.Add( preset, buttonsContainer );
		}

		public event Action? StopEditingPresetRequested;
		public event Action<ConfigurationPreset<OsuXrSetting>>? EditPresetRequested;
		public event Action<ConfigurationPreset<OsuXrSetting>>? RequestPresetRemoval;

		class PresetButtonContainer : Container {
			public Bindable<string> NameBindable = new();
			public BindableBool IsEditingBindable = new( false );
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
				Action = () => CreatePresetRequested?.Invoke()
			} );

			presetContainer.IsEditingBindable.BindValueChanged( v => {
				createButton.Enabled.Value = !v.NewValue;
			}, true );
		}

		public event Action? CreatePresetRequested;
	}
}