using osu.Framework.XR.Graphics.Materials;
using osu.XR.Configuration;
using osu.XR.Input;
using osu.XR.IO;
using osu.XR.Osu;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics.Panels;

public partial class OsuGamePanel : CurvedPanel {
	public readonly OsuGameContainer GameContainer;
	public OsuDependencies OsuDependencies => GameContainer.OsuDependencies;

	[Resolved]
	Bindable<BindingsFile> bindings { get; set; } = null!;

	public OsuGamePanel () {
		RenderStage = RenderingStage.Transparent;
		Content.Add( GameContainer = new() );

		HeightBindable.BindValueChanged( v => Y = v.NewValue, true );
		ResolutionXBindable.BindValueChanged( v => ContentSize = ContentSize with { X = v.NewValue }, true );
		ResolutionYBindable.BindValueChanged( v => ContentSize = ContentSize with { Y = v.NewValue }, true );

		OsuDependencies.Player.BindValueChanged( v => {
			var info = v.NewValue;
			if ( info is null )
				return;

			var ruleset = bindings.Value.GetOrAdd( new( info.DrawableRuleset.Ruleset.ShortName ) );
			var variant = ruleset.GetOrAdd( new( info.Variant ) );

			info.InputManager.Add( new InjectedInput( info, variant ) );
		} );
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
