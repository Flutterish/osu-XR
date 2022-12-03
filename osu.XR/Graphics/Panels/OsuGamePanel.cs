using osu.Framework.XR.Graphics.Materials;
using osu.XR.Configuration;
using osu.XR.Osu;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics.Panels;

public partial class OsuGamePanel : CurvedPanel {
	public readonly OsuGameContainer GameContainer;
	public OsuDependencies OsuDependencies => GameContainer.OsuDependencies;

	public OsuGamePanel () {
		RenderStage = RenderingStage.Transparent;
		Content.Add( GameContainer = new() );

		HeightBindable.BindValueChanged( v => Y = v.NewValue, true );
		ResolutionXBindable.BindValueChanged( v => ContentSize = ContentSize with { X = v.NewValue }, true );
		ResolutionYBindable.BindValueChanged( v => ContentSize = ContentSize with { Y = v.NewValue }, true );
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( MaterialNames.PanelTransparent );
	}

	Bindable<float> HeightBindable = new( 1.8f );
	Bindable<int> ResolutionXBindable = new( 1920 );
	Bindable<int> ResolutionYBindable = new( 1080 );

	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( OsuXrConfigManager? config ) {
		if ( config is null )
			return;

		config.BindWith( OsuXrSetting.ScreenArc, ArcBindable );
		config.BindWith( OsuXrSetting.ScreenRadius, RadiusBindable );
		config.BindWith( OsuXrSetting.ScreenHeight, HeightBindable );

		config.BindWith( OsuXrSetting.ScreenResolutionX, ResolutionXBindable );
		config.BindWith( OsuXrSetting.ScreenResolutionY, ResolutionYBindable );
	}
}
