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