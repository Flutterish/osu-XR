using osu.XR.Allocation;

namespace osu.XR.Osu;

/// <summary>
/// A component whose children can resolve osu dependencies
/// </summary>
public abstract partial class OsuComponent : CompositeComponent {
	[Resolved]
	OsuDependencies dependencies { get; set; } = null!;

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		return base.CreateChildDependencies( new MergedDepencencyContainer( parent.Get<OsuDependencies>(), parent ) );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		dependencies.OsuLoaded += OnDependenciesChanged;
	}

	protected override void Dispose ( bool isDisposing ) {
		dependencies.OsuLoaded -= OnDependenciesChanged;
		base.Dispose( isDisposing );
	}

	protected abstract void OnDependenciesChanged ( OsuDependencies dependencies );
}
