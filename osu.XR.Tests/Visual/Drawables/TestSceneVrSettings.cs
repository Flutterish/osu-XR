using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Menu.Settings;
using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Tests.Visual.Drawables;

public class TestSceneVrSettings : OsuTestScene {
	[Resolved]
	OsuXrConfigManager config { get; set; } = null!;

	public TestSceneVrSettings () {
		PresetPreview presets;
		VrSettingsPanel.Sections sections;

		Add( new Container {
			RelativeSizeAxes = Axes.Y,
			Width = 400,
			Masking = true,
			Child = sections = new VrSettingsPanel.Sections( false )
		} );
		Add( new Container {
			RelativeSizeAxes = Axes.Y,
			Width = 400,
			Origin = Anchor.TopRight,
			Anchor = Anchor.TopRight,
			Masking = true,
			Child = presets = new PresetPreview( false )
		} );

		// TODO bindable list add/delete operations instead
		presets.EditPresetRequested += edit;
		void edit ( ConfigurationPreset<OsuXrSetting> preset ) {
			sections.PresetContainer.SelectedPresetBindable.Value = preset;
			presets.Preset = preset;

			sections.PresetContainer.IsEditingBindable.Value = true;
			presets.PresetContainer.IsEditingBindable.Value = true;
		}

		sections.Presets.EditPresetRequested += select;
		void select ( ConfigurationPreset<OsuXrSetting> preset ) {
			sections.PresetContainer.SelectedPresetBindable.Value = preset;
			presets.Preset = preset;
		}

		presets.RequestPresetRemoval += deleted;
		void deleted ( ConfigurationPreset<OsuXrSetting> preset ) {
			if ( sections.PresetContainer.SelectedPresetBindable.Value == preset ) {
				sections.PresetContainer.IsEditingBindable.Value = false;
				sections.PresetContainer.SelectedPresetBindable.Value = null;
			}

			if ( presets.Preset == preset )
				presets.Preset = null;

			if ( preset != null )
				sections.Presets.List.RemovePreset( preset );
		}
		
		presets.StopEditingPresetRequested += stopEditing;
		void stopEditing () {
			sections.PresetContainer.IsEditingBindable.Value = false;
			presets.PresetContainer.IsEditingBindable.Value = false;
		}

		sections.Presets.StopEditingPresetRequested += deselect;
		void deselect () {
			sections.PresetContainer.IsEditingBindable.Value = false;
			presets.PresetContainer.IsEditingBindable.Value = false;
			sections.PresetContainer.SelectedPresetBindable.Value = null;
			presets.PresetContainer.SelectedPresetBindable.Value = null;
		}

		sections.Presets.CreatePresetRequested += () => {
			var preset = config.CreateFullPreset();
			preset.Name = "New Preset";
			sections.Presets.List.AddPreset( preset );
			presets.Preset = preset;
		};
	}
}
