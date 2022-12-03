namespace osu.XR.Allocation;

public class ExcludingDependencyContainer : IReadOnlyDependencyContainer {
	IReadOnlyDependencyContainer source;
	Func<Type, bool> selector;
	public ExcludingDependencyContainer ( IReadOnlyDependencyContainer source, Func<Type, bool> selector ) {
		this.source = source;
		this.selector = selector;
	}

	public object? Get ( Type type ) {
		if ( selector( type ) )
			return source.Get( type );
		return null;
	}

	public object? Get ( Type type, CacheInfo info ) {
		if ( selector( type ) )
			return source.Get( type, info );
		return null;
	}

	DependencyContainer? injector;
	public void Inject<T> ( T instance ) where T : class, IDependencyInjectionCandidate {
		( injector ??= new( this ) ).Inject( instance );
	}
}
