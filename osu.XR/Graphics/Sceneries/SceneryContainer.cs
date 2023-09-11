using osu.Framework.XR.Graphics;
using osu.XR.Configuration;
using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public partial class SceneryContainer : CompositeDrawable3D {
	Bindable<SceneryType> type = new();
	public readonly Scenery Scenery = new();

	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( OsuXrConfigManager config ) {
		if ( config is null )
			return;

		config.BindWith( OsuXrSetting.SceneryType, type );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		AddInternal( Scenery );

		type.BindValueChanged( v => {
			while ( Scenery.Components.Any() ) {
				var last = Scenery.Components[^1];
				Scenery.Components.RemoveAt( Scenery.Components.Count - 1 );
				last.Dispose(); // TODO option to not unload them
			}

			Scenery.Components.AddRange( v.NewValue switch { 
				SceneryType.Solid => GridScenery.CreateComponents(),
				SceneryType.LightsOut => LightsOutScenery.CreateComponents(),
				_ => Array.Empty<ISceneryComponent>()
			} );
		}, true );
	}
}
