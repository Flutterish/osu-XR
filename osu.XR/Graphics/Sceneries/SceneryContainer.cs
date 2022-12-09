using osu.Framework.XR.Graphics;
using osu.XR.Configuration;

namespace osu.XR.Graphics.Sceneries;

public partial class SceneryContainer : CompositeDrawable3D {
	Bindable<SceneryType> type = new();

	GridScenery? solid;
	LightsOutScenery? lightsOut;

	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( OsuXrConfigManager config ) {
		if ( config is null )
			return;

		config.BindWith( OsuXrSetting.SceneryType, type );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		type.BindValueChanged( v => {
			ClearInternal( disposeChildren: false );
			AddInternal( v.NewValue switch {
				SceneryType.Solid => solid ??= new(),
				SceneryType.LightsOut or _ => lightsOut ??= new()
			} );
		}, true );
	}
}
