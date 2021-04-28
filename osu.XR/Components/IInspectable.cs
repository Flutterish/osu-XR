using osu.Framework.XR.Physics;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
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
	/// An object that should never be seen in the inspector
	/// </summary>
	public interface INotInspectable { }

	// TODO IHasInspectorVisuals will be able to render things when selected by the inspector
	public interface IHasInspectorVisuals { }
}
