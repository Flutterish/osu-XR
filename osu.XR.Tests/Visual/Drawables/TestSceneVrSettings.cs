using osu.Framework.Graphics.Containers;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Menu.Settings;
using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Tests.Visual.Drawables;

public class TestSceneVrSettings : OsuTestScene {
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
		sections.Presets.EditPresetRequested += edit;
		presets.EditPresetRequested += edit;
		void edit ( ConfigurationPreset<OsuXrSetting> preset ) {
			sections.PresetContainer.SelectedPresetBindable.Value = preset;
			presets.Preset = preset;

			sections.PresetContainer.IsEditingBindable.Value = true;
			presets.PresetContainer.IsEditingBindable.Value = true;
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
		sections.Presets.StopEditingPresetRequested += stopEditing;
		void stopEditing () {
			sections.PresetContainer.IsEditingBindable.Value = false;
			presets.PresetContainer.IsEditingBindable.Value = false;
			sections.PresetContainer.SelectedPresetBindable.Value = null;
		}

		sections.Presets.CreatePresetRequested += () => {
			var preset = new ConfigurationPreset<OsuXrSetting>();
			sections.Presets.List.AddPreset( preset );
			edit( preset );
		};
	}
}
