using osu.Framework.XR.Graphics;
using osu.XR.Configuration;
using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public partial class SceneryContainer : CompositeDrawable3D {
	public readonly Scenery Scenery = new();

	[BackgroundDependencyLoader]
	private void load () {
		AddInternal( Scenery );
		LoadPreset( SceneryType.Solid );
	}

	public void LoadPreset ( SceneryType preset ) {
		while ( Scenery.Components.Any() ) {
			var last = Scenery.Components[^1];
			Scenery.Components.RemoveAt( Scenery.Components.Count - 1 );
			last.Dispose(); // TODO option to not unload them
		}

		Scenery.Components.AddRange( preset switch {
			SceneryType.Solid => GridScenery.CreateComponents(),
			SceneryType.LightsOut => LightsOutScenery.CreateComponents(),
			_ => Array.Empty<ISceneryComponent>()
		} );
	}
}
