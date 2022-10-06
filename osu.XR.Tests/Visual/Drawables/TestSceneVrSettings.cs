using osu.Framework.Graphics.Containers;
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

		presets.PresetContainer.IsEditingBindable.Value = true;

		sections.Presets.PresetEdited += preset => {
			sections.PresetContainer.SelectedPresetBindable.Value = preset;
			presets.Preset = preset;
			
			sections.PresetContainer.IsEditingBindable.Value = true;
		};
		sections.Presets.PresetDeleted += preset => {
			if ( sections.PresetContainer.SelectedPresetBindable.Value == preset ) {
				sections.PresetContainer.IsEditingBindable.Value = false;
				sections.PresetContainer.SelectedPresetBindable.Value = null;
			}

			if ( presets.Preset == preset )
				presets.Preset = null;
		};
	}
}
