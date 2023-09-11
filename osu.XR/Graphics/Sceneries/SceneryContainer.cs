using osu.Framework.XR.Graphics;
using osu.XR.Configuration;
using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public partial class SceneryContainer : CompositeDrawable3D {
	Bindable<SceneryType> type = new();
	Scenery scenery = new();

	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( OsuXrConfigManager config ) {
		if ( config is null )
			return;

		config.BindWith( OsuXrSetting.SceneryType, type );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		AddInternal( scenery );

		type.BindValueChanged( v => {
			while ( scenery.Components.Any() ) {
				var last = scenery.Components[^1];
				scenery.Components.RemoveAt( scenery.Components.Count - 1 );
				last.Dispose(); // TODO option to not unload them
			}

			scenery.Components.AddRange( v.NewValue switch { 
				SceneryType.Solid => GridScenery.CreateComponents(),
				SceneryType.LightsOut => LightsOutScenery.CreateComponents(),
				_ => Array.Empty<ISceneryComponent>()
			} );
		}, true );
	}
}
