namespace osu.XR.Allocation;

public class MergedDepencencyContainer : IReadOnlyDependencyContainer {
	IReadOnlyDependencyContainer[] sources;
	public MergedDepencencyContainer ( params IReadOnlyDependencyContainer[] sources ) {
		this.sources = sources;
	}

	public object? Get ( Type type )
		=> sources.Select( x => x.Get( type ) ).FirstOrDefault( x => x != null );

	public object? Get ( Type type, CacheInfo info )
		=> sources.Select( x => x.Get( type, info ) ).FirstOrDefault( x => x != null );

	DependencyContainer? injector;
	public void Inject<T> ( T instance ) where T : class, IDependencyInjectionCandidate {
		( injector ??= new( this ) ).Inject( instance );
	}
}
