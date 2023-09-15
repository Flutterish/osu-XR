using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.XR;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Configuration.Presets;

namespace osu.XR.Graphics.Settings;

public partial class PresetsSettingSection : SettingsSection {
	public override LocalisableString Header => Localisation.Config.PresetsStrings.Header;
	public override Drawable CreateIcon () => new SpriteIcon {
		Icon = FontAwesome.Solid.BoxOpen
	};

	public readonly ListSubsection List;
	public readonly ManagementSubsection Management;
	public PresetsSettingSection () {
		Add( List = new ListSubsection() );
		Add( Management = new ManagementSubsection() );
	}

	public partial class ListSubsection : SettingsSubsection {
		protected override LocalisableString Header => Localisation.Config.PresetsStrings.LoadHeader;

		[Resolved]
		ConfigPresetSource<OsuXrSetting> presetContainer { get; set; } = null!;

		[Resolved]
		OsuXrConfigManager config { get; set; } = null!;

		protected override void LoadComplete () {
			presets.BindTo( presetContainer.Presets );
			presets.BindCollectionChanged( (_, e) => {
				if ( e.OldItems != null ) {
					foreach ( ConfigPreset<OsuXrSetting> i in e.OldItems ) {
						removePreset( i );
					}
				}
				if ( e.NewItems != null ) {
					foreach ( ConfigPreset<OsuXrSetting> i in e.NewItems ) {
						addPreset( i );
					}
				}
			}, true );
		}

		BindableList<ConfigPreset<OsuXrSetting>> presets = new();
		Dictionary<ConfigPreset<OsuXrSetting>, Drawable> buttonsByPreset = new();
		void removePreset ( ConfigPreset<OsuXrSetting> preset ) {
			if ( !buttonsByPreset.Remove( preset, out var button ) )
				return;

			Remove( button, true );
		}

		void addPreset ( ConfigPreset<OsuXrSetting> preset ) {
			if ( buttonsByPreset.ContainsKey( preset ) )
				return;

			SettingsButton main;
			SettingsButton edit;
			PresetButtonContainer buttonsContainer;

			Add( buttonsContainer = new PresetButtonContainer {
				RelativeSizeAxes = Axes.X,
				Padding = new() { Horizontal = 20 },
				AutoSizeAxes = Axes.Y,
				Children = new[] {
					main = new SettingsButton {
						Text = preset.Name.Value,
						RelativeSizeAxes = Axes.None,
						Padding = default,
						Action = () => {
							if ( !presetContainer.IsSlideoutEnabled.Value )
								config.ApplyPreset( preset );
						}
					},
					edit = new SettingsButton {
						RelativeSizeAxes = Axes.None,
						Padding = default,
						Action = () => {
							if ( presetContainer.SelectedPreset.Value == preset ) {
								presetContainer.SelectedPreset.Value = null;
								presetContainer.IsSlideoutEnabled.Value = false;
							}
							else
								presetContainer.SelectedPreset.Value = preset;
						}
					}
				}
			} );

			buttonsContainer.IsEditingBindable.BindTo( presetContainer.IsSlideoutEnabled );
			buttonsContainer.PresetBindable.BindTo( presetContainer.SelectedPreset );
			(buttonsContainer.PresetBindable, buttonsContainer.IsEditingBindable).BindValuesChanged( (p, e) => main.Enabled.Value = (p is null && !e) || p == preset, true );
			buttonsContainer.NameBindable.BindValueChanged( v => main.Text = v.NewValue );
			buttonsContainer.NameBindable.BindTo( preset.Name );

			edit.Add( new SpriteIcon {
				RelativeSizeAxes = Axes.Both,
				Size = new( 0.5f ),
				Icon = FontAwesome.Solid.PaintBrush,
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre
			} );

			buttonsContainer.OnUpdate += _ => {
				var gap = 10;
				edit.Width = main.DrawHeight;
				main.Width = DrawWidth - buttonsContainer.Padding.TotalHorizontal - edit.Width - gap;
				edit.X = main.Width + gap;
			};

			buttonsByPreset.Add( preset, buttonsContainer );
		}

		partial class PresetButtonContainer : Container {
			public Bindable<string> NameBindable = new();
			public Bindable<ConfigPreset<OsuXrSetting>?> PresetBindable = new();
			public BindableBool IsEditingBindable = new();
		}
	}

	public partial class ManagementSubsection : SettingsSubsection {
		protected override LocalisableString Header => Localisation.Config.PresetsStrings.ManageHeader;

		[Resolved]
		ConfigPresetSource<OsuXrSetting> presetContainer { get; set; } = null!;

		[BackgroundDependencyLoader]
		private void load ( OsuXrConfigManager config ) {
			SettingsButton createButton;
			Add( createButton = new SettingsButton {
				Text = Localisation.Config.Presets.ManageStrings.New,
				TooltipText = Localisation.Config.Presets.ManageStrings.NewTooltip,
				Action = () => {
					var preset = config.CreateFullPreset();
					preset.Name.Value = @"New Preset"; // TODO this from localisation
					presetContainer.Presets.Add( preset );
					presetContainer.IsSlideoutEnabled.Value = false;
					presetContainer.SelectedPreset.Value = preset;
				}
			} );

			presetContainer.IsSlideoutEnabled.BindValueChanged( v => {
				createButton.Enabled.Value = !v.NewValue;
			}, true );
		}
	}
}