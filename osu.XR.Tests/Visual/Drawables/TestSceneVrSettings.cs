using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.XR.Configuration;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Settings;

namespace osu.XR.Tests.Visual.Drawables;

public partial class TestSceneVrSettings : OsuTestScene {
	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config ) {
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

		sections.PresetContainer.Presets.BindTo( config.Presets );

		presets.PresetContainer.Presets.BindTo( sections.PresetContainer.Presets );
		presets.PresetContainer.IsSlideoutEnabled.BindTo( sections.PresetContainer.IsSlideoutEnabled );
		presets.PresetContainer.SelectedPreset.BindTo( sections.PresetContainer.SelectedPreset );
	}
}
