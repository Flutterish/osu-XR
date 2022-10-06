using osu.Framework.Platform;
using osu.XR.Configuration;

namespace osu.XR;

public class OsuXrGameBase : Framework.Game {
	Storage storage = null!;
	OsuXrConfigManager config = null!;

	DependencyContainer dependencies = null!;
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent )
		=> dependencies = new DependencyContainer( base.CreateChildDependencies( parent ) );

	[BackgroundDependencyLoader]
	private void load ( GameHost host ) {
		storage = host.Storage.GetStorageForDirectory( "XR" );
		dependencies.CacheAs( storage );
		config = new();
		dependencies.CacheAs( config );
	}
}
