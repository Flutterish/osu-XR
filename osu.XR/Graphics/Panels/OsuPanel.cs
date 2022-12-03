using osu.Framework.XR.Graphics.Panels;
using osu.XR.Allocation;
using osu.XR.Osu;

namespace osu.XR.Graphics.Panels;

/// <summary>
/// A panel whose content can resolve osu dependencies
/// </summary>
public partial class OsuPanel : Panel {
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		return base.CreateChildDependencies( new MergedDepencencyContainer( parent.Get<OsuDependencies>(), parent ) );
	}
}
