using osu.Game.Overlays.Settings;
using System.Collections.Generic;

namespace osu.XR.Inspector {
	/// <summary>
	/// An object whose properties can be inspected
	/// </summary>
	public interface IInspectable { }

	/// <summary>
	/// An object whose properties can be inspected and that has custom inspector subsections
	/// </summary>
	public interface IConfigurableInspectable : IInspectable {
		IEnumerable<SettingsSubsection> CreateInspectorSubsections ();
	}

	/// <summary>
	/// An object that should never be seen in the inspector. Its children will still be visible.
	/// </summary>
	public interface INotInspectable { }

	// TODO IHasInspectorVisuals will be able to render things when selected by the inspector
	public interface IHasInspectorVisuals { }
}
