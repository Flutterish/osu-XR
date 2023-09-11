using osu.Framework.Localisation;

namespace osu.XR.Graphics.Sceneries.Components;

public interface ISceneryComponent : IDisposable {
	LocalisableString Name { get; }
}
